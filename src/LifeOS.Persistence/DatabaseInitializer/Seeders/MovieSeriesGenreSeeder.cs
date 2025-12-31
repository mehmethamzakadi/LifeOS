using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Film/Dizi türlerini seed eder
/// </summary>
public class MovieSeriesGenreSeeder : BaseSeeder
{
    public MovieSeriesGenreSeeder(LifeOSDbContext context, ILogger<MovieSeriesGenreSeeder> logger) 
        : base(context, logger)
    {
    }

    public override int Order => 10; // WatchPlatform'dan sonra
    public override string Name => "MovieSeriesGenre Seeder";

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var seedDate = new DateTime(2025, 10, 23, 7, 0, 0, DateTimeKind.Utc);
        var systemUserId = SystemUsers.SystemUserId;

        // MovieSeriesType enum değerlerini genre olarak seed ediyoruz
        var genreDataList = new[]
        {
            new { EnumValue = MovieSeriesType.Movie, Name = "Film" },
            new { EnumValue = MovieSeriesType.Series, Name = "Dizi" }
        };

        var genres = new List<MovieSeriesGenre>();

        foreach (var genreData in genreDataList)
        {
            var genre = MovieSeriesGenre.Create(genreData.Name);

            // Sabit ID ve tarihleri EF Core ile set et
            var entry = Context.Entry(genre);
            entry.Property("Id").CurrentValue = Guid.Parse($"33000000-0000-0000-0000-00000000000{(int)genreData.EnumValue + 1}");
            entry.Property("CreatedDate").CurrentValue = seedDate;
            entry.Property("CreatedById").CurrentValue = systemUserId;
            entry.Property("IsDeleted").CurrentValue = false;

            genres.Add(genre);
        }

        await AddRangeIfNotExistsAsync(genres, g => (Guid)Context.Entry(g).Property("Id").CurrentValue!, cancellationToken);
    }
}

