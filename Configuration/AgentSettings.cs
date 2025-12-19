
namespace LetopiaAgent.Configuration
{
    public class AgentSettings
    {
        public string GitHubToken { get; set; } = string.Empty;
        public string ModelId { get; set; } = "gpt-4o";
        public string GitHubModelsEndpoint => "https://models.github.ai/inference";
    }
}