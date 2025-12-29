using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class GameConfiguration : BaseConfiguraiton<Game>
{
    public override void Configure(EntityTypeBuilder<Game> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CoverUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Store)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsOwned)
            .IsRequired()
            .HasDefaultValue(false);

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_Games_Title");

        builder.HasIndex(x => x.Platform)
            .HasDatabaseName("IX_Games_Platform");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Games_Status");

        builder.HasIndex(x => x.IsOwned)
            .HasDatabaseName("IX_Games_IsOwned");
    }
}

