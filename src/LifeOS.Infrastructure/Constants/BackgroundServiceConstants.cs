namespace LifeOS.Infrastructure.Constants;

/// <summary>
/// Background service yapılandırma sabitleri
/// </summary>
public static class BackgroundServiceConstants
{
    /// <summary>
    /// Session cleanup servisinin çalışma aralığı
    /// </summary>
    public static readonly TimeSpan SessionCleanupInterval = TimeSpan.FromHours(6);
}

