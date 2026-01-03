using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class MusicConnectionConfiguration : BaseConfiguraiton<MusicConnection>
{
    public override void Configure(EntityTypeBuilder<MusicConnection> builder)
    {
        base.Configure(builder);

        builder.ToTable("MusicConnections");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.AccessToken)
            .IsRequired()
            .HasMaxLength(2000); // Encrypted token

        builder.Property(x => x.RefreshToken)
            .IsRequired()
            .HasMaxLength(2000); // Encrypted token

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.SpotifyUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SpotifyUserName)
            .HasMaxLength(200);

        builder.Property(x => x.SpotifyUserEmail)
            .HasMaxLength(200);

        builder.Property(x => x.ConnectedAt)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("IX_MusicConnections_UserId");

        builder.HasIndex(x => x.SpotifyUserId)
            .HasDatabaseName("IX_MusicConnections_SpotifyUserId");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_MusicConnections_IsActive");

        // Foreign key
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

