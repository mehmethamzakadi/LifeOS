using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations;

public class GameStoreConfiguration : BaseConfiguraiton<GameStore>
{
    public override void Configure(EntityTypeBuilder<GameStore> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Indexler
        builder.HasIndex(x => x.Name)
            .HasDatabaseName("IX_GameStores_Name");
    }
}

