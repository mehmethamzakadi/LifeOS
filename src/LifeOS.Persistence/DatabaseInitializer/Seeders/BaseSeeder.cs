using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Tüm seeder sınıfları için base class
/// Ortak işlevsellik ve yardımcı metodlar sağlar
/// </summary>
public abstract class BaseSeeder : ISeeder
{
    protected readonly LifeOSDbContext Context;
    protected readonly ILogger Logger;

    protected BaseSeeder(LifeOSDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    public abstract int Order { get; }
    public abstract string Name { get; }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Starting seed: {SeederName}", Name);
            
            await SeedDataAsync(cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
            
            Logger.LogInformation("Completed seed: {SeederName}", Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while seeding: {SeederName}", Name);
            throw;
        }
    }

    /// <summary>
    /// Alt sınıflar tarafından implement edilecek actual seed logic
    /// </summary>
    protected abstract Task SeedDataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Entity'nin veritabanında olup olmadığını kontrol eder
    /// Soft delete edilmiş kayıtları da kontrol eder (IgnoreQueryFilters)
    /// </summary>
    protected async Task<bool> ExistsAsync<TEntity>(Guid id, CancellationToken cancellationToken) 
        where TEntity : class
    {
        return await Context.Set<TEntity>()
            .IgnoreQueryFilters()
            .AnyAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);
    }

    /// <summary>
    /// Toplu veri eklemek için yardımcı metod
    /// Sadece mevcut olmayanları ekler (idempotent)
    /// Soft delete edilmiş kayıtları da kontrol eder (IgnoreQueryFilters)
    /// </summary>
    protected async Task AddRangeIfNotExistsAsync<TEntity>(
        IEnumerable<TEntity> entities,
        Func<TEntity, Guid> idSelector,
        CancellationToken cancellationToken) where TEntity : class
    {
        // IgnoreQueryFilters kullanarak soft delete edilmiş kayıtları da kontrol et
        var existingIds = await Context.Set<TEntity>()
            .IgnoreQueryFilters()
            .Select(e => EF.Property<Guid>(e, "Id"))
            .ToHashSetAsync(cancellationToken);

        var newEntities = entities
            .Where(e => !existingIds.Contains(idSelector(e)))
            .ToList();

        if (newEntities.Any())
        {
            await Context.Set<TEntity>().AddRangeAsync(newEntities, cancellationToken);
            Logger.LogInformation("Added {Count} new {EntityType} records", 
                newEntities.Count, typeof(TEntity).Name);
        }
        else
        {
            Logger.LogInformation("All {EntityType} records already exist, skipping", 
                typeof(TEntity).Name);
        }
    }
}
