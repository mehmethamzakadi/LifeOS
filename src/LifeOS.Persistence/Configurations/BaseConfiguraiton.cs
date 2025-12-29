
using LifeOS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LifeOS.Persistence.Configurations
{
    /// <summary>
    /// Tüm BaseEntity türevleri için ortak yapılandırma
    /// </summary>
    public class BaseConfiguraiton<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);
            
            // ✅ Concurrency token - Optimistic locking
            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .HasColumnName("RowVersion");
            
            // ✅ Soft delete filter - LifeOSDbContext'te reflection ile otomatik uygulanıyor
            // Burada tekrar uygulamaya gerek yok, çünkü LifeOSDbContext.OnModelCreating'de
            // tüm ISoftDeletable entity'lere otomatik olarak filter uygulanıyor
            // builder.HasQueryFilter(x => !x.IsDeleted); // Yorum satırına alındı - LifeOSDbContext'te zaten var
        }
    }
}
