using LifeOS.Domain.Common;
using LifeOS.Domain.Repositories;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.DatabaseInitializer;
using LifeOS.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LifeOS.Persistence;

public static class PersistenceServicesRegistration
{
    public static IServiceCollection AddConfigurePersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        #region DbContext Yapılandırması
        var postgreSqlConnectionString = configuration.GetConnectionString("LifeOSPostgreConnectionString");

        services.AddDbContext<LifeOSDbContext>((sp, options) =>
        {
            options.UseNpgsql(postgreSqlConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MaxBatchSize(100);
                npgsqlOptions.CommandTimeout(30);
            });
            // ✅ Default Tracking (EF Core default) - Update/Delete işlemleri için gerekli
            // Read-only sorgularda .AsNoTracking() kullanılmalı (performans için)
            // NoTrackingWithIdentityResolution sadece özel durumlar için kullanılır
            // options.UseQueryTrackingBehavior(QueryTrackingBehavior.Tracking); // Default, açıkça belirtmeye gerek yok
            options.EnableServiceProviderCaching();
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        #endregion

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IImageRepository, ImageRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRefreshSessionRepository, RefreshSessionRepository>();
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IMovieSeriesRepository, MovieSeriesRepository>();
        services.AddScoped<IPersonalNoteRepository, PersonalNoteRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        services.AddScoped<IDbInitializer, DbInitializer>();

        // Unit of Work kaydı
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
