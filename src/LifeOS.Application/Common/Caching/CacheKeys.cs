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
}
