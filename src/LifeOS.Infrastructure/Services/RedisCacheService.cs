using LifeOS.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;

namespace LifeOS.Infrastructure.Services;

/// <summary>
/// Redis cache service implementation using StackExchange.Redis (IConnectionMultiplexer).
/// All operations use raw Redis String operations to ensure consistent data types and eliminate WRONGTYPE errors.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly string _keyPrefix;
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public RedisCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        IConfiguration? configuration = null)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));

        // Get InstanceName from configuration or use default
        var instanceName = configuration?.GetSection("RedisCache:InstanceName").Value
            ?? configuration?.GetValue<string>("Redis:InstanceName")
            ?? "LifeOS";

        // Remove trailing underscore if present (for compatibility with old "LifeOS_" format)
        instanceName = instanceName.TrimEnd('_');

        // Use colon separator for key prefix (e.g., "LifeOS:key")
        _keyPrefix = $"{instanceName}:";
    }

    /// <summary>
    /// Gets the prefixed key for Redis operations.
    /// Format: "{InstanceName}:{key}"
    /// </summary>
    private string GetPrefixedKey(string key) => $"{_keyPrefix}{key}";

    /// <summary>
    /// Adds a key-value pair to Redis with optional expiration.
    /// Uses StringSetAsync to ensure consistent String data type.
    /// 
    /// ⚠️ WARNING: Do not serialize Entity objects directly. Use DTOs or primitive types to avoid circular reference errors.
    /// </summary>
    public async Task Add(string key, object data, DateTimeOffset? absExpr, TimeSpan? sldExpr)
    {
        if (data is null)
        {
            return;
        }

        // ✅ Mantıksal hata düzeltmesi: Entity'lerin direkt serialize edilmesini engelle
        var dataType = data.GetType();
        if (dataType.Namespace?.Contains("LifeOS.Domain.Entities") == true)
        {
            throw new InvalidOperationException(
                $"Cannot serialize Entity type '{dataType.Name}' directly. Use DTOs or primitive types to avoid circular reference errors.");
        }

        var database = _connectionMultiplexer.GetDatabase();
        string json = JsonSerializer.Serialize(data, SerializerOptions);
        var prefixedKey = GetPrefixedKey(key);

        // Calculate expiration TimeSpan
        TimeSpan? expiration = null;
        if (absExpr.HasValue)
        {
            expiration = absExpr.Value.Subtract(DateTimeOffset.UtcNow);
            // If expiration is in the past, don't set it
            if (expiration.Value.TotalSeconds <= 0)
            {
                return;
            }
        }
        else if (sldExpr.HasValue)
        {
            expiration = sldExpr;
        }

        await database.StringSetAsync(prefixedKey, json, expiration);
    }

    /// <summary>
    /// Checks if a key exists in Redis.
    /// Uses KeyExistsAsync for efficient existence check without reading the value.
    /// </summary>
    public async Task<bool> AnyAsync(string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var prefixedKey = GetPrefixedKey(key);
        return await database.KeyExistsAsync(prefixedKey);
    }

    /// <summary>
    /// Gets a value from Redis and deserializes it to the specified type.
    /// Uses StringGetAsync to read String data type.
    /// </summary>
    public async Task<T?> Get<T>(string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var prefixedKey = GetPrefixedKey(key);

        var value = await database.StringGetAsync(prefixedKey);
        if (!value.HasValue)
        {
            return default;
        }

        var json = value.ToString();
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    /// <summary>
    /// Removes a key from Redis.
    /// Uses KeyDeleteAsync to delete the key.
    /// </summary>
    public async Task Remove(string key)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var prefixedKey = GetPrefixedKey(key);
        await database.KeyDeleteAsync(prefixedKey);
    }

    /// <summary>
    /// Adds a key to Redis only if it doesn't exist (SETNX - SET if Not eXists).
    /// Atomic operation that prevents race conditions.
    /// Uses StringSetAsync with When.NotExists to ensure consistent String data type.
    /// 
    /// ⚠️ WARNING: Do not serialize Entity objects directly. Use DTOs or primitive types to avoid circular reference errors.
    /// </summary>
    public async Task<bool> AddIfNotExists(string key, object data, DateTimeOffset? absExpr, TimeSpan? sldExpr)
    {
        if (data is null)
        {
            return false;
        }

        // ✅ Mantıksal hata düzeltmesi: Entity'lerin direkt serialize edilmesini engelle
        var dataType = data.GetType();
        if (dataType.Namespace?.Contains("LifeOS.Domain.Entities") == true)
        {
            throw new InvalidOperationException(
                $"Cannot serialize Entity type '{dataType.Name}' directly. Use DTOs or primitive types to avoid circular reference errors.");
        }

        var database = _connectionMultiplexer.GetDatabase();
        string json = JsonSerializer.Serialize(data, SerializerOptions);
        var prefixedKey = GetPrefixedKey(key);

        // Calculate expiration TimeSpan
        TimeSpan? expiration = null;
        if (absExpr.HasValue)
        {
            expiration = absExpr.Value.Subtract(DateTimeOffset.UtcNow);
            // If expiration is in the past, don't set it
            if (expiration.Value.TotalSeconds <= 0)
            {
                return false;
            }
        }
        else if (sldExpr.HasValue)
        {
            expiration = sldExpr;
        }

        // SETNX - atomic operation: set only if not exists
        var wasSet = await database.StringSetAsync(
            prefixedKey,
            json,
            expiration,
            When.NotExists);

        return wasSet;
    }
}
