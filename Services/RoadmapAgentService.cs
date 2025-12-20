using System.ClientModel;
using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OpenAI;
using LetopiaAgent.Configuration;
using LetopiaAgent.Tools;
using OpenAI.Chat;

public class RoadmapAgentService
{
    private readonly AIAgent _agent;
    private readonly string _systemPrompt;
    private readonly WebSearchTools? _webSearchTools;

    public RoadmapAgentService(AgentSettings settings)
    {
        // Load system prompt from file
        _systemPrompt = LoadSystemPrompt();

        // Initialize web search tools with Serper.dev API key
        var tools = new List<AITool>();
        
        if (!string.IsNullOrEmpty(settings.SerperApiKey))
        {
            _webSearchTools = new WebSearchTools(settings.SerperApiKey);
            tools.Add(AIFunctionFactory.Create(_webSearchTools.SearchResourceUrl));
            tools.Add(AIFunctionFactory.Create(_webSearchTools.WebSearch));
            tools.Add(AIFunctionFactory.Create(_webSearchTools.ValidateUrl));
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("🔍 Web Search enabled (Serper.dev - Google Search)");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  Web Search disabled - Set SerperApiKey for verified URLs");
            Console.WriteLine("   Run: dotnet user-secrets set \"AgentSettings:SerperApiKey\" \"YOUR_KEY\"");
            Console.ResetColor();
        }

        // Create OpenAI client pointing to GitHub Models
        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.GitHubToken),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(settings.GitHubModelsEndpoint)
            }
        );

        // Get chat client for specific model
        var chatClient = openAIClient.GetChatClient(settings.ModelId);

        // Create the agent with web search tools (if available)
        _agent = chatClient.CreateAIAgent(
            instructions: _systemPrompt,
            name: "Letopia - Roadmap Agent",
            tools: tools.Count > 0 ? tools : null
        );
    }

    /// <summary>
    /// Run the agent with streaming output
    /// </summary>
    public async IAsyncEnumerable<string> RunStreamingAsync(string userInput, AgentThread? thread = null)
    {
        using var activity = TracingConfiguration.ActivitySource.StartActivity("agent.chat", ActivityKind.Client);
        activity?.SetTag("gen_ai.system", "openai");
        activity?.SetTag("gen_ai.operation.name", "chat");
        activity?.SetTag("gen_ai.request.model", "gpt-4o");
        activity?.SetTag("user.input", userInput.Length > 200 ? userInput[..200] + "..." : userInput);

        var tokenCount = 0;
        await foreach (var update in _agent.RunStreamingAsync(userInput, thread))
        {
            tokenCount += update.Text?.Length ?? 0;
            yield return update.Text;
        }

        activity?.SetTag("gen_ai.response.finish_reason", "stop");
        activity?.SetTag("response.characters", tokenCount);
    }

    /// <summary>
    /// Run the agent and return complete response
    /// </summary>
    public async Task<string> RunAsync(string userInput, AgentThread? thread = null)
    {
        using var activity = TracingConfiguration.ActivitySource.StartActivity("agent.chat", ActivityKind.Client);
        activity?.SetTag("gen_ai.system", "openai");
        activity?.SetTag("gen_ai.operation.name", "chat");
        activity?.SetTag("gen_ai.request.model", "gpt-4o");
        activity?.SetTag("user.input", userInput.Length > 200 ? userInput[..200] + "..." : userInput);

        var response = await _agent.RunAsync(userInput, thread);
        var result = response?.ToString() ?? string.Empty;

        activity?.SetTag("gen_ai.response.finish_reason", "stop");
        activity?.SetTag("response.characters", result.Length);
        return result;
    }

    /// <summary>
    /// Create a new converation thread
    /// </summary>
    public AgentThread GetNewThread()
    {
        return _agent.GetNewThread();
    }

    /// <summary>
    /// Load the system prompt from the Prompts folder
    /// </summary>
    private string LoadSystemPrompt()
    {
        var promptPath = Path.Combine(Directory.GetCurrentDirectory(), "Prompts", "SystemPrompt.txt");

        if (!File.Exists(promptPath))
        {
            throw new FileNotFoundException($"System prompt file not found at path: {promptPath}");
        }

        return File.ReadAllText(promptPath);
    }
}