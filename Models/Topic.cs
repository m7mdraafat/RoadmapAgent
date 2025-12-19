using System.Reflection.Metadata.Ecma335;

namespace LetopiaAgent.Models;

public class Topic
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Resource> Resources { get; set; } = new List<Resource>();
    public List<string> HandsOnTasks { get; set; } = new List<string>();
    public List<string> KeyConcepts { get; set; } = new List<string>();
}