namespace LifeOS.Infrastructure.Options;

/// <summary>
/// Spotify API yapılandırma seçenekleri
/// </summary>
public sealed class SpotifyOptions
{
    public const string SectionName = "SpotifyOptions";

    /// <summary>
    /// Spotify Client ID (Spotify Developer Dashboard'dan alınır)
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Spotify Client Secret (Spotify Developer Dashboard'dan alınır)
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth redirect URI (callback URL)
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>
    /// Spotify Web API base URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.spotify.com/v1";

    /// <summary>
    /// Spotify Accounts API base URL (OAuth için)
    /// </summary>
    public string AccountsBaseUrl { get; set; } = "https://accounts.spotify.com";

    /// <summary>
    /// HTTP isteği için timeout süresi (saniye)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Token yenileme için güvenlik marjı (saniye) - Token süresi dolmadan önce yenileme
    /// </summary>
    public int TokenRefreshMarginSeconds { get; set; } = 300; // 5 dakika
}

