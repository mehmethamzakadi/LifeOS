using AspNetCoreRateLimit;
using LifeOS.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

namespace LifeOS.API.Extensions;

/// <summary>
/// WebApplication için extension methodlar - Program.cs sadeleştirmesi için
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Static files ve image storage yapılandırmasını ekler
    /// </summary>
    public static WebApplication UseStaticFilesWithImageStorage(this WebApplication app)
    {
        app.UseStaticFiles();

        var imageStorageOptions = app.Services.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
        var imageRootPath = imageStorageOptions.RootPath;

        if (!Path.IsPathRooted(imageRootPath))
        {
            imageRootPath = Path.Combine(app.Environment.ContentRootPath, imageRootPath);
        }

        imageRootPath = Path.GetFullPath(imageRootPath);
        Directory.CreateDirectory(imageRootPath);

        if (Directory.Exists(imageRootPath))
        {
            var requestPathValue = (imageStorageOptions.RequestPath ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(requestPathValue) && !requestPathValue.StartsWith('/'))
            {
                requestPathValue = "/" + requestPathValue;
            }

            requestPathValue = requestPathValue.TrimEnd('/');

            var staticFileOptions = new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imageRootPath),
                RequestPath = string.IsNullOrWhiteSpace(requestPathValue) ? default : new PathString(requestPathValue)
            };

            app.UseStaticFiles(staticFileOptions);
        }

        return app;
    }

    /// <summary>
    /// Development ortamında OpenAPI ve Scalar dokümantasyonunu ekler
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();  // OpenAPI dokümanı için endpoint ekle

            app.MapScalarApiReference(options =>
            {
                options.Title = "LifeOS API";
                options.Theme = ScalarTheme.DeepSpace; // DeepSpace, Light, Solar gibi temalar kullanılabilir
            });
        }

        return app;
    }

    /// <summary>
    /// Serilog request logging'i yapılandırır - Trace ID ile zenginleştirilmiş
    /// </summary>
    public static WebApplication UseSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());

                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserName", httpContext.User.Identity.Name ?? "unknown");
                }
            };
        });

        return app;
    }

    /// <summary>
    /// HTTPS redirection ve HSTS yapılandırmasını ekler (Production için)
    /// </summary>
    public static WebApplication UseHttpsSecurity(this WebApplication app)
    {
        // Production ortamında HTTPS zorunlu
        if (app.Environment.IsProduction())
        {
            // HTTP isteklerini HTTPS'e yönlendir
            app.UseHttpsRedirection();

            // HSTS (HTTP Strict Transport Security) - Production'da aktif
            app.UseHsts();
        }
        // Development ortamında HTTPS redirection yapmıyoruz - HTTP ve HTTPS her ikisi de kullanılabilir

        return app;
    }

    /// <summary>
    /// Security headers middleware'ini ekler
    /// </summary>
    public static WebApplication UseSecurityHeaders(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            // Security headers ekle
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

            // Production'da CSP header'ı ekle (gerekirse özelleştirilebilir)
            if (app.Environment.IsProduction())
            {
                // CSP - Uygulamanıza göre düzenleyin
                // Örnek: default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline';
                context.Response.Headers.Append("Content-Security-Policy", 
                    "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;");
            }

            await next();
        });

        return app;
    }

    /// <summary>
    /// Middleware pipeline'ını yapılandırır
    /// </summary>
    public static WebApplication UseApiMiddleware(this WebApplication app, string corsPolicyName)
    {
        // ✅ HTTPS Security (en başta - diğer middleware'lerden önce)
        app.UseHttpsSecurity();

        // ✅ Security Headers
        app.UseSecurityHeaders();

        app.UseResponseCompression();
        app.UseResponseCaching();

        app.UseMiddleware<Middlewares.ExceptionHandlingMiddleware>();

        app.UseRouting();

        app.UseCors(corsPolicyName); // CORS, endpoint routing sonrası ve auth öncesi konumlandırılmalı

        app.UseIpRateLimiting();

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
