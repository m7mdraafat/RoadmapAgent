using System.Text;
using LetopiaAgent.Models;

namespace LetopiaAgent.Formatters;

public static class MarkdownFormatter
{
    /// <summary>
    /// Format a complete roadmap as Markdown
    /// </summary>
    public static string FormatRoadmap(Roadmap roadmap)
    {
        var sb = new StringBuilder();

        // Title and Overview
        sb.AppendLine($"# 🗺️ {roadmap.Title}");
        sb.AppendLine();
        sb.AppendLine($"**Domain:** {roadmap.Domain}");
        sb.AppendLine();
        sb.AppendLine($"## Overview");
        sb.AppendLine();
        sb.AppendLine(roadmap.Description);
        sb.AppendLine();
        sb.AppendLine($"- **Target Audience:** {roadmap.TargetAudience}");
        sb.AppendLine($"- **Total Duration:** {roadmap.TotalDuration}");
        sb.AppendLine($"- **Generated:** {roadmap.GeneratedAt:MMMM dd, yyyy}");
        sb.AppendLine();

        // Prerequisites
        if (roadmap.Prerequisites.Any())
        {
            sb.AppendLine("## Prerequisites");
            sb.AppendLine();
            foreach (var prereq in roadmap.Prerequisites)
            {
                sb.AppendLine($"- {prereq}");
            }
            sb.AppendLine();
        }

        // Phases
        sb.AppendLine("---");
        sb.AppendLine();

        foreach (var phase in roadmap.Phases)
        {
            sb.AppendLine(FormatPhase(phase));
        }

        // Next Steps
        if (roadmap.NextSteps.Any())
        {
            sb.AppendLine("## 🚀 Next Steps");
            sb.AppendLine();
            foreach (var step in roadmap.NextSteps)
            {
                sb.AppendLine($"- {step}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format a single phase as Markdown
    /// </summary>
    public static string FormatPhase(Phase phase)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"## Phase {phase.PhaseNumber}: {phase.Title}");
        sb.AppendLine();
        sb.AppendLine($"**Duration:** {phase.Duration}");
        sb.AppendLine();

        // Learning Objectives
        if (phase.LearningObjectives.Any())
        {
            sb.AppendLine("### 🎯 Learning Objectives");
            sb.AppendLine();
            foreach (var objective in phase.LearningObjectives)
            {
                sb.AppendLine($"- [ ] {objective}");
            }
            sb.AppendLine();
        }

        // Topics
        if (phase.Topics.Any())
        {
            sb.AppendLine("### 📚 Topics");
            sb.AppendLine();
            foreach (var topic in phase.Topics)
            {
                sb.AppendLine(FormatTopic(topic));
            }
        }

        // Projects
        if (phase.Projects.Any())
        {
            sb.AppendLine("### 🛠️ Projects");
            sb.AppendLine();
            foreach (var project in phase.Projects)
            {
                sb.AppendLine(FormatProject(project));
            }
        }

        // Milestones
        if (phase.Milestones.Any())
        {
            sb.AppendLine("### ✅ Milestones");
            sb.AppendLine();
            foreach (var milestone in phase.Milestones)
            {
                sb.AppendLine($"- [ ] {milestone}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Format a topic with its resources
    /// </summary>
    private static string FormatTopic(Topic topic)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"#### {topic.Name}");
        sb.AppendLine();
        sb.AppendLine(topic.Description);
        sb.AppendLine();

        // Key Concepts
        if (topic.KeyConcepts.Any())
        {
            sb.AppendLine("**Key Concepts:**");
            foreach (var concept in topic.KeyConcepts)
            {
                sb.AppendLine($"- {concept}");
            }
            sb.AppendLine();
        }

        // Resources Table
        if (topic.Resources.Any())
        {
            sb.AppendLine("**Resources:**");
            sb.AppendLine();
            sb.AppendLine("| Type | Title | Difficulty | Time |");
            sb.AppendLine("|------|-------|------------|------|");
            foreach (var resource in topic.Resources)
            {
                var typeEmoji = GetResourceTypeEmoji(resource.Type);
                sb.AppendLine($"| {typeEmoji} {resource.Type} | [{resource.Title}]({resource.Url}) | {resource.Difficulty} | {resource.EstimatedTime} |");
            }
            sb.AppendLine();
        }

        // Hands-on Tasks
        if (topic.HandsOnTasks.Any())
        {
            sb.AppendLine("**Hands-on Tasks:**");
            foreach (var task in topic.HandsOnTasks)
            {
                sb.AppendLine($"- [ ] {task}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Format a project
    /// </summary>
    private static string FormatProject(Project project)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"#### 💻 {project.Name}");
        sb.AppendLine();
        sb.AppendLine(project.Description);
        sb.AppendLine();
        sb.AppendLine($"- **Difficulty:** {project.Difficulty}");
        sb.AppendLine($"- **Estimated Time:** {project.EstimatedTime}");

        if (project.SkillsPracticed.Any())
        {
            sb.AppendLine($"- **Skills:** {string.Join(", ", project.SkillsPracticed)}");
        }

        if (project.Requirements.Any())
        {
            sb.AppendLine("- **Requirements:**");
            foreach (var req in project.Requirements)
            {
                sb.AppendLine($"  - {req}");
            }
        }

        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// Get emoji for resource type
    /// </summary>
    private static string GetResourceTypeEmoji(ResourceType type)
    {
        return type switch
        {
            ResourceType.Documentation => "📖",
            ResourceType.Article => "📄",
            ResourceType.Video => "📺",
            ResourceType.Course => "🎓",
            ResourceType.Tool => "🔧",
            ResourceType.Book => "📚",
            _ => "📌"
        };
    }

    /// <summary>
    /// Save roadmap to a Markdown file
    /// </summary>
    public static async Task SaveToFileAsync(Roadmap roadmap, string filePath)
    {
        var markdown = FormatRoadmap(roadmap);
        await File.WriteAllTextAsync(filePath, markdown);
    }
}