using LifeOS.Domain.Entities;
namespace LifeOS.Domain.Services;

/// <summary>
/// Yapay zeka destekli içerik üretme servisi için interface.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Verilen kategori adı için SEO uyumlu, kısa bir açıklama üretir.
    /// </summary>
    /// <param name="categoryName">Kategori adı</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Üretilen kategori açıklaması</returns>
    Task<string> GenerateCategoryDescriptionAsync(string categoryName, CancellationToken cancellationToken = default);
}
