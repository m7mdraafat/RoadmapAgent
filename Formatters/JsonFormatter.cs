using System.Text.Json;
using System.Text.Json.Serialization;
using LetopiaAgent.Models;

namespace LetopiaAgent.Formatters;

public static class JsonFormatter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serialize roadmap to JSON string
    /// </summary>
    public static string ToJson(Roadmap roadmap)
    {
        return JsonSerializer.Serialize(roadmap, Options);
    }

    /// <summary>
    /// Deserialize JSON string to Roadmap
    /// </summary>
    public static Roadmap? FromJson(string json)
    {
        return JsonSerializer.Deserialize<Roadmap>(json, Options);
    }

    /// <summary>
    /// Save roadmap to JSON file
    /// </summary>
    public static async Task SaveToFileAsync(Roadmap roadmap, string filePath)
    {
        var json = ToJson(roadmap);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Load roadmap from JSON file
    /// </summary>
    public static async Task<Roadmap?> LoadFromFileAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return FromJson(json);
    }
}