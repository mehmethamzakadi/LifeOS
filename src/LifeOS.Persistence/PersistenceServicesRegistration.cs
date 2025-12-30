using LifeOS.Persistence.Constants;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.DatabaseInitializer;
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
                npgsqlOptions.MaxBatchSize(DatabaseConstants.MaxBatchSize);
                npgsqlOptions.CommandTimeout(DatabaseConstants.CommandTimeoutSeconds);
            });
            
            options.EnableServiceProviderCaching();
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        #endregion

        services.AddScoped<IDbInitializer, DbInitializer>();

        return services;
    }
}
