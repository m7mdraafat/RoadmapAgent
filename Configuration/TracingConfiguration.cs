using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace LetopiaAgent.Configuration;

/// <summary>
/// Configures OpenTelemetry tracing for the Letopia Agent
/// Exports traces to AI Toolkit's OTLP endpoint for visualization
/// </summary>
public static class TracingConfiguration
{
    private const string ServiceName = "LetopiaAgent";
    private const string ServiceVersion = "1.0.0";
    
    // AI Toolkit OTLP endpoint (HTTP) - use /v1/traces path
    private const string OtlpEndpoint = "http://localhost:4318/v1/traces";

    private static TracerProvider? _tracerProvider;
    
    // Shared activity source for the entire application
    public static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// Initialize OpenTelemetry tracing with OTLP exporter
    /// </summary>
    public static void Initialize()
    {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = "development",
                        ["service.instance.id"] = Environment.MachineName
                    }))
            .AddSource(ServiceName)  // Listen to our custom activity source
            .AddSource("Azure.*")    // Capture Azure SDK traces
            .AddSource("Microsoft.*") // Capture Microsoft SDK traces  
            .AddSource("OpenAI.*")   // Capture OpenAI SDK traces
            .AddHttpClientInstrumentation(options =>
            {
                // Include all HTTP requests
                options.FilterHttpRequestMessage = (request) => true;
                // Enrich spans with request details
                options.EnrichWithHttpRequestMessage = (activity, request) =>
                {
                    activity.SetTag("http.request.host", request.RequestUri?.Host);
                    activity.SetTag("http.url.full", request.RequestUri?.ToString());
                };
                // Record exception details
                options.RecordException = true;
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(OtlpEndpoint);
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
            })
            .Build();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📊 Tracing enabled → {OtlpEndpoint}");
        Console.ResetColor();
    }

    /// <summary>
    /// Shutdown tracing and flush any pending traces
    /// </summary>
    public static void Shutdown()
    {
        _tracerProvider?.Shutdown();
        _tracerProvider?.Dispose();
    }
}
