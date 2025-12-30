using LifeOS.Domain.Common;
using LifeOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

// ISoftDeletable interface kullanılarak soft delete filter sadece gerekli entity'lere uygulanır

namespace LifeOS.Persistence.Contexts
{
    public class LifeOSDbContext : AuditableDbContext
    {
        public LifeOSDbContext(
            DbContextOptions<LifeOSDbContext> options,
            IExecutionContextAccessor executionContextAccessor)
            : base(options, executionContextAccessor)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PERFORMANS İYİLEŞTİRMESİ: Nullable ve non-nullable DateTime için ayrı converter
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // DateTime converter
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }

                // Global query filter for soft delete - only for ISoftDeletable entities
                // Excludes RefreshSession from soft delete filter
                if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType) &&
                    entityType.ClrType != typeof(RefreshSession))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                    var filter = Expression.Lambda(
                        Expression.Equal(property, Expression.Constant(false)), 
                        parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LifeOSDbContext).Assembly);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<RefreshSession> RefreshSessions { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<MovieSeries> MovieSeries { get; set; }
        public DbSet<PersonalNote> PersonalNotes { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
    }
}
