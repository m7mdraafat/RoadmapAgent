namespace LetopiaAgent.Models;

public class Resource
{
    public string Title { get; set; } = string.Empty;
    public ResourceType Type { get; set; } = ResourceType.Documentation;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Difficulty Difficulty { get; set; } = Difficulty.Beginner;
    public string EstimatedTime { get; set; } = string.Empty;

}

public enum ResourceType
{
    Documentation,
    Article,
    Video,
    Course,
    Tool,
    Book
}

public enum Difficulty
{
    Beginner,
    Intermediate,
    Advanced
}