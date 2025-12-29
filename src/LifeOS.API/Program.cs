using LifeOS.API.Configuration;
using LifeOS.API.Extensions;
using LifeOS.Application;
using LifeOS.Infrastructure;
using LifeOS.Persistence;
using LifeOS.Persistence.DatabaseInitializer;

var builder = WebApplication.CreateBuilder(args);

// ✅ Kestrel Server Optimizations
builder.ConfigureKestrelServer();

// ✅ Serilog yapılandırmasını yükle
builder.ConfigureSerilog();

// ✅ CORS Policy yapılandırması
builder.Services.AddCorsPolicy(builder.Configuration, out var corsPolicyName);

// ✅ OpenTelemetry yapılandırması - Tracing ve Metrics için
builder.Services.AddOpenTelemetryServices(builder.Configuration, builder.Environment);

// ✅ Katman servisleri
builder.Services.AddConfigurePersistenceServices(builder.Configuration);
builder.Services.AddConfigureApplicationServices(builder.Configuration);
builder.Services.AddConfigureInfrastructureServices(builder.Configuration);

builder.Services.AddOptions();
builder.Services.AddHttpContextAccessor();

// ✅ Response Optimization (Caching & Compression)
builder.Services.AddResponseOptimization();

// ✅ Rate Limiting
builder.Services.AddRateLimiting(builder.Configuration);

// ✅ API Controllers
builder.Services.AddApiControllers();

// ✅ Application Cookie
builder.Services.AddApplicationCookie(builder.Environment);

var app = builder.Build();

// ✅ Static Files & Image Storage
app.UseStaticFilesWithImageStorage();

// ✅ API Documentation (Development only)
app.UseApiDocumentation();

// ✅ Serilog Request Logging
app.UseSerilogRequestLogging();

// ✅ Veritabanı başlatma ve gerekli tabloları oluştur
await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
await dbInitializer.InitializeAsync(scope.ServiceProvider, app.Lifetime.ApplicationStopping);
await dbInitializer.EnsurePostgreSqlSerilogTableAsync(builder.Configuration, app.Lifetime.ApplicationStopping);

// ✅ Middleware Pipeline
app.UseApiMiddleware(corsPolicyName);

await app.RunAsync();

