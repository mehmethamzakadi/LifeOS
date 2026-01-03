using LifeOS.Domain.Services;
using LifeOS.Infrastructure.Models.GoogleBooks;
using LifeOS.Infrastructure.Models.OpenLibrary;
using LifeOS.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace LifeOS.Infrastructure.Services;

/// <summary>
/// ISBN numarası ile kitap bilgilerini çeken servis.
/// Google Books API ve Open Library API kullanır.
/// Best practices: IHttpClientFactory, retry policy, logging, proper error handling.
/// </summary>
public sealed class BookService : IBookService
{
    private const string GoogleBooksClientName = "GoogleBooksClient";
    private const string OpenLibraryClientName = "OpenLibraryClient";

    private readonly IHttpClientFactory httpClientFactory;
    private readonly BookServiceOptions options;
    private readonly ILogger<BookService> logger;

    public BookService(
        IHttpClientFactory httpClientFactory,
        IOptions<BookServiceOptions> options,
        ILogger<BookService> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
        this.logger = logger;
    }

    public async Task<BookInfoDto> GetBookByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            throw new ArgumentException("ISBN numarası boş olamaz.", nameof(isbn));
        }

        // ISBN'i temizle (sadece rakamlar ve X karakteri)
        var cleanIsbn = CleanIsbn(isbn);
        if (string.IsNullOrWhiteSpace(cleanIsbn))
        {
            throw new ArgumentException("Geçersiz ISBN formatı.", nameof(isbn));
        }

        logger.LogInformation("ISBN ile kitap bilgisi çekiliyor: {Isbn}", cleanIsbn);

        // ISBN-13 ise ISBN-10'a da çevirip deneyebiliriz (bazı API'ler sadece ISBN-10 destekler)
        var isbnVariants = new List<string> { cleanIsbn };
        if (cleanIsbn.Length == 13)
        {
            // ISBN-13'ten ISBN-10'a çevir (ilk 3 haneyi ve son check digit'i çıkar)
            var isbn10 = cleanIsbn.Substring(3, 9); // 978'den sonraki 9 hane
            // Check digit hesapla (ISBN-10 için)
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(isbn10[i].ToString()) * (10 - i);
            }
            int checkDigit = (11 - (sum % 11)) % 11;
            var checkChar = checkDigit == 10 ? "X" : checkDigit.ToString();
            isbnVariants.Add(isbn10 + checkChar);
            logger.LogDebug("ISBN-10 variant oluşturuldu: {Isbn10}", isbn10 + checkChar);
        }

        // Öncelikli API kaynağına göre sırayla dene
        if (string.Equals(options.PrimaryApiSource, "GoogleBooks", StringComparison.OrdinalIgnoreCase))
        {
            // Önce Google Books'u tüm ISBN variant'ları ile dene
            foreach (var isbnVariant in isbnVariants)
            {
                try
                {
                    var result = await TryGetFromGoogleBooksAsync(isbnVariant, cancellationToken);
                    if (result != null)
                    {
                        logger.LogInformation("Google Books API'den kitap bilgisi alındı: {Isbn}", isbnVariant);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Google Books API'den kitap bilgisi alınamadı (ISBN: {Isbn}): {Message}", 
                        isbnVariant, ex.Message);
                }
            }

            // Fallback: Open Library - tüm ISBN variant'ları ile dene
            foreach (var isbnVariant in isbnVariants)
            {
                try
                {
                    var result = await TryGetFromOpenLibraryAsync(isbnVariant, cancellationToken);
                    if (result != null)
                    {
                        logger.LogInformation("Open Library API'den kitap bilgisi alındı: {Isbn}", isbnVariant);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Open Library API'den kitap bilgisi alınamadı (ISBN: {Isbn}): {Message}", 
                        isbnVariant, ex.Message);
                }
            }

            throw new HttpRequestException($"ISBN '{cleanIsbn}' için kitap bilgisi bulunamadı. Google Books ve Open Library API'lerinde sonuç bulunamadı.");
        }
        else
        {
            // Önce Open Library'yi tüm ISBN variant'ları ile dene
            foreach (var isbnVariant in isbnVariants)
            {
                try
                {
                    var result = await TryGetFromOpenLibraryAsync(isbnVariant, cancellationToken);
                    if (result != null)
                    {
                        logger.LogInformation("Open Library API'den kitap bilgisi alındı: {Isbn}", isbnVariant);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Open Library API'den kitap bilgisi alınamadı (ISBN: {Isbn}): {Message}", 
                        isbnVariant, ex.Message);
                }
            }

            // Fallback: Google Books - tüm ISBN variant'ları ile dene
            foreach (var isbnVariant in isbnVariants)
            {
                try
                {
                    var result = await TryGetFromGoogleBooksAsync(isbnVariant, cancellationToken);
                    if (result != null)
                    {
                        logger.LogInformation("Google Books API'den kitap bilgisi alındı: {Isbn}", isbnVariant);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Google Books API'den kitap bilgisi alınamadı (ISBN: {ISBN}): {Message}", 
                        isbnVariant, ex.Message);
                }
            }

            throw new HttpRequestException($"ISBN '{cleanIsbn}' için kitap bilgisi bulunamadı. Open Library ve Google Books API'lerinde sonuç bulunamadı.");
        }
    }

    private async Task<BookInfoDto?> TryGetFromGoogleBooksAsync(string isbn, CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(GoogleBooksClientName);
            
            // Google Books API: ISBN ile arama
            // Dokümantasyon: https://developers.google.com/books/docs/v1/using
            // API key formatı: key=YOUR_API_KEY (query parameter olarak, kodlama gerekmez)
            var queryParams = new List<string> { $"q=isbn:{isbn}" };
            
            if (!string.IsNullOrWhiteSpace(options.GoogleBooksApiKey))
            {
                // Dokümantasyona göre: "API anahtarı, URL'lere yerleştirmek için güvenlidir; herhangi bir kodlama yapmanız gerekmez."
                queryParams.Add($"key={options.GoogleBooksApiKey}");
            }
            
            // ÖNEMLİ: Path başında '/' OLMAMALI (BaseAddress sonunda '/' var)
            var url = $"volumes?{string.Join("&", queryParams)}";
            var fullUrl = $"{httpClient.BaseAddress}{url}";
            logger.LogDebug("Google Books API isteği: {FullUrl}", fullUrl);
            var response = await httpClient.GetAsync(url, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("Google Books API response - StatusCode: {StatusCode}, ContentLength: {Length}, Content: {Response}", 
                response.StatusCode, responseContent.Length, responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Google Books API hatası: StatusCode={StatusCode}, Response={Response}, FullURL={FullUrl}",
                    response.StatusCode,
                    responseContent,
                    fullUrl);
                
                // Google Books API genellikle 404 dönmez, 200 döner ama totalItems: 0 olur
                // Eğer gerçekten 404 dönüyorsa, bu bir yapılandırma hatası olabilir
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogWarning("Google Books API'de endpoint bulunamadı (404). FullURL kontrol edilmeli: {FullUrl}", fullUrl);
                    return null;
                }

                // 403 veya 401 ise API key sorunu olabilir
                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    logger.LogWarning("Google Books API key sorunu olabilir. StatusCode: {StatusCode}, Response: {Response}", 
                        response.StatusCode, responseContent);
                }

                throw new HttpRequestException(
                    $"Google Books API hatası: {response.StatusCode} - {responseContent}",
                    null,
                    response.StatusCode);
            }

            // Response'u tekrar okumak yerine, string'den parse edelim
            GoogleBooksResponse? googleResponse;
            try
            {
                googleResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleBooksResponse>(responseContent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Google Books API response parse edilemedi. Raw response: {Response}", responseContent);
                return null;
            }

            if (googleResponse == null)
            {
                logger.LogWarning("Google Books API response parse edilemedi. Raw response: {Response}", responseContent);
                return null;
            }

            if (googleResponse.Items == null || googleResponse.Items.Count == 0)
            {
                logger.LogInformation("Google Books API'de ISBN için sonuç bulunamadı: {Isbn}, TotalItems: {TotalItems}", 
                    isbn, googleResponse.TotalItems);
                return null;
            }

            var book = googleResponse.Items[0].VolumeInfo;
            if (book == null)
            {
                return null;
            }

            // Yayın tarihini parse et
            DateTime? publishedDate = null;
            if (!string.IsNullOrWhiteSpace(book.PublishedDate))
            {
                // Yıl formatı (2020) veya tam tarih (2020-01-15)
                if (DateTime.TryParse(book.PublishedDate, out var parsedDate))
                {
                    publishedDate = parsedDate;
                }
                else if (int.TryParse(book.PublishedDate, out var year) && year > 0 && year <= DateTime.Now.Year)
                {
                    publishedDate = new DateTime(year, 1, 1);
                }
            }

            // Kapak resmi URL'ini al (en büyük boyut)
            var coverUrl = book.ImageLinks?.Large 
                ?? book.ImageLinks?.Medium 
                ?? book.ImageLinks?.Thumbnail;

            // Yazar listesini birleştir
            var author = book.Authors != null && book.Authors.Count > 0
                ? string.Join(", ", book.Authors)
                : "Bilinmeyen Yazar";

            return new BookInfoDto
            {
                Title = book.Title ?? "Bilinmeyen Başlık",
                Author = author,
                Publisher = book.Publisher,
                PublishedDate = publishedDate,
                PageCount = book.PageCount,
                CoverUrl = coverUrl,
                Description = book.Description,
                Isbn = isbn,
                Language = book.Language,
                Categories = book.Categories ?? new List<string>()
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Google Books API timeout: {Isbn}", isbn);
            throw new TimeoutException("Google Books API'ye istek zaman aşımına uğradı.", ex);
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google Books API'den kitap bilgisi çekilirken beklenmeyen hata: {Isbn}", isbn);
            throw;
        }
    }

    private async Task<BookInfoDto?> TryGetFromOpenLibraryAsync(string isbn, CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient(OpenLibraryClientName);
            
            // Open Library API: ISBN ile arama
            // Format: api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data
            // ÖNEMLİ: Path başında '/' OLMAMALI (BaseAddress sonunda '/' var)
            var url = $"api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogInformation("Open Library API'de ISBN bulunamadı: {Isbn}", isbn);
                    return null;
                }

                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogError(
                    "Open Library API hatası: StatusCode={StatusCode}, Response={Response}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"Open Library API hatası: {response.StatusCode}",
                    null,
                    response.StatusCode);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("Open Library API response: {Response}", responseContent);

            var openLibraryResponse = await response.Content.ReadFromJsonAsync<OpenLibraryResponse>(cancellationToken: cancellationToken);

            if (openLibraryResponse == null || openLibraryResponse.Count == 0)
            {
                logger.LogInformation("Open Library API'de ISBN için sonuç bulunamadı: {Isbn}, Response: {Response}", isbn, responseContent);
                return null;
            }

            // Key formatı: "ISBN:9786054054206" veya sadece ISBN numarası olabilir
            var bookKey = openLibraryResponse.Keys.FirstOrDefault(k => 
                k.Contains(isbn, StringComparison.OrdinalIgnoreCase));
            
            if (bookKey == null)
            {
                logger.LogWarning("Open Library API'de ISBN key bulunamadı: {Isbn}, Available keys: {Keys}", 
                    isbn, string.Join(", ", openLibraryResponse.Keys));
                return null;
            }

            var book = openLibraryResponse[bookKey];
            if (book == null)
            {
                return null;
            }

            // Yayın tarihini parse et
            DateTime? publishedDate = null;
            if (!string.IsNullOrWhiteSpace(book.PublishDate))
            {
                // Yıl formatı (2020) veya tam tarih
                if (DateTime.TryParse(book.PublishDate, out var parsedDate))
                {
                    publishedDate = parsedDate;
                }
                else if (int.TryParse(book.PublishDate, out var year) && year > 0 && year <= DateTime.Now.Year)
                {
                    publishedDate = new DateTime(year, 1, 1);
                }
            }

            // Kapak resmi URL'ini oluştur (öncelik: cover.large -> cover.medium -> cover.small -> covers listesi)
            string? coverUrl = null;
            if (book.Cover != null)
            {
                // cover objesi varsa onu kullan (en yüksek kalite)
                coverUrl = book.Cover.Large 
                    ?? book.Cover.Medium 
                    ?? book.Cover.Small;
            }
            else if (book.Covers != null && book.Covers.Count > 0)
            {
                // cover objesi yoksa covers listesinden large URL oluştur
                var coverId = book.Covers[0];
                coverUrl = $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg"; // Large size
            }

            // Yazar listesini birleştir
            var author = book.Authors != null && book.Authors.Count > 0
                ? string.Join(", ", book.Authors.Select(a => a.Name).Where(n => !string.IsNullOrWhiteSpace(n)))
                : "Bilinmeyen Yazar";

            // Yayınevi
            var publisher = book.Publishers != null && book.Publishers.Count > 0
                ? string.Join(", ", book.Publishers.Select(p => p.Name).Where(n => !string.IsNullOrWhiteSpace(n)))
                : null;

            // Dil
            var language = book.Languages != null && book.Languages.Count > 0
                ? book.Languages[0].Key?.Replace("/languages/", "").ToUpper()
                : null;

            // Kategoriler
            var categories = book.Subjects != null
                ? book.Subjects.Select(s => s.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Cast<string>().ToList()
                : new List<string>();

            // Açıklama
            var description = book.Description?.Value ?? book.Description?.Type;

            return new BookInfoDto
            {
                Title = book.Title ?? "Bilinmeyen Başlık",
                Author = author,
                Publisher = publisher,
                PublishedDate = publishedDate,
                PageCount = book.NumberOfPages,
                CoverUrl = coverUrl,
                Description = description,
                Isbn = isbn,
                Language = language,
                Categories = categories
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError(ex, "Open Library API timeout: {Isbn}", isbn);
            throw new TimeoutException("Open Library API'ye istek zaman aşımına uğradı.", ex);
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Open Library API'den kitap bilgisi çekilirken beklenmeyen hata: {Isbn}", isbn);
            throw;
        }
    }

    /// <summary>
    /// ISBN'i temizler (sadece rakamlar ve X karakteri kalır)
    /// </summary>
    private static string CleanIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return string.Empty;
        }

        // Sadece rakamlar ve X karakteri kalır
        var cleaned = Regex.Replace(isbn, @"[^\dX]", "", RegexOptions.IgnoreCase);
        
        // X karakterini büyük harfe çevir
        cleaned = cleaned.ToUpperInvariant();

        return cleaned;
    }
}

