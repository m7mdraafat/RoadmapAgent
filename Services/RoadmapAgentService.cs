using System.ClientModel;
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

    public RoadmapAgentService(AgentSettings settings)
    {
        // Load system prompt from file
        _systemPrompt = LoadSystemPrompt();

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

        // Create the agent with tools
        _agent = chatClient.CreateAIAgent(
            instructions: _systemPrompt,
            name: "Letopia - Roadmap Agent"
        );
    }

    /// <summary>
    /// Run the agent with streaming output
    /// </summary>
    public async IAsyncEnumerable<string> RunStreamingAsync(string userInput, AgentThread? thread = null)
    {
        await foreach (var update in _agent.RunStreamingAsync(userInput, thread))
        {
            yield return update.Text;
        }
    }

    /// <summary>
    /// Run the agent and return complete response
    /// </summary>
    public async Task<string> RunAsync(string userInput, AgentThread? thread = null)
    {
        var response = await _agent.RunAsync(userInput, thread);
        return response?.ToString() ?? string.Empty;
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