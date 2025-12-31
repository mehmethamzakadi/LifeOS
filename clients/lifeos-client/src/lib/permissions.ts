/**
 * Backend'deki Permissions.cs ile senkronize edilmiş permission constant'ları
 * Format: {ModuleName}.{PermissionType}
 */
export const Permissions = {
  // Dashboard
  DashboardView: "Dashboard.View",

  // User Management
  UsersCreate: "Users.Create",
  UsersRead: "Users.Read",
  UsersUpdate: "Users.Update",
  UsersDelete: "Users.Delete",
  UsersViewAll: "Users.ViewAll",

  // Role Management
  RolesCreate: "Roles.Create",
  RolesRead: "Roles.Read",
  RolesUpdate: "Roles.Update",
  RolesDelete: "Roles.Delete",
  RolesViewAll: "Roles.ViewAll",
  RolesAssignPermissions: "Roles.AssignPermissions",

  // Category Management
  CategoriesCreate: "Categories.Create",
  CategoriesRead: "Categories.Read",
  CategoriesUpdate: "Categories.Update",
  CategoriesDelete: "Categories.Delete",
  CategoriesViewAll: "Categories.ViewAll",

  // Media Management
  MediaUpload: "Media.Upload",

  // Book Management
  BooksCreate: "Books.Create",
  BooksRead: "Books.Read",
  BooksUpdate: "Books.Update",
  BooksDelete: "Books.Delete",
  BooksViewAll: "Books.ViewAll",

  // Game Management
  GamesCreate: "Games.Create",
  GamesRead: "Games.Read",
  GamesUpdate: "Games.Update",
  GamesDelete: "Games.Delete",
  GamesViewAll: "Games.ViewAll",

  // MovieSeries Management
  MovieSeriesCreate: "MovieSeries.Create",
  MovieSeriesRead: "MovieSeries.Read",
  MovieSeriesUpdate: "MovieSeries.Update",
  MovieSeriesDelete: "MovieSeries.Delete",
  MovieSeriesViewAll: "MovieSeries.ViewAll",

  // PersonalNote Management
  PersonalNotesCreate: "PersonalNotes.Create",
  PersonalNotesRead: "PersonalNotes.Read",
  PersonalNotesUpdate: "PersonalNotes.Update",
  PersonalNotesDelete: "PersonalNotes.Delete",
  PersonalNotesViewAll: "PersonalNotes.ViewAll",

  // WalletTransaction Management
  WalletTransactionsCreate: "WalletTransactions.Create",
  WalletTransactionsRead: "WalletTransactions.Read",
  WalletTransactionsUpdate: "WalletTransactions.Update",
  WalletTransactionsDelete: "WalletTransactions.Delete",
  WalletTransactionsViewAll: "WalletTransactions.ViewAll",

  // GamePlatform Management
  GamePlatformsCreate: "GamePlatforms.Create",
  GamePlatformsRead: "GamePlatforms.Read",
  GamePlatformsUpdate: "GamePlatforms.Update",
  GamePlatformsDelete: "GamePlatforms.Delete",
  GamePlatformsViewAll: "GamePlatforms.ViewAll",

  // GameStore Management
  GameStoresCreate: "GameStores.Create",
  GameStoresRead: "GameStores.Read",
  GameStoresUpdate: "GameStores.Update",
  GameStoresDelete: "GameStores.Delete",
  GameStoresViewAll: "GameStores.ViewAll",

  // WatchPlatform Management
  WatchPlatformsCreate: "WatchPlatforms.Create",
  WatchPlatformsRead: "WatchPlatforms.Read",
  WatchPlatformsUpdate: "WatchPlatforms.Update",
  WatchPlatformsDelete: "WatchPlatforms.Delete",
  WatchPlatformsViewAll: "WatchPlatforms.ViewAll",

  // MovieSeriesGenre Management
  MovieSeriesGenresCreate: "MovieSeriesGenres.Create",
  MovieSeriesGenresRead: "MovieSeriesGenres.Read",
  MovieSeriesGenresUpdate: "MovieSeriesGenres.Update",
  MovieSeriesGenresDelete: "MovieSeriesGenres.Delete",
  MovieSeriesGenresViewAll: "MovieSeriesGenres.ViewAll",
} as const;

export type Permission = (typeof Permissions)[keyof typeof Permissions];
