namespace LetopiaAgent.Models;

public class Project
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SkillsParcticed { get; set; } = new List<string>();
    public Difficulty Difficulty { get; set; } = Difficulty.Beginner;
    public List<string> Requirements { get; set; } = new List<string>();   
}