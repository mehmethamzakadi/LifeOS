namespace LifeOS.Infrastructure.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "OllamaOptions";

    public string Endpoint { get; set; } = "http://localhost:11434";
    public string ModelId { get; set; } = "qwen2.5:7b";
    public string ApiKey { get; set; } = "ollama";

    /// <summary>
    /// HTTP isteği için timeout süresi (dakika)
    /// </summary>
    public int TimeoutMinutes { get; set; } = 2;

    /// <summary>
    /// Başarısız istekler için retry sayısı
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Retry'ler arası bekleme süresi (saniye)
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
}
