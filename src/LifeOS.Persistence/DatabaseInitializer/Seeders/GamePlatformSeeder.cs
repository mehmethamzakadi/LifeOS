using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Oyun platformlarını seed eder
/// </summary>
public class GamePlatformSeeder : BaseSeeder
{
    public GamePlatformSeeder(LifeOSDbContext context, ILogger<GamePlatformSeeder> logger) 
        : base(context, logger)
    {
    }

    public override int Order => 7; // Category'den sonra
    public override string Name => "GamePlatform Seeder";

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var seedDate = new DateTime(2025, 10, 23, 7, 0, 0, DateTimeKind.Utc);
        var systemUserId = SystemUsers.SystemUserId;

        var platformDataList = new[]
        {
            new { EnumValue = LifeOS.Domain.Enums.GamePlatform.PC, Name = "PC" },
            new { EnumValue = LifeOS.Domain.Enums.GamePlatform.PS5, Name = "PS5" },
            new { EnumValue = LifeOS.Domain.Enums.GamePlatform.Xbox, Name = "Xbox" },
            new { EnumValue = LifeOS.Domain.Enums.GamePlatform.Switch, Name = "Switch" },
            new { EnumValue = LifeOS.Domain.Enums.GamePlatform.Mobile, Name = "Mobile" }
        };

        var platforms = new List<LifeOS.Domain.Entities.GamePlatform>();

        foreach (var platformData in platformDataList)
        {
            var platform = LifeOS.Domain.Entities.GamePlatform.Create(platformData.Name);

            // Sabit ID ve tarihleri EF Core ile set et
            var entry = Context.Entry(platform);
            entry.Property("Id").CurrentValue = Guid.Parse($"30000000-0000-0000-0000-00000000000{(int)platformData.EnumValue + 1}");
            entry.Property("CreatedDate").CurrentValue = seedDate;
            entry.Property("CreatedById").CurrentValue = systemUserId;
            entry.Property("IsDeleted").CurrentValue = false;

            platforms.Add(platform);
        }

        await AddRangeIfNotExistsAsync(platforms, p => (Guid)Context.Entry(p).Property("Id").CurrentValue!, cancellationToken);
    }
}

