namespace LifeOS.Application.Abstractions;

public interface ICacheService
{
    Task<T?> Get<T>(string key);
    Task Add(string key, object data, DateTimeOffset? absExpr, TimeSpan? sldExpr);
    Task<bool> AnyAsync(string key);
    Task Remove(string key);
    /// <summary>
    /// Redis'e key ekler, ancak sadece key yoksa (SETNX - SET if Not eXists).
    /// Atomic işlem - race condition'ı önler.
    /// </summary>
    /// <param name="key">Cache key</param>
    /// <param name="data">Cache edilecek veri</param>
    /// <param name="absExpr">Absolute expiration</param>
    /// <param name="sldExpr">Sliding expiration (ignored if absExpr is provided)</param>
    /// <returns>True if key was added (didn't exist), False if key already exists</returns>
    Task<bool> AddIfNotExists(string key, object data, DateTimeOffset? absExpr, TimeSpan? sldExpr);
}
