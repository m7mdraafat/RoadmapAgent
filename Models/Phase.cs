namespace LetopiaAgent.Models;

public class Phase
{
    public int PhaseNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public List<string> LearningObjectives { get; set; } = new List<string>();
    public List<Topic> Topics { get; set; } = new List<Topic>();
    public List<Project> Projects { get; set; } = new List<Project>();
    public List<string> Milestones { get; set; } = new List<string>();
}