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
    /// String'den deterministic Guid oluşturur (MD5 hash kullanarak)
    /// Aynı string her zaman aynı Guid'i üretir
    /// 
    /// Kullanım: MessageId yoksa fallback ID oluşturmak için
    /// Örnek: GenerateDeterministicGuid($"{entityId}_{timestamp:O}_{type}")
    /// </summary>
    /// <param name="input">Hash'lenecek string</param>
    /// <returns>Deterministic Guid</returns>
    public static Guid GenerateDeterministicGuid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
