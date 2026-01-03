using LifeOS.Domain.Services;
using LifeOS.Infrastructure.Models.Spotify;
using LifeOS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LifeOS.Infrastructure.Services;

/// <summary>
/// Spotify Web API ile iletişim servisi
/// Best practices: IHttpClientFactory, retry policy, logging, proper error handling
/// </summary>
public sealed class SpotifyApiService : ISpotifyApiService
{
    private const string SpotifyApiClientName = "SpotifyApiClient";
    private const string SpotifyAccountsClientName = "SpotifyAccountsClient";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SpotifyOptions _options;
    private readonly ILogger<SpotifyApiService> _logger;

    public SpotifyApiService(
        IHttpClientFactory httpClientFactory,
        IOptions<SpotifyOptions> options,
        ILogger<SpotifyApiService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public string GetAuthorizationUrl(string state)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId))
        {
            throw new InvalidOperationException("Spotify ClientId yapılandırılmamış. Lütfen appsettings.json dosyasında SpotifyOptions.ClientId değerini ayarlayın.");
        }

        if (string.IsNullOrWhiteSpace(_options.RedirectUri))
        {
            throw new InvalidOperationException("Spotify RedirectUri yapılandırılmamış. Lütfen appsettings.json dosyasında SpotifyOptions.RedirectUri değerini ayarlayın.");
        }

        var scopes = new[]
        {
            "user-read-private",
            "user-read-email",
            "user-read-currently-playing",
            "user-read-recently-played",
            "user-top-read",
            "playlist-read-private",
            "playlist-read-collaborative"
        };

        var queryParams = new Dictionary<string, string>
        {
            { "client_id", _options.ClientId },
            { "response_type", "code" },
            { "redirect_uri", _options.RedirectUri },
            { "scope", string.Join(" ", scopes) },
            { "state", state },
            { "show_dialog", "false" }
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return $"{_options.AccountsBaseUrl}/authorize?{queryString}";
    }

    public async Task<SpotifyTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyAccountsClientName);

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", _options.RedirectUri }
        };

        var content = new FormUrlEncodedContent(requestBody);
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/token")
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

        _logger.LogDebug("Spotify token exchange isteği gönderiliyor");

        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Spotify token exchange hatası: {StatusCode}, {Response}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Spotify token exchange hatası: {response.StatusCode} - {responseContent}", null, response.StatusCode);
        }

        var tokenData = JsonSerializer.Deserialize<SpotifyTokenResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Token response parse edilemedi");

        return new SpotifyTokenResponse
        {
            AccessToken = tokenData.AccessToken,
            RefreshToken = tokenData.RefreshToken ?? throw new InvalidOperationException("Refresh token alınamadı"),
            ExpiresIn = tokenData.ExpiresIn,
            TokenType = tokenData.TokenType ?? "Bearer",
            Scope = tokenData.Scope
        };
    }

    public async Task<SpotifyTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyAccountsClientName);

        var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };

        var content = new FormUrlEncodedContent(requestBody);
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/token")
        {
            Content = content
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

        _logger.LogDebug("Spotify token refresh isteği gönderiliyor");

        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Spotify token refresh hatası: {StatusCode}, {Response}", response.StatusCode, responseContent);
            throw new HttpRequestException($"Spotify token refresh hatası: {response.StatusCode} - {responseContent}", null, response.StatusCode);
        }

        var tokenData = JsonSerializer.Deserialize<SpotifyTokenResponseDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Token response parse edilemedi");

        // Refresh token yanıtında refresh_token opsiyonel, eğer yoksa eski refresh token'ı kullan
        return new SpotifyTokenResponse
        {
            AccessToken = tokenData.AccessToken,
            RefreshToken = tokenData.RefreshToken ?? refreshToken,
            ExpiresIn = tokenData.ExpiresIn,
            TokenType = tokenData.TokenType ?? "Bearer",
            Scope = tokenData.Scope
        };
    }

    public async Task<SpotifyCurrentlyPlayingResponse?> GetCurrentlyPlayingAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("/v1/me/player/currently-playing", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            // Şu an hiçbir şey çalmıyor
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Spotify currently playing hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            return null;
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyCurrentlyPlayingDto>(cancellationToken: cancellationToken);
        if (data == null)
        {
            return null;
        }

        return MapToCurrentlyPlayingResponse(data);
    }

    public async Task<SpotifyRecentlyPlayedResponse> GetRecentlyPlayedAsync(string accessToken, int limit = 50, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"/v1/me/player/recently-played?limit={limit}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify recently played hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify recently played hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyRecentlyPlayedDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Recently played response parse edilemedi");

        return new SpotifyRecentlyPlayedResponse
        {
            Items = data.Items?.Select(MapToPlayHistoryItem).ToList() ?? new List<SpotifyPlayHistoryItem>(),
            Next = data.Next
        };
    }

    public async Task<SpotifyPlaylistsResponse> GetPlaylistsAsync(string accessToken, int limit = 50, int offset = 0, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"/v1/me/playlists?limit={limit}&offset={offset}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify playlists hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify playlists hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyPlaylistsDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Playlists response parse edilemedi");

        return new SpotifyPlaylistsResponse
        {
            Items = data.Items?.Select(MapToPlaylistItem).ToList() ?? new List<SpotifyPlaylistItem>(),
            Total = data.Total,
            Next = data.Next
        };
    }

    public async Task<SpotifyPlaylistTracksResponse> GetPlaylistTracksAsync(string accessToken, string playlistId, int limit = 50, int offset = 0, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"/v1/playlists/{playlistId}/tracks?limit={limit}&offset={offset}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify playlist tracks hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify playlist tracks hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyPlaylistTracksDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Playlist tracks response parse edilemedi");

        return new SpotifyPlaylistTracksResponse
        {
            Items = data.Items?.Select(MapToPlaylistTrackItem).ToList() ?? new List<SpotifyPlaylistTrackItem>(),
            Total = data.Total,
            Next = data.Next
        };
    }

    public async Task<SpotifyTopTracksResponse> GetTopTracksAsync(string accessToken, string timeRange = "medium_term", int limit = 20, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"/v1/me/top/tracks?time_range={timeRange}&limit={limit}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify top tracks hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify top tracks hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyTopTracksDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Top tracks response parse edilemedi");

        return new SpotifyTopTracksResponse
        {
            Items = data.Items?.Select(MapToTrackItem).ToList() ?? new List<SpotifyTrackItem>(),
            Total = data.Items?.Count ?? 0
        };
    }

    public async Task<SpotifyTopArtistsResponse> GetTopArtistsAsync(string accessToken, string timeRange = "medium_term", int limit = 20, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var url = $"/v1/me/top/artists?time_range={timeRange}&limit={limit}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify top artists hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify top artists hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyTopArtistsDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Top artists response parse edilemedi");

        return new SpotifyTopArtistsResponse
        {
            Items = data.Items?.Select(MapToArtist).ToList() ?? new List<SpotifyArtist>(),
            Total = data.Items?.Count ?? 0
        };
    }

    public async Task<SpotifyUserProfileResponse> GetUserProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync("/v1/me", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify user profile hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify user profile hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyUserProfileDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("User profile response parse edilemedi");

        return new SpotifyUserProfileResponse
        {
            Id = data.Id ?? string.Empty,
            DisplayName = data.DisplayName ?? string.Empty,
            Email = data.Email,
            Country = data.Country,
            Images = data.Images?.Select(i => new SpotifyImage { Url = i.Url ?? string.Empty, Height = i.Height, Width = i.Width }).ToList() ?? new List<SpotifyImage>()
        };
    }

    public async Task<SpotifyTrackResponse> GetTrackAsync(string accessToken, string trackId, CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var response = await httpClient.GetAsync($"/v1/tracks/{trackId}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify track hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify track hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyTrackDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Track response parse edilemedi");

        return MapToTrackResponse(data);
    }

    public async Task<SpotifyRecommendationsResponse> GetRecommendationsAsync(
        string accessToken,
        List<string>? seedTracks = null,
        List<string>? seedArtists = null,
        List<string>? seedGenres = null,
        double? targetValence = null,
        double? targetEnergy = null,
        double? targetDanceability = null,
        double? minTempo = null,
        double? maxTempo = null,
        string? market = null,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Query parametrelerini oluştur
        var queryParams = new List<string>();

        // Spotify API kuralı: En az 1 seed olmalı (track, artist veya genre)
        // Toplam seed sayısı 5'i geçmemeli
        // Not: Spotify ID'leri zaten URL-safe karakterlerden oluşur, virgül encode edilmemeli
        var totalSeeds = 0;

        if (seedTracks != null && seedTracks.Any() && totalSeeds < 5)
        {
            var tracks = seedTracks.Take(5 - totalSeeds).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            if (tracks.Any())
            {
                var tracksValue = string.Join(",", tracks);
                queryParams.Add($"seed_tracks={tracksValue}"); // Virgül encode edilmez
                totalSeeds += tracks.Count;
            }
        }

        if (seedArtists != null && seedArtists.Any() && totalSeeds < 5)
        {
            var artists = seedArtists.Take(5 - totalSeeds).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
            if (artists.Any())
            {
                var artistsValue = string.Join(",", artists);
                queryParams.Add($"seed_artists={artistsValue}"); // Virgül encode edilmez
                totalSeeds += artists.Count;
            }
        }

        if (seedGenres != null && seedGenres.Any() && totalSeeds < 5)
        {
            var genres = seedGenres.Take(5 - totalSeeds).Where(g => !string.IsNullOrWhiteSpace(g)).ToList();
            if (genres.Any())
            {
                var genresValue = string.Join(",", genres);
                queryParams.Add($"seed_genres={genresValue}"); // Virgül encode edilmez
                totalSeeds += genres.Count;
            }
        }

        // En az 1 seed garantisi
        if (totalSeeds == 0)
        {
            throw new InvalidOperationException("En az 1 seed (track, artist veya genre) belirtilmelidir.");
        }

        if (targetValence.HasValue)
        {
            queryParams.Add($"target_valence={targetValence.Value:F2}");
        }

        if (targetEnergy.HasValue)
        {
            queryParams.Add($"target_energy={targetEnergy.Value:F2}");
        }

        if (targetDanceability.HasValue)
        {
            queryParams.Add($"target_danceability={targetDanceability.Value:F2}");
        }

        if (minTempo.HasValue)
        {
            queryParams.Add($"min_tempo={minTempo.Value:F1}");
        }

        if (maxTempo.HasValue)
        {
            queryParams.Add($"max_tempo={maxTempo.Value:F1}");
        }

        if (!string.IsNullOrWhiteSpace(market))
        {
            queryParams.Add($"market={market}");
        }

        queryParams.Add($"limit={Math.Clamp(limit, 1, 100)}");

        var url = $"/v1/recommendations?{string.Join("&", queryParams)}";
        var response = await httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Spotify recommendations hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Spotify recommendations hatası: {response.StatusCode}", null, response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<SpotifyRecommendationsDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Recommendations response parse edilemedi");

        return new SpotifyRecommendationsResponse
        {
            Tracks = data.Tracks?.Select(MapToTrackItem).ToList() ?? new List<SpotifyTrackItem>()
        };
    }

    public async Task<List<SpotifyAudioFeaturesResponse>> GetAudioFeaturesAsync(
        string accessToken,
        List<string> trackIds,
        CancellationToken cancellationToken = default)
    {
        if (trackIds == null || !trackIds.Any())
        {
            return new List<SpotifyAudioFeaturesResponse>();
        }

        var httpClient = _httpClientFactory.CreateClient(SpotifyApiClientName);
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Spotify API maksimum 100 track ID kabul eder
        var allFeatures = new List<SpotifyAudioFeaturesResponse>();
        var batches = trackIds.Chunk(100);

        foreach (var batch in batches)
        {
            var ids = string.Join(",", batch);
            var url = $"/v1/audio-features?ids={ids}";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Spotify audio features hatası: {StatusCode}, {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Spotify audio features hatası: {response.StatusCode}", null, response.StatusCode);
            }

            var data = await response.Content.ReadFromJsonAsync<SpotifyAudioFeaturesResponseDto>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Audio features response parse edilemedi");

            if (data.AudioFeatures != null)
            {
                foreach (var feature in data.AudioFeatures)
                {
                    if (feature != null)
                    {
                        allFeatures.Add(new SpotifyAudioFeaturesResponse
                        {
                            Id = feature.Id ?? string.Empty,
                            Valence = feature.Valence,
                            Energy = feature.Energy,
                            Danceability = feature.Danceability,
                            Tempo = feature.Tempo,
                            Key = feature.Key,
                            Mode = feature.Mode,
                            Acousticness = feature.Acousticness,
                            Instrumentalness = feature.Instrumentalness,
                            Liveness = feature.Liveness,
                            Speechiness = feature.Speechiness
                        });
                    }
                }
            }
        }

        return allFeatures;
    }

    #region Mapping Methods

    private SpotifyCurrentlyPlayingResponse MapToCurrentlyPlayingResponse(SpotifyCurrentlyPlayingDto dto)
    {
        return new SpotifyCurrentlyPlayingResponse
        {
            IsPlaying = dto.IsPlaying,
            Item = dto.Item != null ? MapToTrackItem(dto.Item) : null,
            ProgressMs = dto.ProgressMs,
            Timestamp = dto.Timestamp
        };
    }

    private SpotifyTrackItem MapToTrackItem(SpotifyTrackItemDto dto)
    {
        return new SpotifyTrackItem
        {
            Id = dto.Id ?? string.Empty,
            Name = dto.Name ?? string.Empty,
            Artists = dto.Artists?.Select(a => new SpotifyArtist { Id = a.Id ?? string.Empty, Name = a.Name ?? string.Empty }).ToList() ?? new List<SpotifyArtist>(),
            Album = dto.Album != null ? MapToAlbum(dto.Album) : null,
            DurationMs = dto.DurationMs,
            PreviewUrl = dto.PreviewUrl,
            ExternalUrl = dto.ExternalUrls?.Spotify
        };
    }

    private SpotifyAlbum MapToAlbum(SpotifyAlbumDto dto)
    {
        return new SpotifyAlbum
        {
            Id = dto.Id ?? string.Empty,
            Name = dto.Name ?? string.Empty,
            Images = dto.Images?.Select(i => new SpotifyImage { Url = i.Url ?? string.Empty, Height = i.Height, Width = i.Width }).ToList() ?? new List<SpotifyImage>()
        };
    }

    private SpotifyPlayHistoryItem MapToPlayHistoryItem(SpotifyPlayHistoryItemDto dto)
    {
        return new SpotifyPlayHistoryItem
        {
            Track = MapToTrackItem(dto.Track ?? throw new InvalidOperationException("Track is null")),
            PlayedAt = DateTimeOffset.FromUnixTimeMilliseconds(dto.PlayedAt).DateTime
        };
    }

    private SpotifyPlaylistItem MapToPlaylistItem(SpotifyPlaylistItemDto dto)
    {
        return new SpotifyPlaylistItem
        {
            Id = dto.Id ?? string.Empty,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description,
            Images = dto.Images?.Select(i => new SpotifyImage { Url = i.Url ?? string.Empty, Height = i.Height, Width = i.Width }).ToList() ?? new List<SpotifyImage>(),
            Owner = dto.Owner != null ? new SpotifyUser { Id = dto.Owner.Id ?? string.Empty, DisplayName = dto.Owner.DisplayName ?? string.Empty } : null,
            TracksTotal = dto.Tracks?.Total ?? 0
        };
    }

    private SpotifyPlaylistTrackItem MapToPlaylistTrackItem(SpotifyPlaylistTrackItemDto dto)
    {
        DateTime? addedAt = null;
        if (!string.IsNullOrWhiteSpace(dto.AddedAt))
        {
            if (DateTimeOffset.TryParse(dto.AddedAt, out var parsedDate))
            {
                addedAt = parsedDate.DateTime;
            }
        }

        return new SpotifyPlaylistTrackItem
        {
            AddedAt = addedAt,
            Track = MapToTrackItem(dto.Track ?? throw new InvalidOperationException("Track is null"))
        };
    }

    private SpotifyArtist MapToArtist(SpotifyArtistDto dto)
    {
        return new SpotifyArtist
        {
            Id = dto.Id ?? string.Empty,
            Name = dto.Name ?? string.Empty
        };
    }

    private SpotifyTrackResponse MapToTrackResponse(SpotifyTrackDto dto)
    {
        return new SpotifyTrackResponse
        {
            Id = dto.Id ?? string.Empty,
            Name = dto.Name ?? string.Empty,
            Artists = dto.Artists?.Select(a => new SpotifyArtist { Id = a.Id ?? string.Empty, Name = a.Name ?? string.Empty }).ToList() ?? new List<SpotifyArtist>(),
            Album = dto.Album != null ? MapToAlbum(dto.Album) : null,
            DurationMs = dto.DurationMs,
            PreviewUrl = dto.PreviewUrl,
            ExternalUrl = dto.ExternalUrls?.Spotify,
            Genres = new List<string>() // Track DTO'da genre yok, album'den alınabilir
        };
    }

    #endregion
}

