using System.Security.Cryptography;
using System.Text;

namespace LifeOS.Domain.Common.Utilities;

/// <summary>
/// Guid utility metodları
/// Deterministic Guid oluşturma için kullanılır
/// </summary>
public static class GuidHelper
{
    /// <summary>
    /// String'den deterministic Guid oluşturur (SHA256 hash kullanarak)
    /// Aynı string her zaman aynı Guid'i üretir
    /// 
    /// Kullanım: MessageId yoksa fallback ID oluşturmak için
    /// Örnek: GenerateDeterministicGuid($"{entityId}_{timestamp:O}_{type}")
    /// 
    /// NOT: MD5 yerine SHA256 kullanılıyor (güvenlik için)
    /// </summary>
    /// <param name="input">Hash'lenecek string</param>
    /// <returns>Deterministic Guid (SHA256 hash'in ilk 16 byte'ı kullanılarak)</returns>
    public static Guid GenerateDeterministicGuid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        // SHA256 32 byte, Guid 16 byte - ilk 16 byte'ı kullan
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }
}
