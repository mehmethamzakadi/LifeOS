using LifeOS.Application.Abstractions;
using LifeOS.Application.Abstractions.Identity;
using LifeOS.Application.Abstractions.Images;
using LifeOS.Domain.Common;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Infrastructure.Authorization;
using LifeOS.Infrastructure.Options;
using LifeOS.Infrastructure.Services;
using LifeOS.Infrastructure.Services.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;
using System.Net;
using System.Text;
using TokenOptions = LifeOS.Application.Options.TokenOptions;

namespace LifeOS.Infrastructure
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenOptions>(configuration.GetSection(TokenOptions.SectionName));
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.Configure<PasswordResetOptions>(configuration.GetSection(PasswordResetOptions.SectionName));
            services.Configure<ImageStorageOptions>(configuration.GetSection(ImageStorageOptions.SectionName));
            services.Configure<Options.OllamaOptions>(configuration.GetSection(Options.OllamaOptions.SectionName));

            // Custom Password Hasher for User entity
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<AspNetCorePasswordHasher>();
            services.AddScoped<Domain.Services.IPasswordHasher>(sp => sp.GetRequiredService<AspNetCorePasswordHasher>());
            services.AddScoped<Application.Abstractions.Identity.IPasswordHasher>(sp => sp.GetRequiredService<AspNetCorePasswordHasher>());

            TokenOptions tokenOptions = configuration.GetSection(TokenOptions.SectionName).Get<TokenOptions>()
                ?? throw new InvalidOperationException("Token ayarları yapılandırılmalıdır.");

            var environment = configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT");
            bool requireHttpsMetadata = !string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecurityKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

            var redisConnectionString = configuration.GetConnectionString("RedisCache");
            IConnectionMultiplexer? connectionMultiplexer = null;

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                // Redis cache için IDistributedCache register et (diğer servisler için gerekli olabilir)
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "LifeOS_";
                });

                // IConnectionMultiplexer'ı register et (RedisCacheService için gerekli)
                connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
                services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            // Background Services
            services.AddHostedService<SessionCleanupService>();

            // RedisCacheService requires IConnectionMultiplexer - only register if Redis is available
            if (connectionMultiplexer != null)
            {
                services.AddSingleton<ICacheService, RedisCacheService>();
            }
            else
            {
                // Fallback: If Redis is not available, you may want to register a MemoryCache-based implementation
                // For now, we'll throw an exception if Redis is required but not available
                // You can implement a fallback ICacheService if needed
                throw new InvalidOperationException(
                    "Redis connection string is required for ICacheService. Please configure RedisCache connection string.");
            }
            services.AddTransient<ITokenService, JwtTokenService>();
            services.AddTransient<IMailService, MailService>();
            
            // IExecutionContextAccessor - Domain.Common için tek implementation
            services.AddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();
            
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IImageStorageService, ImageStorageService>();
            services.AddScoped<IUserDomainService, UserDomainService>();

            // Ollama AI Service - Best practices: IHttpClientFactory + Polly retry policy
            var ollamaOptions = configuration.GetSection(Options.OllamaOptions.SectionName).Get<Options.OllamaOptions>()
                ?? throw new InvalidOperationException("Ollama ayarları yapılandırılmalıdır.");

            services.AddHttpClient("OllamaClient", client =>
            {
                client.BaseAddress = new Uri(ollamaOptions.Endpoint);
                client.Timeout = TimeSpan.FromMinutes(ollamaOptions.TimeoutMinutes);
            })
            .AddPolicyHandler(GetRetryPolicy(ollamaOptions));

            services.AddScoped<IAiService, AiService>();

            // Authorization
            services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddAuthorizationCore(options =>
            {
                // Permission'lar için policy'ler oluştur
                foreach (var permission in Permissions.GetAllPermissions())
                {
                    options.AddPolicy(permission, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            // Register log cleanup background service
            services.AddHostedService<LogCleanupService>();

            return services;
        }

        /// <summary>
        /// Ollama API için retry policy oluşturur.
        /// Best practice: Transient hatalar için exponential backoff retry.
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(Options.OllamaOptions options)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // 5xx ve 408 (Request Timeout) hatalarını yakalar
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests) // 429 Rate Limit
                .WaitAndRetryAsync(
                    retryCount: options.RetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(
                        Math.Pow(2, retryAttempt) * options.RetryDelaySeconds), // Exponential backoff
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Logging için (opsiyonel - ILogger inject edilebilir)
                        // Burada sadece policy tanımlanıyor, logging servis içinde yapılıyor
                    });
        }
    }
}