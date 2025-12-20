using Microsoft.Extensions.Configuration;
using LetopiaAgent.Configuration;
using Microsoft.Agents.AI;
using System.Text;
using System.Text.Json;
using LetopiaAgent.Tests;

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()          // Loads from user secrets
    .AddEnvironmentVariables()          // Override with env vars if set
    .Build();

// Bind to AgentSettings
var settings = new AgentSettings();
configuration.GetSection("AgentSettings").Bind(settings);

// Validate settings
if (string.IsNullOrEmpty(settings.GitHubToken))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: GitHubToken is not configured!");
    Console.WriteLine("Run: dotnet user-secrets set \"AgentSettings:GitHubToken\" \"your-token\"");
    Console.ResetColor();
    return;
}

// Initialize OpenTelemetry tracing (exports to AI Toolkit at localhost:4318)
TracingConfiguration.Initialize();

// Display welcome banner
PrintWelcomeBanner();

try
{
    var agentService = new RoadmapAgentService(settings);
    var thread = agentService.GetNewThread();
    string lastResponse = string.Empty;

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Agent initialized. successfully!.");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine("What would you like to learn? (e.g., 'Create a frontend development roadmap')");
    Console.WriteLine();

    // Main conversation loop
    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("You: ");
        Console.ResetColor();

        var input = Console.ReadLine()?.Trim();

        // Handle empty input
        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        // Handle commands
        if (input.StartsWith("/"))
        {
            var (handled, newThread) = await HandleCommand(input, agentService, thread, lastResponse);
            thread = newThread;
            if (handled == CommandResult.Exit)
            {
                return;
            }
            if (handled == CommandResult.Handled)
            {
                continue;
            }
        }

        // Process user input with the agent
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("Letopia Agent: ");
        Console.ResetColor();
        Console.WriteLine();


        try
        {
            var responseBuilder = new StringBuilder();
            await foreach (var chunk in agentService.RunStreamingAsync(input, thread))
            {
                Console.Write(chunk);
                responseBuilder.Append(chunk);
            }
            lastResponse = responseBuilder.ToString();
            Console.WriteLine();
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nError: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Fatal Error: {ex.Message}");
    Console.ResetColor();
}
finally
{
    // Shutdown tracing and flush pending traces
    TracingConfiguration.Shutdown();
}

// Helper methods

static void PrintWelcomeBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║           🗺️  Letopia - Roadmap Generation Agent           ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  Commands:                                                 ║");
    Console.WriteLine("║    /new    - Start a new conversation                      ║");
    Console.WriteLine("║    /save   - Save roadmap as Markdown                      ║");
    Console.WriteLine("║    /export - Export roadmap as JSON                        ║");
    Console.WriteLine("║    /help   - Show all commands                             ║");
    Console.WriteLine("║    /quit   - Exit the application                          ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

static void PrintHelp()
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║                      Available Commands                    ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  /new     - Start a fresh conversation                     ║");
    Console.WriteLine("║  /save    - Save last roadmap as Markdown file             ║");
    Console.WriteLine("║  /export  - Export last roadmap as JSON file               ║");
    Console.WriteLine("║  /help    - Show this help message                         ║");
    Console.WriteLine("║  /quit    - Exit the application                           ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║                      Example Prompts                       ║");
    Console.WriteLine("╠════════════════════════════════════════════════════════════╣");
    Console.WriteLine("║  • Create a frontend development roadmap for beginners     ║");
    Console.WriteLine("║  • I want to learn UI/UX design, 10 hours per week         ║");
    Console.WriteLine("║  • Generate a 6-month backend development plan             ║");
    Console.WriteLine("║  • Data science roadmap, I know Python basics              ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

static async Task<(CommandResult, AgentThread)> HandleCommand(string input, RoadmapAgentService agentService, AgentThread thread, string lastResponse)
{
    switch (input.ToLower())
    {
        case "/quit":
        case "/exit":
        case "/q":
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n👋 Goodbye! Happy learning!");
            Console.ResetColor();
            return (CommandResult.Exit, thread);
        
        case "/new":
            thread = agentService.GetNewThread();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n🔄 Started a new conversation!\n");
            Console.ResetColor();
            return (CommandResult.Handled, thread);
        
        case "/help":
            PrintHelp();
            return (CommandResult.Handled, thread);
        
        case "/save":
            if (string.IsNullOrEmpty(lastResponse))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n⚠️  No conversation to save yet. Start chatting with the agent first.\n");
                Console.ResetColor();
                return (CommandResult.Handled, thread);
            }
            var mdFileName = $"Letopia_Roadmap_{DateTime.Now:yyyyMMdd_HHmmss}.md";
            await File.WriteAllTextAsync(mdFileName, lastResponse);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ Conversation saved to {mdFileName}\n");
            Console.ResetColor();
            return (CommandResult.Handled, thread);
        
        case "/export":
            if (string.IsNullOrEmpty(lastResponse))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n⚠️  No conversation to export yet. Start chatting with the agent first.\n");
                Console.ResetColor();
                return (CommandResult.Handled, thread);
            }
            var jsonFileName = $"Letopia_Roadmap_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var exportData = new
            {
                generatedAt = DateTime.Now,
                content = lastResponse
            };
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(exportData, jsonOptions);
            await File.WriteAllTextAsync(jsonFileName, jsonString);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ Conversation exported to {jsonFileName}\n");
            Console.ResetColor();
            return (CommandResult.Handled, thread);
        case "/test":
            Console.Write("Enter number of concurrent requests (default 10): ");
            var countInput = Console.ReadLine();
            var count = int.TryParse(countInput, out var c) ? c : 10;
            await ConcurrencyTest.RunTest(count);
            return (CommandResult.Handled, thread);
        default:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❓ Unknown command. Type /help to see available commands.\n");
            Console.ResetColor();
            return (CommandResult.Handled, thread);
    }
}

enum CommandResult
{
    Handled,
    NotHandled,
    Exit
}