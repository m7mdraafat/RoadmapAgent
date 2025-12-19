namespace LetopiaAgent.Models;

public class Roadmap
{
    public string Title { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public List<string> Prerequisites { get; set; } = new List<string>();
    public string TotalDuration { get; set; } = string.Empty;
    public List<Phase> Phases { get; set; } = new List<Phase>();
    public List<string> NextSteps { get; set; } = new List<string>();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}