using System.Text.Json.Serialization;

namespace LifeOS.Infrastructure.Models.GoogleBooks;

/// <summary>
/// Google Books API response model
/// </summary>
public sealed class GoogleBooksResponse
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }

    [JsonPropertyName("items")]
    public List<GoogleBookItem>? Items { get; set; }
}

public sealed class GoogleBookItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("volumeInfo")]
    public GoogleBookVolumeInfo? VolumeInfo { get; set; }
}

public sealed class GoogleBookVolumeInfo
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("authors")]
    public List<string>? Authors { get; set; }

    [JsonPropertyName("publisher")]
    public string? Publisher { get; set; }

    [JsonPropertyName("publishedDate")]
    public string? PublishedDate { get; set; }

    [JsonPropertyName("pageCount")]
    public int? PageCount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("industryIdentifiers")]
    public List<GoogleBookIndustryIdentifier>? IndustryIdentifiers { get; set; }

    [JsonPropertyName("imageLinks")]
    public GoogleBookImageLinks? ImageLinks { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }
}

public sealed class GoogleBookIndustryIdentifier
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("identifier")]
    public string? Identifier { get; set; }
}

public sealed class GoogleBookImageLinks
{
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    [JsonPropertyName("smallThumbnail")]
    public string? SmallThumbnail { get; set; }

    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    [JsonPropertyName("large")]
    public string? Large { get; set; }
}

