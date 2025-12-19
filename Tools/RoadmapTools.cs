using System.ComponentModel;

namespace LetopiaAgent.Tools;

public static class RoadmapTools
{
    [Description("Retrieve information about a learning domain including core skills, typical phases, and common tools")]
    public static string GetDomainInfo(
        [Description("The learning domain to get information about, e.g., 'frontend', 'ui-ux', 'backend'")] string domain)
    {
        return string.Empty;
    }

    [Description("Generate a detailed learning phase for a specific domain")]
    public static string GeneratePhase(
        [Description("The name of the learning phase")] string phaseName,
        [Description("The phase number in the learning sequence")] int phaseNumber,
        [Description("The learning domain, e.g., 'frontend', 'ui-ux', 'backend'")] string domain,
        [Description("The difficulty level of the phase, e.g., 'beginner', 'intermediate', 'advanced'")] string difficulty)
    {
        return string.Empty;
    }

    [Description("Suggest learning resources for a specific topic")]
    public static string SuggestResources(
        [Description("The topic to find resources for")] string topic,
        [Description("The type of resource, e.g., 'video', 'article', 'course', 'book'")] string resourceType,
        [Description("The difficulty level, e.g., 'beginner', 'intermediate', 'advanced'")] string difficulty,
        [Description("The number of resources to suggest")] int count)
    {
        return string.Empty;
    }

    [Description("Suggest hands-on projects to practice specific skills")]
    public static string SuggestProjects(
        [Description("The skills to practice through projects")] string skills,
        [Description("The difficulty level, e.g., 'beginner', 'intermediate', 'advanced'")] string difficulty,
        [Description("The learning domain, e.g., 'frontend', 'ui-ux', 'backend'")] string domain)
    {
        return string.Empty;
    }

    [Description("Estimate the time required to learn specific topics based on learner level and available hours")]
    public static string EstimateTime(
        [Description("The topics to learn, comma-separated")] string topics,
        [Description("The current level of the learner, e.g., 'beginner', 'intermediate', 'advanced'")] string learnerLevel,
        [Description("The number of hours per week the learner can dedicate")] int hoursPerWeek)
    {
        return string.Empty;
    }

    [Description("Analyze prerequisites and identify skill gaps for a target learning domain")]
    public static string AnalyzePrerequisites(
        [Description("The target learning domain, e.g., 'frontend', 'ui-ux', 'backend'")] string targetDomain,
        [Description("The current skills the learner already has, comma-separated")] string currentSkills)
    {
        return string.Empty;
    }
}