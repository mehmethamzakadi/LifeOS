using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class MovieSeriesConfiguration : BaseConfiguraiton<MovieSeries>
{
    public override void Configure(EntityTypeBuilder<MovieSeries> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CoverUrl)
            .HasMaxLength(500);

        builder.Property(x => x.MovieSeriesGenreId)
            .IsRequired();

        builder.Property(x => x.WatchPlatformId)
            .IsRequired();

        builder.Property(x => x.CurrentSeason);

        builder.Property(x => x.CurrentEpisode);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Rating);

        builder.Property(x => x.PersonalNote)
            .HasMaxLength(2000);

        // Foreign Keys
        builder.HasOne(x => x.Genre)
            .WithMany(x => x.MovieSeries)
            .HasForeignKey(x => x.MovieSeriesGenreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WatchPlatform)
            .WithMany(x => x.MovieSeries)
            .HasForeignKey(x => x.WatchPlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_MovieSeries_Title");

        builder.HasIndex(x => x.MovieSeriesGenreId)
            .HasDatabaseName("IX_MovieSeries_MovieSeriesGenreId");

        builder.HasIndex(x => x.WatchPlatformId)
            .HasDatabaseName("IX_MovieSeries_WatchPlatformId");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_MovieSeries_Status");
    }
}

