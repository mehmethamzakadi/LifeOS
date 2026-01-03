namespace LifeOS.Domain.Services;

/// <summary>
/// Spotify Web API ile iletişim için servis interface'i
/// </summary>
public interface ISpotifyApiService
{
    /// <summary>
    /// OAuth authorization URL'ini oluşturur
    /// </summary>
    string GetAuthorizationUrl(string state);

    /// <summary>
    /// Authorization code ile access token alır
    /// </summary>
    Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh token ile yeni access token alır
    /// </summary>
    Task<SpotifyTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Şu an dinlenen şarkıyı getirir
    /// </summary>
    Task<SpotifyCurrentlyPlayingResponse?> GetCurrentlyPlayingAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Son dinlenen şarkıları getirir
    /// </summary>
    Task<SpotifyRecentlyPlayedResponse> GetRecentlyPlayedAsync(string accessToken, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcının playlist'lerini getirir
    /// </summary>
    Task<SpotifyPlaylistsResponse> GetPlaylistsAsync(string accessToken, int limit = 50, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// Playlist içeriğini getirir
    /// </summary>
    Task<SpotifyPlaylistTracksResponse> GetPlaylistTracksAsync(string accessToken, string playlistId, int limit = 50, int offset = 0, CancellationToken cancellationToken = default);

    /// <summary>
    /// En çok dinlenen şarkıları getirir
    /// </summary>
    Task<SpotifyTopTracksResponse> GetTopTracksAsync(string accessToken, string timeRange = "medium_term", int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// En çok dinlenen sanatçıları getirir
    /// </summary>
    Task<SpotifyTopArtistsResponse> GetTopArtistsAsync(string accessToken, string timeRange = "medium_term", int limit = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kullanıcı profil bilgilerini getirir
    /// </summary>
    Task<SpotifyUserProfileResponse> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Track detaylarını getirir
    /// </summary>
    Task<SpotifyTrackResponse> GetTrackAsync(string accessToken, string trackId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Spotify token response
/// </summary>
public sealed record SpotifyTokenResponse
{
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; } = "Bearer";
    public string? Scope { get; init; }
}

/// <summary>
/// Spotify currently playing response
/// </summary>
public sealed record SpotifyCurrentlyPlayingResponse
{
    public bool IsPlaying { get; init; }
    public SpotifyTrackItem? Item { get; init; }
    public int? ProgressMs { get; init; }
    public long? Timestamp { get; init; }
}

/// <summary>
/// Spotify track item
/// </summary>
public sealed record SpotifyTrackItem
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public List<SpotifyArtist> Artists { get; init; } = new();
    public SpotifyAlbum? Album { get; init; }
    public int DurationMs { get; init; }
    public string? PreviewUrl { get; init; }
    public string? ExternalUrl { get; init; }
}

/// <summary>
/// Spotify artist
/// </summary>
public sealed record SpotifyArtist
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
}

/// <summary>
/// Spotify album
/// </summary>
public sealed record SpotifyAlbum
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public List<SpotifyImage> Images { get; init; } = new();
}

/// <summary>
/// Spotify image
/// </summary>
public sealed record SpotifyImage
{
    public string Url { get; init; } = default!;
    public int? Height { get; init; }
    public int? Width { get; init; }
}

/// <summary>
/// Spotify recently played response
/// </summary>
public sealed record SpotifyRecentlyPlayedResponse
{
    public List<SpotifyPlayHistoryItem> Items { get; init; } = new();
    public string? Next { get; init; }
}

/// <summary>
/// Spotify play history item
/// </summary>
public sealed record SpotifyPlayHistoryItem
{
    public SpotifyTrackItem Track { get; init; } = default!;
    public DateTime PlayedAt { get; init; }
}

/// <summary>
/// Spotify playlists response
/// </summary>
public sealed record SpotifyPlaylistsResponse
{
    public List<SpotifyPlaylistItem> Items { get; init; } = new();
    public int Total { get; init; }
    public string? Next { get; init; }
}

/// <summary>
/// Spotify playlist item
/// </summary>
public sealed record SpotifyPlaylistItem
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public List<SpotifyImage> Images { get; init; } = new();
    public SpotifyUser? Owner { get; init; }
    public int TracksTotal { get; init; }
}

/// <summary>
/// Spotify user
/// </summary>
public sealed record SpotifyUser
{
    public string Id { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Email { get; init; }
}

/// <summary>
/// Spotify playlist tracks response
/// </summary>
public sealed record SpotifyPlaylistTracksResponse
{
    public List<SpotifyPlaylistTrackItem> Items { get; init; } = new();
    public int Total { get; init; }
    public string? Next { get; init; }
}

/// <summary>
/// Spotify playlist track item
/// </summary>
public sealed record SpotifyPlaylistTrackItem
{
    public DateTime? AddedAt { get; init; }
    public SpotifyTrackItem Track { get; init; } = default!;
}

/// <summary>
/// Spotify top tracks response
/// </summary>
public sealed record SpotifyTopTracksResponse
{
    public List<SpotifyTrackItem> Items { get; init; } = new();
    public int Total { get; init; }
}

/// <summary>
/// Spotify top artists response
/// </summary>
public sealed record SpotifyTopArtistsResponse
{
    public List<SpotifyArtist> Items { get; init; } = new();
    public int Total { get; init; }
}

/// <summary>
/// Spotify user profile response
/// </summary>
public sealed record SpotifyUserProfileResponse
{
    public string Id { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Email { get; init; }
    public string? Country { get; init; }
    public List<SpotifyImage> Images { get; init; } = new();
}

/// <summary>
/// Spotify track response
/// </summary>
public sealed record SpotifyTrackResponse
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public List<SpotifyArtist> Artists { get; init; } = new();
    public SpotifyAlbum? Album { get; init; }
    public int DurationMs { get; init; }
    public string? PreviewUrl { get; init; }
    public string? ExternalUrl { get; init; }
    public List<string> Genres { get; init; } = new();
}

