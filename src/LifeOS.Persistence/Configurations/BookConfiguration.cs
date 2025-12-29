using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class BookConfiguration : BaseConfiguraiton<Book>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CoverUrl)
            .HasMaxLength(500);

        builder.Property(x => x.TotalPages)
            .IsRequired();

        builder.Property(x => x.CurrentPage)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Rating);

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_Books_Title");

        builder.HasIndex(x => x.Author)
            .HasDatabaseName("IX_Books_Author");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_Books_Status");
    }
}

