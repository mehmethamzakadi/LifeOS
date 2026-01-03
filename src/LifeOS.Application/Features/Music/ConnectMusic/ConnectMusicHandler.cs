using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.ConnectMusic;

public sealed class ConnectMusicHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ISpotifyApiService _spotifyApiService;
    private readonly ISpotifyTokenEncryptionService _tokenEncryptionService;
    private readonly ICurrentUserService _currentUserService;

    public ConnectMusicHandler(
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

    public async Task<ApiResult<ConnectMusicResponse>> HandleAsync(
        ConnectMusicCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<ConnectMusicResponse>("Yetkisiz erişim");
        }

        // Authorization code ile token al
        var tokenResponse = await _spotifyApiService.ExchangeCodeForTokenAsync(command.Code, cancellationToken);

        // Kullanıcı profil bilgilerini al
        var userProfile = await _spotifyApiService.GetUserProfileAsync(tokenResponse.AccessToken, cancellationToken);

        // Mevcut bağlantıyı kontrol et
        var existingConnection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted, cancellationToken);

        var expiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

        if (existingConnection != null)
        {
            // Mevcut bağlantıyı güncelle
            existingConnection.UpdateTokens(
                _tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                _tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                expiresAt);
            existingConnection.Activate();
            _context.MusicConnections.Update(existingConnection);
        }
        else
        {
            // Yeni bağlantı oluştur
            var connection = MusicConnection.Create(
                userId.Value,
                _tokenEncryptionService.Encrypt(tokenResponse.AccessToken),
                _tokenEncryptionService.Encrypt(tokenResponse.RefreshToken),
                expiresAt,
                userProfile.Id,
                userProfile.DisplayName,
                userProfile.Email);

            await _context.MusicConnections.AddAsync(connection, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(
            new ConnectMusicResponse(true, "Spotify hesabı başarıyla bağlandı"),
            "Spotify hesabı başarıyla bağlandı");
    }
}

