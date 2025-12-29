using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class PersonalNoteConfiguration : BaseConfiguraiton<PersonalNote>
{
    public override void Configure(EntityTypeBuilder<PersonalNote> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasColumnType("text"); // HTML/RichText için text tipi

        builder.Property(x => x.Category)
            .HasMaxLength(100);

        builder.Property(x => x.IsPinned)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.Tags)
            .HasMaxLength(500); // Comma separated tags

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_PersonalNotes_Title");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("IX_PersonalNotes_Category");

        builder.HasIndex(x => x.IsPinned)
            .HasDatabaseName("IX_PersonalNotes_IsPinned");
    }
}

