using System.ClientModel;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using OpenAI;
using LetopiaAgent.Configuration;

namespace LetopiaAgent.Tests;

public static class ConcurrencyTest
{
    // Rate limit configuration
    private const int MaxConcurrentRequests = 5;
    private const int RequestsPerMinute = 15;
    private const int BatchSize = 5;
    
    // Calculate delay between batches: 15 req/min = 3 batches of 5 per minute = 20 seconds per batch
    private static readonly TimeSpan BatchDelay = TimeSpan.FromSeconds(60.0 / (RequestsPerMinute / BatchSize));

    public static async Task RunTest(int totalRequests = 10)
    {
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .Build();

        var settings = new AgentSettings();
        configuration.GetSection("AgentSettings").Bind(settings);

        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"  Batch Processing Test: {settings.ModelId}");
        Console.WriteLine($"  Total Requests: {totalRequests}");
        Console.WriteLine($"  Max Concurrent: {MaxConcurrentRequests}");
        Console.WriteLine($"  Rate Limit: {RequestsPerMinute} req/min");
        Console.WriteLine($"  Batch Size: {BatchSize} | Batch Delay: {BatchDelay.TotalSeconds}s");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine();

        var client = new OpenAIClient(
            new ApiKeyCredential(settings.GitHubToken),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(settings.GitHubModelsEndpoint)
            }
        );

        var chatClient = client.GetChatClient(settings.ModelId);

        var stopwatch = Stopwatch.StartNew();
        var results = new List<(int Id, bool Success, string Message, long Ms)>();

        // Use SemaphoreSlim to limit concurrent requests
        using var semaphore = new SemaphoreSlim(MaxConcurrentRequests);
        
        // Track requests for rate limiting
        var requestTimestamps = new List<DateTime>();
        var rateLimitLock = new object();

        // Process requests in batches
        var batches = Enumerable.Range(1, totalRequests)
            .Select((id, index) => new { Id = id, BatchIndex = index / BatchSize })
            .GroupBy(x => x.BatchIndex)
            .Select(g => g.Select(x => x.Id).ToList())
            .ToList();

        Console.WriteLine($"  Processing {batches.Count} batch(es)...");
        Console.WriteLine();

        for (int batchNum = 0; batchNum < batches.Count; batchNum++)
        {
            var batch = batches[batchNum];
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ── Batch {batchNum + 1}/{batches.Count} ({batch.Count} requests) ──");
            Console.ResetColor();

            var batchTasks = batch.Select(async i =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Rate limiting: ensure we don't exceed requests per minute
                    await EnforceRateLimitAsync(requestTimestamps, rateLimitLock);
                    
                    var taskStopwatch = Stopwatch.StartNew();
                    try
                    {
                        var response = await chatClient.CompleteChatAsync($"Say 'Test {i} OK' in exactly 3 words.");
                        taskStopwatch.Stop();

                        lock (results)
                        {
                            results.Add((i, true, "Success", taskStopwatch.ElapsedMilliseconds));
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"    [✓] Request {i}: Success ({taskStopwatch.ElapsedMilliseconds}ms)");
                            Console.ResetColor();
                        }
                    }
                    catch (Exception ex)
                    {
                        taskStopwatch.Stop();
                        
                        // Retry logic for rate limit errors
                        if (ex.Message.Contains("429") || ex.Message.Contains("rate"))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"    [⏳] Request {i}: Rate limited, retrying in 5s...");
                            Console.ResetColor();
                            
                            await Task.Delay(5000);
                            
                            taskStopwatch.Restart();
                            try
                            {
                                var retryResponse = await chatClient.CompleteChatAsync($"Say 'Test {i} OK' in exactly 3 words.");
                                taskStopwatch.Stop();

                                lock (results)
                                {
                                    results.Add((i, true, "Success (retry)", taskStopwatch.ElapsedMilliseconds));
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"    [✓] Request {i}: Success on retry ({taskStopwatch.ElapsedMilliseconds}ms)");
                                    Console.ResetColor();
                                }
                                return;
                            }
                            catch (Exception retryEx)
                            {
                                ex = retryEx;
                            }
                        }

                        var errorMsg = ex.Message.Contains("429") ? "Rate Limited" :
                                      ex.Message.Contains("quota") ? "Quota Exceeded" :
                                      ex.Message.Length > 50 ? ex.Message[..50] + "..." : ex.Message;

                        lock (results)
                        {
                            results.Add((i, false, errorMsg, taskStopwatch.ElapsedMilliseconds));
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"    [✗] Request {i}: {errorMsg} ({taskStopwatch.ElapsedMilliseconds}ms)");
                            Console.ResetColor();
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(batchTasks);

            // Wait between batches (except for the last one)
            if (batchNum < batches.Count - 1)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"    Waiting {BatchDelay.TotalSeconds}s before next batch...");
                Console.ResetColor();
                await Task.Delay(BatchDelay);
            }
        }

        stopwatch.Stop();

        // Print summary
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine("  RESULTS SUMMARY");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        var successful = results.Count(r => r.Success);
        var failed = results.Count(r => !r.Success);
        var avgTime = results.Where(r => r.Success).Select(r => r.Ms).DefaultIfEmpty(0).Average();

        Console.WriteLine($"  Total Requests:    {totalRequests}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Successful:        {successful}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Failed:            {failed}");
        Console.ResetColor();
        Console.WriteLine($"  Success Rate:      {(successful * 100.0 / totalRequests):F1}%");
        Console.WriteLine($"  Total Time:        {stopwatch.ElapsedMilliseconds}ms ({stopwatch.Elapsed.TotalSeconds:F1}s)");
        Console.WriteLine($"  Avg Response Time: {avgTime:F0}ms");
        Console.WriteLine($"  Throughput:        {(totalRequests / stopwatch.Elapsed.TotalMinutes):F1} req/min");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        // Show rate limit errors
        var rateLimited = results.Where(r => r.Message.Contains("Rate") || r.Message.Contains("429")).ToList();
        if (rateLimited.Any())
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ {rateLimited.Count} requests were rate limited");
            Console.ResetColor();
        }

        if (failed == 0)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ All requests completed successfully!");
            Console.ResetColor();
        }
    }

    private static async Task EnforceRateLimitAsync(List<DateTime> timestamps, object lockObj)
    {
        while (true)
        {
            lock (lockObj)
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddMinutes(-1);
                
                // Remove timestamps older than 1 minute
                timestamps.RemoveAll(t => t < windowStart);
                
                // If under the limit, record this request and proceed
                if (timestamps.Count < RequestsPerMinute)
                {
                    timestamps.Add(now);
                    return;
                }
            }
            
            // Wait a bit and check again
            await Task.Delay(1000);
        }
    }
}