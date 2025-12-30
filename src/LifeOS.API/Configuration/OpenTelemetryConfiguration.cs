using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace LifeOS.API.Configuration;

/// <summary>
/// OpenTelemetry yapılandırması - Dağıtık sistem takibi için
/// HTTP Request ve EF Core sorguları için otomatik tracing
/// </summary>
public static class OpenTelemetryConfiguration
{
    public static IServiceCollection AddOpenTelemetryServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var serviceName = "LifeOS.API";
        var serviceVersion = "1.0.0";

        // Resource attributes - servis bilgileri
        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion,
                serviceInstanceId: Environment.MachineName)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment.EnvironmentName
            });

        // Tracing yapılandırması
        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    // HTTP Request tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.method", request.Method);
                            activity.SetTag("http.request.path", request.Path);
                            activity.SetTag("http.request.query_string", request.QueryString.ToString());
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                        };
                    })
                    // EF Core query tracing
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.EnrichWithIDbCommand = (activity, command) =>
                        {
                            activity.SetTag("db.command.text", command.CommandText);
                        };
                    })
                    // HTTP Client tracing (Ollama, external APIs)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.client.request.method", request.Method?.ToString());
                            activity.SetTag("http.client.request.uri", request.RequestUri?.ToString());
                        };
                    })
                    // Console exporter (development için)
                    .AddConsoleExporter();

                // OTLP Exporter - Jaeger veya diğer OTLP uyumlu collector'lara gönderim
                // Environment variable'dan endpoint alınır: OTEL_EXPORTER_OTLP_ENDPOINT
                // Docker ortamında: http://jaeger:4317
                // Local development: http://localhost:4317 (appsettings.json'dan)
                var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] 
                    ?? configuration["OpenTelemetry:Otlp:Endpoint"] 
                    ?? (environment.IsDevelopment() ? "http://localhost:4317" : null);

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracerProviderBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        // Protocol environment variable'dan alınır: OTEL_EXPORTER_OTLP_PROTOCOL (grpc veya http/protobuf)
                        var protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.ToLowerInvariant() ?? "grpc";
                        if (protocol == "http/protobuf" || protocol == "http")
                        {
                            // HTTP protokolü için endpoint'i değiştir (4318 portu)
                            var uriBuilder = new UriBuilder(otlpEndpoint);
                            if (uriBuilder.Port == 4317)
                            {
                                uriBuilder.Port = 4318;
                            }
                            options.Endpoint = uriBuilder.Uri;
                        }
                    });
                }

                // Production'da Prometheus exporter eklenebilir
                // Not: Prometheus exporter için AddPrometheusExporter() yerine
                // app.MapPrometheusScrapingEndpoint() kullanılmalı (Program.cs'de)
                // if (!environment.IsDevelopment())
                // {
                //     tracerProviderBuilder.AddPrometheusExporter();
                // }
            })
            // Metrics yapılandırması
            .WithMetrics(metricsProviderBuilder =>
            {
                metricsProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    // HTTP Request metrics
                    .AddAspNetCoreInstrumentation()
                    // HTTP Client metrics
                    .AddHttpClientInstrumentation()
                    // ✅ Runtime metrics (GC, ThreadPool, etc.)
                    // .NET 9'da runtime metrics built-in olarak geliyor (System.Runtime meter)
                    // OpenTelemetry.Instrumentation.Runtime paketi artık gerekli değil
                    // .AddRuntimeInstrumentation() // .NET 9'da gerekli değil
                    // Console exporter (development için)
                    .AddConsoleExporter();

                // OTLP Exporter - Metrics için de aynı endpoint kullanılır
                var otlpEndpointForMetrics = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] 
                    ?? configuration["OpenTelemetry:Otlp:Endpoint"] 
                    ?? (environment.IsDevelopment() ? "http://localhost:4317" : null);

                if (!string.IsNullOrWhiteSpace(otlpEndpointForMetrics))
                {
                    metricsProviderBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpointForMetrics);
                        var protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.ToLowerInvariant() ?? "grpc";
                        if (protocol == "http/protobuf" || protocol == "http")
                        {
                            var uriBuilder = new UriBuilder(otlpEndpointForMetrics);
                            if (uriBuilder.Port == 4317)
                            {
                                uriBuilder.Port = 4318;
                            }
                            options.Endpoint = uriBuilder.Uri;
                        }
                    });
                }

                // Production'da Prometheus exporter eklenebilir
                // Not: Prometheus exporter için AddPrometheusExporter() yerine
                // app.MapPrometheusScrapingEndpoint() kullanılmalı (Program.cs'de)
                // if (!environment.IsDevelopment())
                // {
                //     metricsProviderBuilder.AddPrometheusExporter();
                // }
            });

        // Serilog ile OpenTelemetry entegrasyonu - Trace ID'yi loglara ekle
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddOpenTelemetry(options =>
            {
                options
                    .SetResourceBuilder(resourceBuilder)
                    .AddConsoleExporter();

                // OTLP Exporter - Logs için de aynı endpoint kullanılır
                var otlpEndpointForLogs = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] 
                    ?? configuration["OpenTelemetry:Otlp:Endpoint"] 
                    ?? (environment.IsDevelopment() ? "http://localhost:4317" : null);

                if (!string.IsNullOrWhiteSpace(otlpEndpointForLogs))
                {
                    options.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(otlpEndpointForLogs);
                        var protocol = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"]?.ToLowerInvariant() ?? "grpc";
                        if (protocol == "http/protobuf" || protocol == "http")
                        {
                            var uriBuilder = new UriBuilder(otlpEndpointForLogs);
                            if (uriBuilder.Port == 4317)
                            {
                                uriBuilder.Port = 4318;
                            }
                            otlpOptions.Endpoint = uriBuilder.Uri;
                        }
                    });
                }
            });
        });

        return services;
    }
}
