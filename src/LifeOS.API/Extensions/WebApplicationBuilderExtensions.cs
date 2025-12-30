using AspNetCoreRateLimit;
using LifeOS.API.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

namespace LifeOS.API.Extensions;

/// <summary>
/// WebApplicationBuilder için extension methodlar - Program.cs sadeleştirmesi için
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Kestrel server optimizasyonlarını yapılandırır
    /// </summary>
    public static WebApplicationBuilder ConfigureKestrelServer(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxConcurrentConnections = 1000;
            serverOptions.Limits.MaxConcurrentUpgradedConnections = 1000;
            serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
        });

        return builder;
    }

    /// <summary>
    /// Response caching ve compression servislerini yapılandırır
    /// </summary>
    public static IServiceCollection AddResponseOptimization(this IServiceCollection services)
    {
        // Response Caching
        services.AddResponseCaching();

        // Response Compression
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        });

        services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        return services;
    }

    /// <summary>
    /// Rate limiting servislerini yapılandırır
    /// </summary>
    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
        services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));
        services.AddInMemoryRateLimiting();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

        return services;
    }

    /// <summary>
    /// CORS policy'lerini yapılandırır
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration, out string policyName)
    {
        policyName = "_dynamicCorsPolicy";
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        if (allowedOrigins.Length == 0)
        {
            throw new InvalidOperationException("En az bir izinli CORS origin yapılandırılmalıdır (Cors:AllowedOrigins).");
        }

        // ✅ out parametresini lambda dışında kullanmak için local değişkene kopyalıyoruz
        var corsPolicyName = policyName;
        services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicyName, policyBuilder =>
            {
                policyBuilder.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // React withCredentials için gerekli
            });
        });

        return services;
    }

    /// <summary>
    /// Minimal API için gerekli servisleri yapılandırır
    /// </summary>
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        // Küçük harfli URL'ler için routing yapılandırması
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = false;
        });

        services.AddEndpointsApiExplorer();

        services.AddOpenApi(options =>
        {
            options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
        });

        return services;
    }
}
