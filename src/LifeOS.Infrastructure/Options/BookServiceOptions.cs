namespace LifeOS.Infrastructure.Options;

/// <summary>
/// Kitap servisi (Google Books API ve Open Library API) yapılandırma seçenekleri.
/// </summary>
public sealed class BookServiceOptions
{
    public const string SectionName = "BookServiceOptions";

    /// <summary>
    /// Google Books API endpoint URL'i
    /// </summary>
    public string GoogleBooksApiEndpoint { get; set; } = "https://www.googleapis.com/books/v1";

    /// <summary>
    /// Google Books API key (opsiyonel - rate limit için önerilir)
    /// </summary>
    public string? GoogleBooksApiKey { get; set; }

    /// <summary>
    /// Open Library API endpoint URL'i
    /// </summary>
    public string OpenLibraryApiEndpoint { get; set; } = "https://openlibrary.org";

    /// <summary>
    /// HTTP isteği için timeout süresi (saniye)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Başarısız istekler için retry sayısı
    /// </summary>
    public int RetryCount { get; set; } = 2;

    /// <summary>
    /// Retry'ler arası bekleme süresi (saniye)
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 1;

    /// <summary>
    /// Öncelikli API kaynağı (GoogleBooks veya OpenLibrary)
    /// </summary>
    public string PrimaryApiSource { get; set; } = "GoogleBooks";
}

