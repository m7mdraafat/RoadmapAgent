using System.ComponentModel;
using System.Text.Json;
using LetopiaAgent.Services;

namespace LetopiaAgent.Tools;

/// <summary>
/// Web search tools for finding and validating learning resource URLs
/// Uses Serper.dev (Google Search) - 2,500 FREE queries!
/// </summary>
public class WebSearchTools
{
    private readonly WebSearchService _searchService;

    public WebSearchTools(string serperApiKey)
    {
        _searchService = new WebSearchService(serperApiKey);
    }

    [Description("Search the web to find the real URL for a learning resource. Returns success=true with a URL if found, or success=false if not found. If success=false, DO NOT include this resource - try a different one instead.")]
    public async Task<string> SearchResourceUrl(
        [Description("The title of the resource to find, e.g., 'ASP.NET Core Fundamentals'")] string resourceTitle,
        [Description("The platform where the resource should be found, e.g., 'YouTube', 'Udemy', 'Microsoft Learn', 'Coursera'")] string platform,
        [Description("The main topic the resource covers, e.g., 'web development', 'C# programming'")] string topic)
    {
        var url = await _searchService.SearchResourceUrlAsync(resourceTitle, platform, topic);
        
        if (string.IsNullOrEmpty(url))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Could not find a verified URL for '{resourceTitle}' on {platform}. DO NOT include this resource in the roadmap. Try searching for a different resource instead.",
                resourceTitle = resourceTitle,
                platform = platform
            });
        }
        
        return JsonSerializer.Serialize(new
        {
            success = true,
            url = url,
            resourceTitle = resourceTitle,
            platform = platform,
            message = "URL verified. You can include this resource in the roadmap."
        });
    }

    [Description("Search the web for general information and return the top result URL and description")]
    public async Task<string> WebSearch(
        [Description("The search query to find information about")] string query)
    {
        var result = await _searchService.SearchAsync(query);
        
        if (result != null)
        {
            return JsonSerializer.Serialize(new
            {
                success = true,
                title = result.Title,
                url = result.Url,
                description = result.Description
            });
        }

        return JsonSerializer.Serialize(new
        {
            success = false,
            message = "No results found for the query"
        });
    }

    [Description("Validate if a URL exists and is accessible. Returns true if the URL is valid.")]
    public async Task<string> ValidateUrl(
        [Description("The URL to validate")] string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await httpClient.SendAsync(request);
            
            return JsonSerializer.Serialize(new
            {
                valid = response.IsSuccessStatusCode,
                statusCode = (int)response.StatusCode,
                url = url
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                valid = false,
                error = ex.Message,
                url = url
            });
        }
    }
}
