namespace LifeOS.Application.Abstractions;

/// <summary>
/// Spotify token'larını şifrelemek/şifresini çözmek için servis interface'i
/// </summary>
public interface ISpotifyTokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
}

