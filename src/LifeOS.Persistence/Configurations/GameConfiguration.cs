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

        builder.Property(x => x.GamePlatformId)
            .IsRequired();

        builder.Property(x => x.GameStoreId)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.IsOwned)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign Keys
        builder.HasOne(x => x.GamePlatform)
            .WithMany(x => x.Games)
            .HasForeignKey(x => x.GamePlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.GameStore)
            .WithMany(x => x.Games)
            .HasForeignKey(x => x.GameStoreId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_Games_Title");

        builder.HasIndex(x => x.GamePlatformId)
            .HasDatabaseName("IX_Games_GamePlatformId");

        builder.HasIndex(x => x.GameStoreId)
            .HasDatabaseName("IX_Games_GameStoreId");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Games_Status");

        builder.HasIndex(x => x.IsOwned)
            .HasDatabaseName("IX_Games_IsOwned");
    }
}

