using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Oyun mağazalarını seed eder
/// </summary>
public class GameStoreSeeder : BaseSeeder
{
    public GameStoreSeeder(LifeOSDbContext context, ILogger<GameStoreSeeder> logger) 
        : base(context, logger)
    {
    }

    public override int Order => 8; // GamePlatform'dan sonra
    public override string Name => "GameStore Seeder";

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var seedDate = new DateTime(2025, 10, 23, 7, 0, 0, DateTimeKind.Utc);
        var systemUserId = SystemUsers.SystemUserId;

        var storeDataList = new[]
        {
            new { EnumValue = LifeOS.Domain.Enums.GameStore.Steam, Name = "Steam" },
            new { EnumValue = LifeOS.Domain.Enums.GameStore.Epic, Name = "Epic" },
            new { EnumValue = LifeOS.Domain.Enums.GameStore.PS_Store, Name = "PS Store" },
            new { EnumValue = LifeOS.Domain.Enums.GameStore.Xbox_Store, Name = "Xbox Store" },
            new { EnumValue = LifeOS.Domain.Enums.GameStore.Physical, Name = "Physical" }
        };

        var stores = new List<LifeOS.Domain.Entities.GameStore>();

        foreach (var storeData in storeDataList)
        {
            var store = LifeOS.Domain.Entities.GameStore.Create(storeData.Name);

            // Sabit ID ve tarihleri EF Core ile set et
            var entry = Context.Entry(store);
            entry.Property("Id").CurrentValue = Guid.Parse($"31000000-0000-0000-0000-00000000000{(int)storeData.EnumValue + 1}");
            entry.Property("CreatedDate").CurrentValue = seedDate;
            entry.Property("CreatedById").CurrentValue = systemUserId;
            entry.Property("IsDeleted").CurrentValue = false;

            stores.Add(store);
        }

        await AddRangeIfNotExistsAsync(stores, s => (Guid)Context.Entry(s).Property("Id").CurrentValue!, cancellationToken);
    }
}

