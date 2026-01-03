using System.Text.Json.Serialization;

namespace LifeOS.Infrastructure.Models.Spotify;

// Token Response DTOs
internal sealed class SpotifyTokenResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = default!;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

// Currently Playing DTOs
internal sealed class SpotifyCurrentlyPlayingDto
{
    [JsonPropertyName("is_playing")]
    public bool IsPlaying { get; set; }

    [JsonPropertyName("item")]
    public SpotifyTrackItemDto? Item { get; set; }

    [JsonPropertyName("progress_ms")]
    public int? ProgressMs { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }
}

// Track Item DTOs
internal sealed class SpotifyTrackItemDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("artists")]
    public List<SpotifyArtistDto>? Artists { get; set; }

    [JsonPropertyName("album")]
    public SpotifyAlbumDto? Album { get; set; }

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrlsDto? ExternalUrls { get; set; }
}

internal sealed class SpotifyArtistDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImageDto>? Images { get; set; }

    [JsonPropertyName("popularity")]
    public int? Popularity { get; set; }

    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }
}

internal sealed class SpotifyAlbumDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImageDto>? Images { get; set; }
}

internal sealed class SpotifyImageDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }
}

internal sealed class SpotifyExternalUrlsDto
{
    [JsonPropertyName("spotify")]
    public string? Spotify { get; set; }
}

// Recently Played DTOs
internal sealed class SpotifyRecentlyPlayedDto
{
    [JsonPropertyName("items")]
    public List<SpotifyPlayHistoryItemDto>? Items { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

internal sealed class SpotifyPlayHistoryItemDto
{
    [JsonPropertyName("track")]
    public SpotifyTrackItemDto? Track { get; set; }

    [JsonPropertyName("played_at")]
    public long PlayedAt { get; set; }
}

// Playlists DTOs
internal sealed class SpotifyPlaylistsDto
{
    [JsonPropertyName("items")]
    public List<SpotifyPlaylistItemDto>? Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

internal sealed class SpotifyPlaylistItemDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImageDto>? Images { get; set; }

    [JsonPropertyName("owner")]
    public SpotifyUserDto? Owner { get; set; }

    [JsonPropertyName("tracks")]
    public SpotifyTracksInfoDto? Tracks { get; set; }
}

internal sealed class SpotifyTracksInfoDto
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

internal sealed class SpotifyUserDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

// Playlist Tracks DTOs
internal sealed class SpotifyPlaylistTracksDto
{
    [JsonPropertyName("items")]
    public List<SpotifyPlaylistTrackItemDto>? Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

internal sealed class SpotifyPlaylistTrackItemDto
{
    [JsonPropertyName("added_at")]
    public string? AddedAt { get; set; }

    [JsonPropertyName("track")]
    public SpotifyTrackItemDto? Track { get; set; }
}

// Top Tracks DTOs
internal sealed class SpotifyTopTracksDto
{
    [JsonPropertyName("items")]
    public List<SpotifyTrackItemDto>? Items { get; set; }
}

// Top Artists DTOs
internal sealed class SpotifyTopArtistsDto
{
    [JsonPropertyName("items")]
    public List<SpotifyArtistDto>? Items { get; set; }
}

// User Profile DTOs
internal sealed class SpotifyUserProfileDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("display_name")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImageDto>? Images { get; set; }
}

// Track DTOs
internal sealed class SpotifyTrackDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("artists")]
    public List<SpotifyArtistDto>? Artists { get; set; }

    [JsonPropertyName("album")]
    public SpotifyAlbumDto? Album { get; set; }

    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("preview_url")]
    public string? PreviewUrl { get; set; }

    [JsonPropertyName("external_urls")]
    public SpotifyExternalUrlsDto? ExternalUrls { get; set; }
}

// Recommendations DTOs
internal sealed class SpotifyRecommendationsDto
{
    [JsonPropertyName("tracks")]
    public List<SpotifyTrackItemDto>? Tracks { get; set; }
}

// Audio Features DTOs
internal sealed class SpotifyAudioFeaturesDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("valence")]
    public double Valence { get; set; }

    [JsonPropertyName("energy")]
    public double Energy { get; set; }

    [JsonPropertyName("danceability")]
    public double Danceability { get; set; }

    [JsonPropertyName("tempo")]
    public double Tempo { get; set; }

    [JsonPropertyName("key")]
    public int Key { get; set; }

    [JsonPropertyName("mode")]
    public int Mode { get; set; }

    [JsonPropertyName("acousticness")]
    public double Acousticness { get; set; }

    [JsonPropertyName("instrumentalness")]
    public double Instrumentalness { get; set; }

    [JsonPropertyName("liveness")]
    public double Liveness { get; set; }

    [JsonPropertyName("speechiness")]
    public double Speechiness { get; set; }
}

internal sealed class SpotifyAudioFeaturesResponseDto
{
    [JsonPropertyName("audio_features")]
    public List<SpotifyAudioFeaturesDto?>? AudioFeatures { get; set; }
}

// Search DTOs
internal sealed class SpotifySearchDto
{
    [JsonPropertyName("artists")]
    public SpotifySearchArtistsDto? Artists { get; set; }
}

internal sealed class SpotifySearchArtistsDto
{
    [JsonPropertyName("items")]
    public List<SpotifyArtistSearchDto>? Items { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

internal sealed class SpotifyArtistSearchDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("images")]
    public List<SpotifyImageDto>? Images { get; set; }

    [JsonPropertyName("popularity")]
    public int? Popularity { get; set; }

    [JsonPropertyName("genres")]
    public List<string>? Genres { get; set; }
}

// Artist Top Tracks DTOs
internal sealed class SpotifyArtistTopTracksDto
{
    [JsonPropertyName("tracks")]
    public List<SpotifyTrackItemDto>? Tracks { get; set; }
}

