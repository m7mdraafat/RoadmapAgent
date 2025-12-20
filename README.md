# Letopia - Roadmap Generation Agent

A .NET console application that generates comprehensive learning roadmaps using Microsoft Agent Framework and GitHub Models.

## Description

Letopia is an AI-powered agent that creates detailed, structured learning roadmaps for any field including:
- Frontend Development
- Backend Development
- UI/UX Design
- Data Science
- DevOps
- And more...

Each roadmap includes phases, learning objectives, resources, hands-on projects, and milestones.

## Prerequisites

- .NET 8.0 SDK or later
- GitHub Personal Access Token (PAT) with models access

## To get GitHub Personal Access Token?
1. Go to: https://github.com/settings/tokens
2. Generate new token (classic)
3. Copy generated token


## Setup

1. Clone the repository

2. Navigate to the project folder:
   ```
   cd LetopiaAgent
   ```

3. Restore packages:
   ```
   dotnet restore
   ```

4. Configure your GitHub token using Terminal:
   ```
   dotnet user-secrets set "AgentSettings:GitHubToken" "your-github-token"
   dotnet user-secrets set "AgentSettings:ModelId" "gpt-4o"
   ```

## Usage

1. Run the application:
   ```
   dotnet run
   ```

2. Enter your request, for example:
   ```
   Create a frontend development roadmap for beginners
   ```

3. Use commands:
   - `/new` - Start a new conversation
   - `/save` - Save the last roadmap as Markdown
   - `/export` - Export the last roadmap as JSON
   - `/help` - Show available commands
   - `/quit` - Exit the application

## Project Structure

```
LetopiaAgent/
├── Configuration/       # App settings and configuration
├── Formatters/          # Markdown and JSON output formatters
├── Models/              # Data models (Roadmap, Phase, Resource, etc.)
├── Prompts/             # System prompt for the agent
├── Services/            # Agent service implementation
├── Tools/               # Agent tools
└── Program.cs           # Application entry point
```

## Technologies

- Microsoft Agent Framework
- OpenAI SDK
- GitHub Models API
- .NET 8.0

