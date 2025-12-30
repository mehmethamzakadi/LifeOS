using LifeOS.API.Configuration;
using LifeOS.API.Extensions;
using LifeOS.Application;
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
using LifeOS.Application.Features.Books.SearchBooks;
using LifeOS.Application.Features.Categories.CreateCategory;
using LifeOS.Application.Features.Categories.UpdateCategory;
using LifeOS.Application.Features.Categories.DeleteCategory;
using LifeOS.Application.Features.Categories.GetCategoryById;
using LifeOS.Application.Features.Categories.GetAllCategories;
using LifeOS.Application.Features.Categories.SearchCategories;
using LifeOS.Application.Features.Dashboards.GetStatistics;
using LifeOS.Application.Features.Games.CreateGame;
using LifeOS.Application.Features.Games.UpdateGame;
using LifeOS.Application.Features.Games.DeleteGame;
using LifeOS.Application.Features.Games.GetGameById;
using LifeOS.Application.Features.Games.SearchGames;
using LifeOS.Application.Features.MovieSeries.CreateMovieSeries;
using LifeOS.Application.Features.MovieSeries.UpdateMovieSeries;
using LifeOS.Application.Features.MovieSeries.DeleteMovieSeries;
using LifeOS.Application.Features.MovieSeries.GetMovieSeriesById;
using LifeOS.Application.Features.MovieSeries.SearchMovieSeries;
using LifeOS.Application.Features.Permissions.GetAllPermissions;
using LifeOS.Application.Features.Permissions.GetRolePermissions;
using LifeOS.Application.Features.Permissions.AssignPermissionsToRole;
using LifeOS.Application.Features.PersonalNotes.CreatePersonalNote;
using LifeOS.Application.Features.PersonalNotes.UpdatePersonalNote;
using LifeOS.Application.Features.PersonalNotes.DeletePersonalNote;
using LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;
using LifeOS.Application.Features.PersonalNotes.SearchPersonalNotes;
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
using LifeOS.Application.Features.WalletTransactions.CreateWalletTransaction;
using LifeOS.Application.Features.WalletTransactions.UpdateWalletTransaction;
using LifeOS.Application.Features.WalletTransactions.DeleteWalletTransaction;
using LifeOS.Application.Features.WalletTransactions.GetWalletTransactionById;
using LifeOS.Application.Features.WalletTransactions.SearchWalletTransactions;
using LifeOS.Application.Features.Images.UploadImage;
using LifeOS.Infrastructure;
using LifeOS.Persistence;
using LifeOS.Persistence.DatabaseInitializer;

var builder = WebApplication.CreateBuilder(args);

// ✅ Kestrel Server Optimizations
builder.ConfigureKestrelServer();

// ✅ Serilog yapılandırmasını yükle
builder.ConfigureSerilog();

// ✅ CORS Policy yapılandırması
builder.Services.AddCorsPolicy(builder.Configuration, out var corsPolicyName);

// ✅ Katman servisleri
builder.Services.AddConfigurePersistenceServices(builder.Configuration);
builder.Services.AddConfigureApplicationServices(builder.Configuration);
builder.Services.AddConfigureInfrastructureServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();

// ✅ Response Optimization (Caching & Compression)
builder.Services.AddResponseOptimization();

// ✅ Rate Limiting
builder.Services.AddRateLimiting(builder.Configuration);

// ✅ API Services (OpenAPI, Routing)
builder.Services.AddApiControllers();

var app = builder.Build();

// ✅ Static Files & Image Storage
app.UseStaticFilesWithImageStorage();

// ✅ API Documentation (Development only)
app.UseApiDocumentation();

// ✅ Veritabanı başlatma ve gerekli tabloları oluştur
await using AsyncServiceScope scope = app.Services.CreateAsyncScope();
var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
await dbInitializer.InitializeAsync(scope.ServiceProvider, app.Lifetime.ApplicationStopping);
await dbInitializer.EnsurePostgreSqlSerilogTableAsync(builder.Configuration, app.Lifetime.ApplicationStopping);

// ✅ Middleware Pipeline (Endpoint'lerden ÖNCE olmalı)
app.UseApiMiddleware(corsPolicyName);

// ✅ Serilog Request Logging (Routing'den sonra route bilgisini loglamak için)
app.UseSerilogRequestLogging();

// ✅ Auth Endpoints (Vertical Slice Architecture)
RegisterEndpoint.MapEndpoint(app);
LoginEndpoint.MapEndpoint(app);
LogoutEndpoint.MapEndpoint(app);
RefreshTokenEndpoint.MapEndpoint(app);
PasswordResetEndpoint.MapEndpoint(app);
PasswordVerifyEndpoint.MapEndpoint(app);

// ✅ Users Endpoints (Vertical Slice Architecture)
CreateUserEndpoint.MapEndpoint(app);
GetUserByIdEndpoint.MapEndpoint(app);
UpdateUserEndpoint.MapEndpoint(app);
DeleteUserEndpoint.MapEndpoint(app);
SearchUsersEndpoint.MapEndpoint(app);
AssignRolesToUserEndpoint.MapEndpoint(app);
GetUserRolesEndpoint.MapEndpoint(app);
BulkDeleteUsersEndpoint.MapEndpoint(app);
ExportUsersEndpoint.MapEndpoint(app);
GetProfileEndpoint.MapEndpoint(app);
UpdateProfileEndpoint.MapEndpoint(app);
ChangePasswordEndpoint.MapEndpoint(app);

// ✅ Roles Endpoints (Vertical Slice Architecture)
CreateRoleEndpoint.MapEndpoint(app);
GetRoleByIdEndpoint.MapEndpoint(app);
UpdateRoleEndpoint.MapEndpoint(app);
DeleteRoleEndpoint.MapEndpoint(app);
GetListRolesEndpoint.MapEndpoint(app);
BulkDeleteRolesEndpoint.MapEndpoint(app);

// ✅ Permissions Endpoints (Vertical Slice Architecture)
GetAllPermissionsEndpoint.MapEndpoint(app);
GetRolePermissionsEndpoint.MapEndpoint(app);
AssignPermissionsToRoleEndpoint.MapEndpoint(app);

// ✅ Books Endpoints (Vertical Slice Architecture)
CreateBookEndpoint.MapEndpoint(app);
UpdateBookEndpoint.MapEndpoint(app);
DeleteBookEndpoint.MapEndpoint(app);
GetBookByIdEndpoint.MapEndpoint(app);
SearchBooksEndpoint.MapEndpoint(app);

// ✅ Categories Endpoints (Vertical Slice Architecture)
CreateCategoryEndpoint.MapEndpoint(app);
UpdateCategoryEndpoint.MapEndpoint(app);
DeleteCategoryEndpoint.MapEndpoint(app);
GetCategoryByIdEndpoint.MapEndpoint(app);
GetAllCategoriesEndpoint.MapEndpoint(app);
SearchCategoriesEndpoint.MapEndpoint(app);

// ✅ Games Endpoints (Vertical Slice Architecture)
CreateGameEndpoint.MapEndpoint(app);
UpdateGameEndpoint.MapEndpoint(app);
DeleteGameEndpoint.MapEndpoint(app);
GetGameByIdEndpoint.MapEndpoint(app);
SearchGamesEndpoint.MapEndpoint(app);

// ✅ MovieSeries Endpoints (Vertical Slice Architecture)
CreateMovieSeriesEndpoint.MapEndpoint(app);
UpdateMovieSeriesEndpoint.MapEndpoint(app);
DeleteMovieSeriesEndpoint.MapEndpoint(app);
GetMovieSeriesByIdEndpoint.MapEndpoint(app);
SearchMovieSeriesEndpoint.MapEndpoint(app);

// ✅ PersonalNotes Endpoints (Vertical Slice Architecture)
CreatePersonalNoteEndpoint.MapEndpoint(app);
UpdatePersonalNoteEndpoint.MapEndpoint(app);
DeletePersonalNoteEndpoint.MapEndpoint(app);
GetPersonalNoteByIdEndpoint.MapEndpoint(app);
SearchPersonalNotesEndpoint.MapEndpoint(app);

// ✅ WalletTransactions Endpoints (Vertical Slice Architecture)
CreateWalletTransactionEndpoint.MapEndpoint(app);
UpdateWalletTransactionEndpoint.MapEndpoint(app);
DeleteWalletTransactionEndpoint.MapEndpoint(app);
GetWalletTransactionByIdEndpoint.MapEndpoint(app);
SearchWalletTransactionsEndpoint.MapEndpoint(app);

// ✅ Images Endpoints (Vertical Slice Architecture)
UploadImageEndpoint.MapEndpoint(app);

// ✅ Dashboards Endpoints (Vertical Slice Architecture)
GetStatisticsEndpoint.MapEndpoint(app);

await app.RunAsync();

