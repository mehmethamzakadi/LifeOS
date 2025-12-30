using LifeOS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LifeOS.Persistence.Contexts;

/// <summary>
/// Design-time factory for LifeOSDbContext - used by EF Core migrations
/// </summary>
public class LifeOSDbContextFactory : IDesignTimeDbContextFactory<LifeOSDbContext>
{
    public LifeOSDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LifeOSDbContext>();

        // PostgreSQL connection string for migrations
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=LifeOSDb;Username=postgres;Password=postgres",
            b => b.MigrationsHistoryTable("__EFMigrationsHistory", "public")
        );

        // Use a lightweight execution context accessor for design-time operations
        IExecutionContextAccessor executionContextAccessor = new DesignTimeExecutionContextAccessor();

        return new LifeOSDbContext(optionsBuilder.Options, executionContextAccessor);
    }

    private sealed class DesignTimeExecutionContextAccessor : IExecutionContextAccessor
    {
        public Guid? GetCurrentUserId() => null;

        public IDisposable BeginScope(Guid userId) => NullScope.Instance;

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
