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

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.CurrentSeason);

        builder.Property(x => x.CurrentEpisode);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Rating);

        builder.Property(x => x.PersonalNote)
            .HasMaxLength(2000);

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_MovieSeries_Title");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_MovieSeries_Type");

        builder.HasIndex(x => x.Platform)
            .HasDatabaseName("IX_MovieSeries_Platform");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_MovieSeries_Status");
    }
}

