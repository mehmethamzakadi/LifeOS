using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Permission seed işlemlerini gerçekleştirir
/// Sistemdeki tüm permission'ları oluşturur
/// </summary>
public class PermissionSeeder : BaseSeeder
{
    public PermissionSeeder(LifeOSDbContext context, ILogger<PermissionSeeder> logger)
        : base(context, logger)
    {
    }

    public override int Order => 2; // Rollerden sonra permission'lar
    public override string Name => "Permission Seeder";

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var createdDate = new DateTime(2025, 10, 23, 7, 0, 0, DateTimeKind.Utc);

        var allPermissionNames = Permissions.GetAllPermissions();

        // Mevcut permission'ları kontrol et
        var existingPermissions = await Context.Permissions
            .Where(p => !p.IsDeleted)
            .Select(p => new { p.NormalizedName, p.Id })
            .ToListAsync(cancellationToken);

        var existingNormalizedNames = existingPermissions
            .Select(p => p.NormalizedName)
            .Where(n => n != null)
            .ToHashSet();

        var existingIds = existingPermissions
            .Select(p => p.Id)
            .ToHashSet();

        var permissionsToAdd = new List<Permission>();

        // Mevcut permission sayısına göre index'i başlat (yeni permission'lar için)
        var permissionIndex = existingPermissions.Count;

        for (int index = 0; index < allPermissionNames.Count; index++)
        {
            var permissionName = allPermissionNames[index];
            var normalizedName = permissionName.ToUpperInvariant();

            // NormalizedName'e göre kontrol et - zaten varsa atla
            if (existingNormalizedNames.Contains(normalizedName))
            {
                Logger.LogDebug("Permission already exists: {PermissionName}, skipping", permissionName);
                continue;
            }

            var parts = permissionName.Split('.');
            var module = parts[0];
            var type = parts.Length > 1 ? parts[1] : "Custom";

            var permission = Permission.Create(
                permissionName,
                module,
                type,
                GetPermissionDescription(permissionName)
            );

            // ID atama - mevcut ID'lerle çakışmayacak şekilde
            Guid permissionId;
            do
            {
                permissionIndex++;
                permissionId = Guid.Parse($"30000000-0000-0000-0000-000000000{permissionIndex:D3}");
            } while (existingIds.Contains(permissionId) && permissionIndex < 1000);

            // Eğer 1000'e ulaşırsa, Guid.NewGuid() kullan
            if (permissionIndex >= 1000)
            {
                permissionId = Guid.NewGuid();
                Logger.LogWarning("Permission ID index exceeded 1000, using random GUID for: {PermissionName}", permissionName);
            }

            // Sabit ID ve tarihleri EF Core ile set et
            var entry = Context.Entry(permission);
            entry.Property("Id").CurrentValue = permissionId;
            entry.Property("CreatedDate").CurrentValue = createdDate;
            entry.Property("IsDeleted").CurrentValue = false;

            permissionsToAdd.Add(permission);
            existingIds.Add(permissionId); // Yeni ID'yi de set'e ekle
        }

        if (permissionsToAdd.Any())
        {
            await Context.Permissions.AddRangeAsync(permissionsToAdd, cancellationToken);
            Logger.LogInformation("Added {Count} new Permission records", permissionsToAdd.Count);
        }
        else
        {
            Logger.LogInformation("All Permission records already exist, skipping");
        }
    }

    private static string GetPermissionDescription(string permissionName)
    {
        return permissionName switch
        {
            Permissions.DashboardView => "Admin paneli dashboard'una erişim yetkisi",
            Permissions.UsersCreate => "Yeni kullanıcı oluşturma yetkisi",
            Permissions.UsersRead => "Kullanıcı bilgilerini görüntüleme yetkisi",
            Permissions.UsersUpdate => "Kullanıcı bilgilerini güncelleme yetkisi",
            Permissions.UsersDelete => "Kullanıcı silme yetkisi",
            Permissions.UsersViewAll => "Tüm kullanıcıları görüntüleme yetkisi",
            Permissions.RolesCreate => "Yeni rol oluşturma yetkisi",
            Permissions.RolesRead => "Rol bilgilerini görüntüleme yetkisi",
            Permissions.RolesUpdate => "Rol bilgilerini güncelleme yetkisi",
            Permissions.RolesDelete => "Rol silme yetkisi",
            Permissions.RolesViewAll => "Tüm rolleri görüntüleme yetkisi",
            Permissions.RolesAssignPermissions => "Role yetki atama yetkisi",
            Permissions.CategoriesCreate => "Yeni kategori oluşturma yetkisi",
            Permissions.CategoriesRead => "Kategori görüntüleme yetkisi",
            Permissions.CategoriesUpdate => "Kategori güncelleme yetkisi",
            Permissions.CategoriesDelete => "Kategori silme yetkisi",
            Permissions.CategoriesViewAll => "Tüm kategorileri görüntüleme yetkisi",
            Permissions.MediaUpload => "Medya dosyası yükleme yetkisi",
            Permissions.BooksCreate => "Yeni kitap oluşturma yetkisi",
            Permissions.BooksRead => "Kitap bilgilerini görüntüleme yetkisi",
            Permissions.BooksUpdate => "Kitap bilgilerini güncelleme yetkisi",
            Permissions.BooksDelete => "Kitap silme yetkisi",
            Permissions.BooksViewAll => "Tüm kitapları görüntüleme yetkisi",
            Permissions.GamesCreate => "Yeni oyun oluşturma yetkisi",
            Permissions.GamesRead => "Oyun bilgilerini görüntüleme yetkisi",
            Permissions.GamesUpdate => "Oyun bilgilerini güncelleme yetkisi",
            Permissions.GamesDelete => "Oyun silme yetkisi",
            Permissions.GamesViewAll => "Tüm oyunları görüntüleme yetkisi",
            Permissions.MovieSeriesCreate => "Yeni film/dizi oluşturma yetkisi",
            Permissions.MovieSeriesRead => "Film/dizi bilgilerini görüntüleme yetkisi",
            Permissions.MovieSeriesUpdate => "Film/dizi bilgilerini güncelleme yetkisi",
            Permissions.MovieSeriesDelete => "Film/dizi silme yetkisi",
            Permissions.MovieSeriesViewAll => "Tüm film/dizileri görüntüleme yetkisi",
            Permissions.PersonalNotesCreate => "Yeni kişisel not oluşturma yetkisi",
            Permissions.PersonalNotesRead => "Kişisel not bilgilerini görüntüleme yetkisi",
            Permissions.PersonalNotesUpdate => "Kişisel not bilgilerini güncelleme yetkisi",
            Permissions.PersonalNotesDelete => "Kişisel not silme yetkisi",
            Permissions.PersonalNotesViewAll => "Tüm kişisel notları görüntüleme yetkisi",
            Permissions.WalletTransactionsCreate => "Yeni cüzdan işlemi oluşturma yetkisi",
            Permissions.WalletTransactionsRead => "Cüzdan işlemi bilgilerini görüntüleme yetkisi",
            Permissions.WalletTransactionsUpdate => "Cüzdan işlemi bilgilerini güncelleme yetkisi",
            Permissions.WalletTransactionsDelete => "Cüzdan işlemi silme yetkisi",
            Permissions.WalletTransactionsViewAll => "Tüm cüzdan işlemlerini görüntüleme yetkisi",
            Permissions.GamePlatformsCreate => "Yeni oyun platformu oluşturma yetkisi",
            Permissions.GamePlatformsRead => "Oyun platformu bilgilerini görüntüleme yetkisi",
            Permissions.GamePlatformsUpdate => "Oyun platformu bilgilerini güncelleme yetkisi",
            Permissions.GamePlatformsDelete => "Oyun platformu silme yetkisi",
            Permissions.GamePlatformsViewAll => "Tüm oyun platformlarını görüntüleme yetkisi",
            Permissions.GameStoresCreate => "Yeni oyun mağazası oluşturma yetkisi",
            Permissions.GameStoresRead => "Oyun mağazası bilgilerini görüntüleme yetkisi",
            Permissions.GameStoresUpdate => "Oyun mağazası bilgilerini güncelleme yetkisi",
            Permissions.GameStoresDelete => "Oyun mağazası silme yetkisi",
            Permissions.GameStoresViewAll => "Tüm oyun mağazalarını görüntüleme yetkisi",
            Permissions.WatchPlatformsCreate => "Yeni izleme platformu oluşturma yetkisi",
            Permissions.WatchPlatformsRead => "İzleme platformu bilgilerini görüntüleme yetkisi",
            Permissions.WatchPlatformsUpdate => "İzleme platformu bilgilerini güncelleme yetkisi",
            Permissions.WatchPlatformsDelete => "İzleme platformu silme yetkisi",
            Permissions.WatchPlatformsViewAll => "Tüm izleme platformlarını görüntüleme yetkisi",
            Permissions.MovieSeriesGenresCreate => "Yeni film/dizi türü oluşturma yetkisi",
            Permissions.MovieSeriesGenresRead => "Film/dizi türü bilgilerini görüntüleme yetkisi",
            Permissions.MovieSeriesGenresUpdate => "Film/dizi türü bilgilerini güncelleme yetkisi",
            Permissions.MovieSeriesGenresDelete => "Film/dizi türü silme yetkisi",
            Permissions.MovieSeriesGenresViewAll => "Tüm film/dizi türlerini görüntüleme yetkisi",
            _ => $"{permissionName} permission"
        };
    }
}
