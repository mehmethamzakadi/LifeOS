namespace LifeOS.Domain.Constants;

/// <summary>
/// Sistemdeki tüm permission'ları string constant olarak tanımlar.
/// Format: {ModuleName}.{PermissionType}
/// </summary>
public static class Permissions
{
    // Dashboard Permissions
    public const string DashboardView = "Dashboard.View";

    // User Management Permissions
    public const string UsersCreate = "Users.Create";
    public const string UsersRead = "Users.Read";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDelete = "Users.Delete";
    public const string UsersViewAll = "Users.ViewAll";

    // Role Management Permissions
    public const string RolesCreate = "Roles.Create";
    public const string RolesRead = "Roles.Read";
    public const string RolesUpdate = "Roles.Update";
    public const string RolesDelete = "Roles.Delete";
    public const string RolesViewAll = "Roles.ViewAll";
    public const string RolesAssignPermissions = "Roles.AssignPermissions";

    // Category Management Permissions
    public const string CategoriesCreate = "Categories.Create";
    public const string CategoriesRead = "Categories.Read";
    public const string CategoriesUpdate = "Categories.Update";
    public const string CategoriesDelete = "Categories.Delete";
    public const string CategoriesViewAll = "Categories.ViewAll";

    // Media Management Permissions
    public const string MediaUpload = "Media.Upload";

    // Activity Logs Permissions
    public const string ActivityLogsView = "ActivityLogs.View";

    // Book Management Permissions
    public const string BooksCreate = "Books.Create";
    public const string BooksRead = "Books.Read";
    public const string BooksUpdate = "Books.Update";
    public const string BooksDelete = "Books.Delete";
    public const string BooksViewAll = "Books.ViewAll";

    // Game Management Permissions
    public const string GamesCreate = "Games.Create";
    public const string GamesRead = "Games.Read";
    public const string GamesUpdate = "Games.Update";
    public const string GamesDelete = "Games.Delete";
    public const string GamesViewAll = "Games.ViewAll";

    // MovieSeries Management Permissions
    public const string MovieSeriesCreate = "MovieSeries.Create";
    public const string MovieSeriesRead = "MovieSeries.Read";
    public const string MovieSeriesUpdate = "MovieSeries.Update";
    public const string MovieSeriesDelete = "MovieSeries.Delete";
    public const string MovieSeriesViewAll = "MovieSeries.ViewAll";

    // PersonalNote Management Permissions
    public const string PersonalNotesCreate = "PersonalNotes.Create";
    public const string PersonalNotesRead = "PersonalNotes.Read";
    public const string PersonalNotesUpdate = "PersonalNotes.Update";
    public const string PersonalNotesDelete = "PersonalNotes.Delete";
    public const string PersonalNotesViewAll = "PersonalNotes.ViewAll";

    // WalletTransaction Management Permissions
    public const string WalletTransactionsCreate = "WalletTransactions.Create";
    public const string WalletTransactionsRead = "WalletTransactions.Read";
    public const string WalletTransactionsUpdate = "WalletTransactions.Update";
    public const string WalletTransactionsDelete = "WalletTransactions.Delete";
    public const string WalletTransactionsViewAll = "WalletTransactions.ViewAll";

    /// <summary>
    /// Tüm permission'ları liste olarak döndürür. Seed işlemleri için kullanılır.
    /// </summary>
    public static List<string> GetAllPermissions()
    {
        return new List<string>
        {
            // Dashboard
            DashboardView,

            // Users
            UsersCreate, UsersRead, UsersUpdate, UsersDelete, UsersViewAll,

            // Roles
            RolesCreate, RolesRead, RolesUpdate, RolesDelete, RolesViewAll, RolesAssignPermissions,

            // Categories
            CategoriesCreate, CategoriesRead, CategoriesUpdate, CategoriesDelete, CategoriesViewAll,

            // Media
            MediaUpload,

            // Activity Logs
            ActivityLogsView,

            // Books
            BooksCreate, BooksRead, BooksUpdate, BooksDelete, BooksViewAll,

            // Games
            GamesCreate, GamesRead, GamesUpdate, GamesDelete, GamesViewAll,

            // MovieSeries
            MovieSeriesCreate, MovieSeriesRead, MovieSeriesUpdate, MovieSeriesDelete, MovieSeriesViewAll,

            // PersonalNotes
            PersonalNotesCreate, PersonalNotesRead, PersonalNotesUpdate, PersonalNotesDelete, PersonalNotesViewAll,

            // WalletTransactions
            WalletTransactionsCreate, WalletTransactionsRead, WalletTransactionsUpdate, WalletTransactionsDelete, WalletTransactionsViewAll
        };
    }

    /// <summary>
    /// Admin rolü için tüm permission'ları döndürür
    /// </summary>
    public static List<string> GetAdminPermissions()
    {
        return GetAllPermissions();
    }

    /// <summary>
    /// User rolü için temel permission'ları döndürür
    /// </summary>
    public static List<string> GetUserPermissions()
    {
        return new List<string>
        {
            // Kategorileri okuyabilir
            CategoriesRead
        };
    }
}
