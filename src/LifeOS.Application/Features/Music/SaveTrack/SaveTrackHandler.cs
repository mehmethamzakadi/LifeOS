using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.SaveTrack;

public sealed class SaveTrackHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    public SaveTrackHandler(
        LifeOSDbContext context,
        ISpotifyApiService spotifyApiService,
        ISpotifyTokenEncryptionService tokenEncryptionService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _spotifyApiService = spotifyApiService;
        _tokenEncryptionService = tokenEncryptionService;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<SaveTrackResponse>> HandleAsync(
        SaveTrackCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<SaveTrackResponse>("Yetkisiz erişim");
        }

        // Mevcut kaydı kontrol et
        var existingTrack = await _context.SavedTracks
            .FirstOrDefaultAsync(t => t.UserId == userId.Value && t.SpotifyTrackId == command.SpotifyTrackId && !t.IsDeleted, cancellationToken);

        if (existingTrack != null)
        {
            return ApiResultExtensions.Failure<SaveTrackResponse>("Bu şarkı zaten kaydedilmiş");
        }

        // Spotify'dan track bilgilerini al
        var connection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted && c.IsActive, cancellationToken);

        if (connection == null)
        {
            return ApiResultExtensions.Failure<SaveTrackResponse>("Spotify bağlantısı bulunamadı");
        }

        var accessToken = _tokenEncryptionService.Decrypt(connection.AccessToken);

        // Token süresi dolmuşsa yenile
        if (connection.ExpiresAt <= DateTime.UtcNow)
        {
            try
            {
                var refreshToken = _tokenEncryptionService.Decrypt(connection.RefreshToken);
                var tokenResponse = await _spotifyApiService.RefreshTokenAsync(refreshToken, cancellationToken);
                
                var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                connection.UpdateTokens(
                    _tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                    _tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                    expiresAt);
                
                _context.MusicConnections.Update(connection);
                await _context.SaveChangesAsync(cancellationToken);
                
                accessToken = tokenResponse.AccessToken;
            }
            catch
            {
                return ApiResultExtensions.Failure<SaveTrackResponse>("Token yenilenemedi");
            }
        }

        try
        {
            var trackInfo = await _spotifyApiService.GetTrackAsync(accessToken, command.SpotifyTrackId, cancellationToken);

            var savedTrack = SavedTrack.Create(
                userId.Value,
                trackInfo.Id,
                trackInfo.Name,
                trackInfo.Artists.FirstOrDefault()?.Name ?? "Bilinmeyen Sanatçı",
                trackInfo.Album?.Name,
                trackInfo.Album?.Images.FirstOrDefault()?.Url,
                trackInfo.DurationMs,
                command.Notes
            );

            await _context.SavedTracks.AddAsync(savedTrack, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new SaveTrackResponse(
                savedTrack.Id,
                savedTrack.UserId,
                savedTrack.SpotifyTrackId,
                savedTrack.Name,
                savedTrack.Artist,
                savedTrack.Album,
                savedTrack.AlbumCoverUrl,
                savedTrack.DurationMs,
                savedTrack.SavedAt,
                savedTrack.Notes,
                savedTrack.CreatedDate
            );

            return ApiResultExtensions.Success(response, "Şarkı başarıyla kaydedildi");
        }
        catch (Exception ex)
        {
            return ApiResultExtensions.Failure<SaveTrackResponse>($"Şarkı bilgisi alınamadı: {ex.Message}");
        }
    }
}

