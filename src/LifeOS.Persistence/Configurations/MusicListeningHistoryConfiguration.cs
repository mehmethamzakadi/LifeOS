using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class MusicListeningHistoryConfiguration : BaseConfiguraiton<MusicListeningHistory>
{
    public override void Configure(EntityTypeBuilder<MusicListeningHistory> builder)
    {
        base.Configure(builder);

        builder.ToTable("MusicListeningHistory");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.SpotifyTrackId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TrackName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.ArtistName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AlbumName)
            .HasMaxLength(300);

        builder.Property(x => x.Genre)
            .HasMaxLength(100);

        builder.Property(x => x.PlayedAt)
            .IsRequired();

        builder.Property(x => x.DurationMs)
            .IsRequired();

        builder.Property(x => x.ProgressMs);

        // Indexes for performance
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_MusicListeningHistory_UserId");

        builder.HasIndex(x => x.SpotifyTrackId)
            .HasDatabaseName("IX_MusicListeningHistory_SpotifyTrackId");

        builder.HasIndex(x => x.PlayedAt)
            .HasDatabaseName("IX_MusicListeningHistory_PlayedAt");

        // Composite index for statistics queries
        builder.HasIndex(x => new { x.UserId, x.PlayedAt })
            .HasDatabaseName("IX_MusicListeningHistory_UserId_PlayedAt");

        builder.HasIndex(x => new { x.UserId, x.ArtistName })
            .HasDatabaseName("IX_MusicListeningHistory_UserId_ArtistName");

        // Foreign key
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

