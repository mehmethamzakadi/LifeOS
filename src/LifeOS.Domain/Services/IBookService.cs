namespace LifeOS.Domain.Services;

/// <summary>
/// ISBN numarası ile kitap bilgilerini çekmek için servis interface'i.
/// </summary>
public interface IBookService
{
    /// <summary>
    /// ISBN numarası ile kitap bilgilerini çeker.
    /// </summary>
    /// <param name="isbn">ISBN numarası (10 veya 13 haneli)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kitap bilgileri (başlık, yazar, yayınevi, yayın tarihi, sayfa sayısı, kapak resmi vb.)</returns>
    Task<BookInfoDto> GetBookByIsbnAsync(string isbn, CancellationToken cancellationToken = default);
}

/// <summary>
/// ISBN servisinden dönen kitap bilgileri DTO'su.
/// </summary>
public sealed record BookInfoDto
{
    public string Title { get; init; } = default!;
    public string Author { get; init; } = default!;
    public string? Publisher { get; init; }
    public DateTime? PublishedDate { get; init; }
    public int? PageCount { get; init; }
    public string? CoverUrl { get; init; }
    public string? Description { get; init; }
    public string? Isbn { get; init; }
    public string? Language { get; init; }
    public List<string> Categories { get; init; } = new();
}

