using Microsoft.Extensions.Configuration;

namespace LifeOS.Persistence.DatabaseInitializer;

public interface IDbInitializer
{
    Task EnsurePostgreSqlSerilogTableAsync(IConfiguration configuration, CancellationToken cancellationToken = default);
    Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default);
}
