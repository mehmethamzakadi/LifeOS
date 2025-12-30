namespace LifeOS.Persistence.Constants;

/// <summary>
/// Veritabanı yapılandırma sabitleri
/// </summary>
public static class DatabaseConstants
{
    /// <summary>
    /// EF Core batch işlemleri için maksimum batch boyutu
    /// </summary>
    public const int MaxBatchSize = 100;

    /// <summary>
    /// Veritabanı komutları için timeout süresi (saniye)
    /// </summary>
    public const int CommandTimeoutSeconds = 30;
}

