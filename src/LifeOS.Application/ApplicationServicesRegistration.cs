using LifeOS.Application.Behaviors;
using LifeOS.Application.Features.Auths.Login;
using LifeOS.Application.Features.Auths.Logout;
using LifeOS.Application.Features.Auths.PasswordReset;
using LifeOS.Application.Features.Auths.PasswordVerify;
using LifeOS.Application.Features.Auths.RefreshToken;
using LifeOS.Application.Features.Auths.Register;
using LifeOS.Application.Features.Books.CreateBook;
using LifeOS.Application.Features.Books.UpdateBook;
using LifeOS.Application.Features.Books.DeleteBook;
using LifeOS.Application.Features.Books.GetBookById;
using LifeOS.Application.Features.Books.GetBookByIsbn;
using LifeOS.Application.Features.Books.SearchBooks;
using LifeOS.Application.Features.Categories.CreateCategory;
using LifeOS.Application.Features.Categories.UpdateCategory;
using LifeOS.Application.Features.Categories.DeleteCategory;
using LifeOS.Application.Features.Categories.GetCategoryById;
using LifeOS.Application.Features.Categories.GetAllCategories;
using LifeOS.Application.Features.Categories.SearchCategories;
using LifeOS.Application.Features.Categories.GenerateCategoryDescription;
using LifeOS.Application.Features.Dashboards.GetStatistics;
using LifeOS.Application.Features.Dashboards.GetRecentActivities;
using LifeOS.Application.Features.Dashboards.GetFinancialSummary;
using LifeOS.Application.Features.Games.CreateGame;
using LifeOS.Application.Features.Games.UpdateGame;
using LifeOS.Application.Features.Games.DeleteGame;
using LifeOS.Application.Features.Games.GetGameById;
using LifeOS.Application.Features.Games.SearchGames;
using LifeOS.Application.Features.GamePlatforms.CreateGamePlatform;
using LifeOS.Application.Features.GamePlatforms.UpdateGamePlatform;
using LifeOS.Application.Features.GamePlatforms.DeleteGamePlatform;
using LifeOS.Application.Features.GamePlatforms.GetAllGamePlatforms;
using LifeOS.Application.Features.GameStores.CreateGameStore;
using LifeOS.Application.Features.GameStores.UpdateGameStore;
using LifeOS.Application.Features.GameStores.DeleteGameStore;
using LifeOS.Application.Features.GameStores.GetAllGameStores;
using LifeOS.Application.Features.Images.UploadImage;
using LifeOS.Application.Features.MovieSeries.CreateMovieSeries;
using LifeOS.Application.Features.MovieSeries.UpdateMovieSeries;
using LifeOS.Application.Features.MovieSeries.DeleteMovieSeries;
using LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;
using LifeOS.Application.Features.MovieSeries.SearchMovieSeries;
using LifeOS.Application.Features.MovieSeriesGenres.CreateMovieSeriesGenre;
using LifeOS.Application.Features.MovieSeriesGenres.UpdateMovieSeriesGenre;
using LifeOS.Application.Features.MovieSeriesGenres.DeleteMovieSeriesGenre;
using LifeOS.Application.Features.MovieSeriesGenres.GetAllMovieSeriesGenres;
using LifeOS.Application.Features.WatchPlatforms.CreateWatchPlatform;
using LifeOS.Application.Features.WatchPlatforms.UpdateWatchPlatform;
using LifeOS.Application.Features.WatchPlatforms.DeleteWatchPlatform;
using LifeOS.Application.Features.WatchPlatforms.GetAllWatchPlatforms;
using LifeOS.Application.Features.PersonalNotes.CreatePersonalNote;
using LifeOS.Application.Features.PersonalNotes.UpdatePersonalNote;
using LifeOS.Application.Features.PersonalNotes.DeletePersonalNote;
using LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;
using LifeOS.Application.Features.PersonalNotes.SearchPersonalNotes;
using LifeOS.Application.Features.WalletTransactions.CreateWalletTransaction;
using LifeOS.Application.Features.WalletTransactions.UpdateWalletTransaction;
using LifeOS.Application.Features.WalletTransactions.DeleteWalletTransaction;
using LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;
using LifeOS.Application.Features.WalletTransactions.SearchWalletTransactions;
using LifeOS.Application.Features.Permissions.GetAllPermissions;
using LifeOS.Application.Features.Permissions.GetRolePermissions;
using LifeOS.Application.Features.Permissions.AssignPermissionsToRole;
using LifeOS.Application.Features.Roles.CreateRole;
using LifeOS.Application.Features.Roles.UpdateRole;
using LifeOS.Application.Features.Roles.DeleteRole;
using LifeOS.Application.Features.Roles.GetRoleById;
using LifeOS.Application.Features.Roles.GetListRoles;
using LifeOS.Application.Features.Roles.BulkDeleteRoles;
using LifeOS.Application.Features.Users.CreateUser;
using LifeOS.Application.Features.Users.UpdateUser;
using LifeOS.Application.Features.Users.DeleteUser;
using LifeOS.Application.Features.Users.GetUserById;
using LifeOS.Application.Features.Users.SearchUsers;
using LifeOS.Application.Features.Users.AssignRolesToUser;
using LifeOS.Application.Features.Users.GetUserRoles;
using LifeOS.Application.Features.Users.BulkDeleteUsers;
using LifeOS.Application.Features.Users.ExportUsers;
using LifeOS.Application.Features.Users.GetProfile;
using LifeOS.Application.Features.Users.UpdateProfile;
using LifeOS.Application.Features.Users.ChangePassword;
using LifeOS.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using LifeOS.Application.Features.Music.GetMusicAuthorizationUrl;
using LifeOS.Application.Features.Music.ConnectMusic;
using LifeOS.Application.Features.Music.GetConnectionStatus;
using LifeOS.Application.Features.Music.DisconnectMusic;
using LifeOS.Application.Features.Music.GetCurrentTrack;
using LifeOS.Application.Features.Music.GetSavedTracks;
using LifeOS.Application.Features.Music.SaveTrack;
using LifeOS.Application.Features.Music.DeleteSavedTrack;
using LifeOS.Application.Features.Music.GetListeningStats;

namespace LifeOS.Application
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection AddConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                
                // Pipeline behaviors - sıralama önemli!
                // 1. Validation - en başta
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
                // 2. Logging
                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
                // 3. Cache invalidation
                configuration.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
                // 4. Concurrency - en sonda (retry mekanizması tüm pipeline'ı tekrar çalıştırır)
                configuration.AddOpenBehavior(typeof(ConcurrencyBehavior<,>));
            });

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Register Books feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateBookHandler>();
            services.AddScoped<UpdateBookHandler>();
            services.AddScoped<DeleteBookHandler>();
            services.AddScoped<GetBookByIdHandler>();
            services.AddScoped<GetBookByIsbnHandler>();
            services.AddScoped<SearchBooksHandler>();

            // Register Music feature handlers (Vertical Slice Architecture)
            services.AddScoped<GetMusicAuthorizationUrlHandler>();
            services.AddScoped<ConnectMusicHandler>();
            services.AddScoped<GetConnectionStatusHandler>();
            services.AddScoped<DisconnectMusicHandler>();
            services.AddScoped<GetCurrentTrackHandler>();
            services.AddScoped<GetSavedTracksHandler>();
            services.AddScoped<SaveTrackHandler>();
            services.AddScoped<DeleteSavedTrackHandler>();
            services.AddScoped<GetListeningStatsHandler>();

            // Register Auths feature handlers (Vertical Slice Architecture)
            services.AddScoped<RegisterHandler>();
            services.AddScoped<LoginHandler>();
            services.AddScoped<LogoutHandler>();
            services.AddScoped<PasswordResetHandler>();
            services.AddScoped<PasswordVerifyHandler>();
            services.AddScoped<RefreshTokenHandler>();

            // Register Categories feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateCategoryHandler>();
            services.AddScoped<UpdateCategoryHandler>();
            services.AddScoped<DeleteCategoryHandler>();
            services.AddScoped<GetCategoryByIdHandler>();
            services.AddScoped<GetAllCategoriesHandler>();
            services.AddScoped<SearchCategoriesHandler>();
            services.AddScoped<GenerateCategoryDescriptionHandler>();

            // Register Dashboards feature handlers (Vertical Slice Architecture)
            services.AddScoped<GetStatisticsHandler>();
            services.AddScoped<GetRecentActivitiesHandler>();
            services.AddScoped<GetFinancialSummaryHandler>();

            // Register Games feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateGameHandler>();
            services.AddScoped<UpdateGameHandler>();
            services.AddScoped<DeleteGameHandler>();
            services.AddScoped<GetGameByIdHandler>();
            services.AddScoped<SearchGamesHandler>();

            // Register GamePlatforms feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateGamePlatformHandler>();
            services.AddScoped<UpdateGamePlatformHandler>();
            services.AddScoped<DeleteGamePlatformHandler>();
            services.AddScoped<GetAllGamePlatformsHandler>();

            // Register GameStores feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateGameStoreHandler>();
            services.AddScoped<UpdateGameStoreHandler>();
            services.AddScoped<DeleteGameStoreHandler>();
            services.AddScoped<GetAllGameStoresHandler>();

            // Register Images feature handlers (Vertical Slice Architecture)
            services.AddScoped<UploadImageHandler>();

            // Register MovieSeries feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateMovieSeriesHandler>();
            services.AddScoped<UpdateMovieSeriesHandler>();
            services.AddScoped<DeleteMovieSeriesHandler>();
            services.AddScoped<GetMovieSeriesByIdHandler>();
            services.AddScoped<SearchMovieSeriesHandler>();

            // Register MovieSeriesGenres feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateMovieSeriesGenreHandler>();
            services.AddScoped<UpdateMovieSeriesGenreHandler>();
            services.AddScoped<DeleteMovieSeriesGenreHandler>();
            services.AddScoped<GetAllMovieSeriesGenresHandler>();

            // Register WatchPlatforms feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateWatchPlatformHandler>();
            services.AddScoped<UpdateWatchPlatformHandler>();
            services.AddScoped<DeleteWatchPlatformHandler>();
            services.AddScoped<GetAllWatchPlatformsHandler>();

            // Register PersonalNotes feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreatePersonalNoteHandler>();
            services.AddScoped<UpdatePersonalNoteHandler>();
            services.AddScoped<DeletePersonalNoteHandler>();
            services.AddScoped<GetPersonalNoteByIdHandler>();
            services.AddScoped<SearchPersonalNotesHandler>();

            // Register WalletTransactions feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateWalletTransactionHandler>();
            services.AddScoped<UpdateWalletTransactionHandler>();
            services.AddScoped<DeleteWalletTransactionHandler>();
            services.AddScoped<GetWalletTransactionByIdHandler>();
            services.AddScoped<SearchWalletTransactionsHandler>();

            // Register Permissions feature handlers (Vertical Slice Architecture)
            services.AddScoped<GetAllPermissionsHandler>();
            services.AddScoped<GetRolePermissionsHandler>();
            services.AddScoped<AssignPermissionsToRoleHandler>();

            // Register Roles feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateRoleHandler>();
            services.AddScoped<UpdateRoleHandler>();
            services.AddScoped<DeleteRoleHandler>();
            services.AddScoped<GetRoleByIdHandler>();
            services.AddScoped<GetListRolesHandler>();
            services.AddScoped<BulkDeleteRolesHandler>();

            // Register Users feature handlers (Vertical Slice Architecture)
            services.AddScoped<CreateUserHandler>();
            services.AddScoped<UpdateUserHandler>();
            services.AddScoped<DeleteUserHandler>();
            services.AddScoped<GetUserByIdHandler>();
            services.AddScoped<SearchUsersHandler>();
            services.AddScoped<AssignRolesToUserHandler>();
            services.AddScoped<GetUserRolesHandler>();
            services.AddScoped<BulkDeleteUsersHandler>();
            services.AddScoped<ExportUsersHandler>();
            services.AddScoped<GetProfileHandler>();
            services.AddScoped<UpdateProfileHandler>();
            services.AddScoped<ChangePasswordHandler>();

            return services;
        }
    }
}
