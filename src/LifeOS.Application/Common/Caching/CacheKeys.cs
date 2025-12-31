using LifeOS.Domain.Common.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LifeOS.Application.Common.Caching;

/// <summary>
/// Centers cache key conventions for frequently used resources so that invalidation stays consistent.
/// Uses version-based cache invalidation strategy to efficiently invalidate all related cache entries.
/// </summary>
public static class CacheKeys
{
    #region Single Entity Keys
    
    public static string Category(Guid categoryId) => $"category:{categoryId}";

    public static string User(Guid userId) => $"user:{userId}";

    public static string UserRoles(Guid userId) => $"user:{userId}:roles";

    public static string UserPermissions(Guid userId) => $"user:{userId}:permissions";

    public static string Role(Guid roleId) => $"role:{roleId}";

    public static string RolePermissions(Guid roleId) => $"role:{roleId}:permissions";

    public static string Book(Guid bookId) => $"book:{bookId}";

    public static string Game(Guid gameId) => $"game:{gameId}";

    public static string MovieSeries(Guid movieSeriesId) => $"movieseries:{movieSeriesId}";

    public static string PersonalNote(Guid personalNoteId) => $"personalnote:{personalNoteId}";

    public static string WalletTransaction(Guid walletTransactionId) => $"wallettransaction:{walletTransactionId}";
    
    #endregion

    #region Version Keys (for efficient cache invalidation)
    
    /// <summary>
    /// Version key for category grid. When this changes, all category grid caches become stale.
    /// </summary>
    public static string CategoryGridVersion() => "version:category:grid";

    /// <summary>
    /// Version key for all user lists. When this changes, all user list caches become stale.
    /// </summary>
    public static string UserListVersion() => "version:users:list";

    /// <summary>
    /// Version key for all role lists. When this changes, all role list caches become stale.
    /// </summary>
    public static string RoleListVersion() => "version:roles:list";

    /// <summary>
    /// Version key for all category lists. When this changes, all category list caches become stale.
    /// </summary>
    public static string CategoryListVersion() => "version:categories:list";

    public static string BookGridVersion() => "version:book:grid";

    public static string GameGridVersion() => "version:game:grid";

    public static string MovieSeriesGridVersion() => "version:movieseries:grid";

    public static string PersonalNoteGridVersion() => "version:personalnote:grid";

    public static string WalletTransactionGridVersion() => "version:wallettransaction:grid";

    public static string GamePlatformListVersion() => "version:gameplatform:list";

    public static string GameStoreListVersion() => "version:gamestore:list";

    public static string WatchPlatformListVersion() => "version:watchplatform:list";

    public static string MovieSeriesGenreListVersion() => "version:movieseriesgenre:list";
    
    #endregion

    #region Versioned List Keys
    
    /// <summary>
    /// Cache key for category grid with version token and dynamic query support.
    /// </summary>
    public static string CategoryGrid(string versionToken, int pageIndex, int pageSize, DynamicQuery? dynamicQuery)
    {
        string dynamicSegment = dynamicQuery is null
            ? "none"
            : ComputeHash(dynamicQuery);

        return $"category:grid:{versionToken}:{pageIndex}:{pageSize}:{dynamicSegment}";
    }

    public static string BookGrid(string versionToken, int pageIndex, int pageSize, DynamicQuery? dynamicQuery)
    {
        string dynamicSegment = dynamicQuery is null
            ? "none"
            : ComputeHash(dynamicQuery);

        return $"book:grid:{versionToken}:{pageIndex}:{pageSize}:{dynamicSegment}";
    }

    public static string GameGrid(string versionToken, int pageIndex, int pageSize, DynamicQuery? dynamicQuery)
    {
        string dynamicSegment = dynamicQuery is null
            ? "none"
            : ComputeHash(dynamicQuery);

        return $"game:grid:{versionToken}:{pageIndex}:{pageSize}:{dynamicSegment}";
    }

    public static string MovieSeriesGrid(string versionToken, int pageIndex, int pageSize, DynamicQuery? dynamicQuery)
    {
        string dynamicSegment = dynamicQuery is null
            ? "none"
            : ComputeHash(dynamicQuery);

        return $"movieseries:grid:{versionToken}:{pageIndex}:{pageSize}:{dynamicSegment}";
    }

    public static string PersonalNoteGrid(string versionToken, int pageIndex, int pageSize, DynamicQuery? dynamicQuery)
    {
        string dynamicSegment = dynamicQuery is null
            ? "none"
            : ComputeHash(dynamicQuery);

        return $"personalnote:grid:{versionToken}:{pageIndex}:{pageSize}:{dynamicSegment}";
    }

    public static string WalletTransactionGrid(string versionToken, int pageIndex, int pageSize, DynamicQuery? dynamicQuery)
    {
        string dynamicSegment = dynamicQuery is null
            ? "none"
            : ComputeHash(dynamicQuery);

        return $"wallettransaction:grid:{versionToken}:{pageIndex}:{pageSize}:{dynamicSegment}";
    }
    
    #endregion

    #region Helpers
    
    private static readonly JsonSerializerOptions KeySerializerOptions = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static string ComputeHash<T>(T value)
    {
        string json = JsonSerializer.Serialize(value, KeySerializerOptions);
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return Convert.ToHexString(hashBytes);
    }
    
    #endregion
}

/// <summary>
/// Provides TTL recommendations for cache entries. Keep the values conservative until usage patterns are validated.
/// </summary>
public static class CacheDurations
{
    public static readonly TimeSpan Category = TimeSpan.FromHours(12);

    public static readonly TimeSpan CategoryGrid = TimeSpan.FromDays(30);

    public static readonly TimeSpan Book = TimeSpan.FromHours(12);

    public static readonly TimeSpan BookGrid = TimeSpan.FromDays(30);

    public static readonly TimeSpan Game = TimeSpan.FromHours(12);

    public static readonly TimeSpan GameGrid = TimeSpan.FromDays(30);

    public static readonly TimeSpan MovieSeries = TimeSpan.FromHours(12);

    public static readonly TimeSpan MovieSeriesGrid = TimeSpan.FromDays(30);

    public static readonly TimeSpan PersonalNote = TimeSpan.FromHours(12);

    public static readonly TimeSpan PersonalNoteGrid = TimeSpan.FromDays(30);

    public static readonly TimeSpan WalletTransaction = TimeSpan.FromHours(12);

    public static readonly TimeSpan WalletTransactionGrid = TimeSpan.FromDays(30);
}
