using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LetopiaAgent.Services;

/// <summary>
/// Service for searching the web using Serper.dev (Google Search API)
/// FREE - 2,500 queries, no credit card required!
/// </summary>
public class WebSearchService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string SerperApiUrl = "https://google.serper.dev/search";

    public WebSearchService(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Search using Serper.dev (Google) and return results
    /// </summary>
    public async Task<SearchResult?> SearchAsync(string query, int maxResults = 3)
    {
        try
        {
            var requestBody = new
            {
                q = query,
                num = maxResults
            };

            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(SerperApiUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Serper] Error: {response.StatusCode} - {errorBody}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<SerperSearchResponse>(json);

            // Try organic results first
            if (searchResponse?.Organic?.Any() == true)
            {
                var firstResult = searchResponse.Organic.First();
                return new SearchResult
                {
                    Title = firstResult.Title ?? "",
                    Url = firstResult.Link ?? "",
                    Description = firstResult.Snippet ?? ""
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Serper] Exception: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Search for a learning resource and return verified URL
    /// Returns empty string if no valid URL found - agent should skip this resource
    /// </summary>
    public async Task<string> SearchResourceUrlAsync(string resourceTitle, string platform, string topic)
    {
        // Construct a targeted search query with site restriction for better results
        var siteFilter = GetSiteFilter(platform);
        var query = string.IsNullOrEmpty(siteFilter) 
            ? $"{resourceTitle} {topic}"
            : $"{resourceTitle} {topic} site:{siteFilter}";
        
        var result = await SearchAsync(query);
        
        if (result != null && IsValidUrl(result.Url, platform))
        {
            return result.Url;
        }

        // Fallback: search with just title and platform name
        query = string.IsNullOrEmpty(siteFilter)
            ? $"{resourceTitle} {platform}"
            : $"{resourceTitle} site:{siteFilter}";
            
        result = await SearchAsync(query);
        
        if (result != null && IsValidUrl(result.Url, platform))
        {
            return result.Url;
        }

        // NO FALLBACK - return empty string so agent knows to skip this resource
        return string.Empty;
    }

    /// <summary>
    /// Get site filter for Google search based on platform
    /// </summary>
    private string GetSiteFilter(string platform)
    {
        return platform.ToLowerInvariant() switch
        {
            "youtube" => "youtube.com",
            "microsoft learn" => "learn.microsoft.com",
            "udemy" => "udemy.com",
            "coursera" => "coursera.org",
            "freecodecamp" => "freecodecamp.org",
            "pluralsight" => "pluralsight.com",
            "dev.to" => "dev.to",
            "medium" => "medium.com",
            "mdn web docs" => "developer.mozilla.org",
            "w3schools" => "w3schools.com",
            "github" => "github.com",
            "edx" => "edx.org",
            "codecademy" => "codecademy.com",
            "linkedin learning" => "linkedin.com/learning",
            "stackoverflow" => "stackoverflow.com",
            _ => ""
        };
    }

    /// <summary>
    /// Validate that the URL matches the expected platform
    /// </summary>
    private bool IsValidUrl(string url, string platform)
    {
        if (string.IsNullOrEmpty(url)) return false;
        
        var platformDomains = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["YouTube"] = new[] { "youtube.com", "youtu.be" },
            ["Microsoft Learn"] = new[] { "learn.microsoft.com", "docs.microsoft.com" },
            ["Udemy"] = new[] { "udemy.com" },
            ["Coursera"] = new[] { "coursera.org" },
            ["freeCodeCamp"] = new[] { "freecodecamp.org" },
            ["Pluralsight"] = new[] { "pluralsight.com" },
            ["Dev.to"] = new[] { "dev.to" },
            ["Medium"] = new[] { "medium.com" },
            ["MDN Web Docs"] = new[] { "developer.mozilla.org" },
            ["W3Schools"] = new[] { "w3schools.com" },
            ["GitHub"] = new[] { "github.com" },
            ["edX"] = new[] { "edx.org" },
            ["Codecademy"] = new[] { "codecademy.com" },
            ["LinkedIn Learning"] = new[] { "linkedin.com/learning" }
        };

        if (platformDomains.TryGetValue(platform, out var domains))
        {
            return domains.Any(domain => url.Contains(domain, StringComparison.OrdinalIgnoreCase));
        }

        // If platform not in list, accept any URL
        return true;
    }
}

#region Serper.dev API Response Models

public class SerperSearchResponse
{
    [JsonPropertyName("organic")]
    public List<SerperOrganicResult>? Organic { get; set; }
    
    [JsonPropertyName("knowledgeGraph")]
    public SerperKnowledgeGraph? KnowledgeGraph { get; set; }
}

public class SerperOrganicResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("link")]
    public string? Link { get; set; }
    
    [JsonPropertyName("snippet")]
    public string? Snippet { get; set; }
}

public class SerperKnowledgeGraph
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }
    
    [JsonPropertyName("website")]
    public string? Website { get; set; }
}

public class SearchResult
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

#endregion
