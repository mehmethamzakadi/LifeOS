using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class WalletTransactionConfiguration : BaseConfiguraiton<WalletTransaction>
{
    public override void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2); // Decimal için precision

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(x => x.TransactionDate)
            .IsRequired();

        // Indexler
        builder.HasIndex(x => x.Title)
            .HasDatabaseName("IX_WalletTransactions_Title");

        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_WalletTransactions_Type");

        builder.HasIndex(x => x.Category)
            .HasDatabaseName("IX_WalletTransactions_Category");

        builder.HasIndex(x => x.TransactionDate)
            .HasDatabaseName("IX_WalletTransactions_TransactionDate");
    }
}

