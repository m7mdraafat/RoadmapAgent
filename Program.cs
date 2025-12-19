using Microsoft.Extensions.Configuration;
using LetopiaAgent.Configuration;
using System;

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

// Test - Print loaded settings (remove in production)
Console.WriteLine($"✅ Configuration loaded successfully");
Console.WriteLine($"   Model: {settings.ModelId}");
Console.WriteLine($"   Endpoint: {settings.GitHubModelsEndpoint}");

var agentService = new RoadmapAgentService(settings);
var thread = agentService.GetNewThread();

// Streaming usage
Console.WriteLine("🤖 Letopia Roadmap Agent:");

await foreach (var output in agentService.RunStreamingAsync("Help me create a roadmap for a new AI-powered project management tool.", thread))
{
    Console.Write(output);
}