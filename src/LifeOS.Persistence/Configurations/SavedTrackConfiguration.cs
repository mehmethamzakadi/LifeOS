using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class SavedTrackConfiguration : BaseConfiguraiton<SavedTrack>
{
    public override void Configure(EntityTypeBuilder<SavedTrack> builder)
    {
        base.Configure(builder);

        builder.ToTable("SavedTracks");

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.SpotifyTrackId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Artist)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Album)
            .HasMaxLength(300);

        builder.Property(x => x.AlbumCoverUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DurationMs);

        builder.Property(x => x.SavedAt);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_SavedTracks_UserId");

        builder.HasIndex(x => x.SpotifyTrackId)
            .HasDatabaseName("IX_SavedTracks_SpotifyTrackId");

        // Unique constraint: A user can only save a track once
        builder.HasIndex(x => new { x.UserId, x.SpotifyTrackId })
            .IsUnique()
            .HasDatabaseName("IX_SavedTracks_UserId_SpotifyTrackId");

        builder.HasIndex(x => x.SavedAt)
            .HasDatabaseName("IX_SavedTracks_SavedAt");

        // Foreign key
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

