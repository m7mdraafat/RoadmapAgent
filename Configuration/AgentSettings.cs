
namespace LetopiaAgent.Configuration
{
    public class AgentSettings
    {
        public string GitHubToken { get; set; } = string.Empty;
        public string ModelId { get; set; } = "gpt-4o";
        public string GitHubModelsEndpoint => "https://models.github.ai/inference";
        
        /// <summary>
        /// Serper.dev API key for web search (Google Search API)
        /// Get your free API key at: https://serper.dev/
        /// Free tier: 2,500 queries
        /// </summary>
        public string SerperApiKey { get; set; } = string.Empty;
    }
}