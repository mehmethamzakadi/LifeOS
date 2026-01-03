using System.Text.Json.Serialization;

namespace LifeOS.Infrastructure.Models.OpenLibrary;

/// <summary>
/// Open Library API response model
/// Response format: { "ISBN:9786054054206": { ... }, "ISBN:9786054054207": { ... } }
/// Key dinamik olduğu için Dictionary kullanıyoruz
/// </summary>
public sealed class OpenLibraryResponse : Dictionary<string, OpenLibraryBook>
{
}

public sealed class OpenLibraryBook
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("authors")]
    public List<OpenLibraryAuthor>? Authors { get; set; }

    [JsonPropertyName("publishers")]
    public List<OpenLibraryPublisher>? Publishers { get; set; }

    [JsonPropertyName("publish_date")]
    public string? PublishDate { get; set; }

    [JsonPropertyName("number_of_pages")]
    public int? NumberOfPages { get; set; }

    [JsonPropertyName("description")]
    public OpenLibraryDescription? Description { get; set; }

    [JsonPropertyName("covers")]
    public List<long>? Covers { get; set; }

    [JsonPropertyName("cover")]
    public OpenLibraryCover? Cover { get; set; }

    [JsonPropertyName("languages")]
    public List<OpenLibraryLanguage>? Languages { get; set; }

    [JsonPropertyName("subjects")]
    public List<OpenLibrarySubject>? Subjects { get; set; }
}

public sealed class OpenLibraryAuthor
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class OpenLibraryPublisher
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class OpenLibraryDescription
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

public sealed class OpenLibraryLanguage
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}

public sealed class OpenLibrarySubject
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public sealed class OpenLibraryCover
{
    [JsonPropertyName("small")]
    public string? Small { get; set; }

    [JsonPropertyName("medium")]
    public string? Medium { get; set; }

    [JsonPropertyName("large")]
    public string? Large { get; set; }
}

