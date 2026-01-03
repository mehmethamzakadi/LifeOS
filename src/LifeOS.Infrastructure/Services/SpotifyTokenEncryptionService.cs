using LifeOS.Application.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace LifeOS.Infrastructure.Services;

/// <summary>
/// Spotify token'larını şifrelemek/şifresini çözmek için servis
/// Data Protection API kullanır
/// </summary>
public sealed class SpotifyTokenEncryptionService : ISpotifyTokenEncryptionService
{
    private readonly IDataProtector _protector;
    private readonly ILogger<SpotifyTokenEncryptionService> _logger;

    public SpotifyTokenEncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<SpotifyTokenEncryptionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector("LifeOS.Spotify.Tokens");
        _logger = logger;
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        try
        {
            return _protector.Protect(plainText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token şifreleme hatası");
            throw;
        }
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
        {
            return string.Empty;
        }

        try
        {
            return _protector.Unprotect(encryptedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token şifre çözme hatası");
            throw;
        }
    }
}

