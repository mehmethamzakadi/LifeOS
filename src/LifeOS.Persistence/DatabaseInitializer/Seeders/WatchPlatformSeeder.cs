using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Film/Dizi izleme platformlarını seed eder
/// </summary>
public class WatchPlatformSeeder : BaseSeeder
{
    public WatchPlatformSeeder(LifeOSDbContext context, ILogger<WatchPlatformSeeder> logger) 
        : base(context, logger)
    {
    }

    public override int Order => 9; // GameStore'dan sonra
    public override string Name => "WatchPlatform Seeder";

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var seedDate = new DateTime(2025, 10, 23, 7, 0, 0, DateTimeKind.Utc);
        var systemUserId = SystemUsers.SystemUserId;

        var platformDataList = new[]
        {
            new { EnumValue = MovieSeriesPlatform.Netflix, Name = "Netflix" },
            new { EnumValue = MovieSeriesPlatform.Prime, Name = "Prime" },
            new { EnumValue = MovieSeriesPlatform.Disney, Name = "Disney" },
            new { EnumValue = MovieSeriesPlatform.Local, Name = "Local" }
        };

        var platforms = new List<WatchPlatform>();

        foreach (var platformData in platformDataList)
        {
            var platform = WatchPlatform.Create(platformData.Name);

            // Sabit ID ve tarihleri EF Core ile set et
            var entry = Context.Entry(platform);
            entry.Property("Id").CurrentValue = Guid.Parse($"32000000-0000-0000-0000-00000000000{(int)platformData.EnumValue + 1}");
            entry.Property("CreatedDate").CurrentValue = seedDate;
            entry.Property("CreatedById").CurrentValue = systemUserId;
            entry.Property("IsDeleted").CurrentValue = false;

            platforms.Add(platform);
        }

        await AddRangeIfNotExistsAsync(platforms, p => (Guid)Context.Entry(p).Property("Id").CurrentValue!, cancellationToken);
    }
}

