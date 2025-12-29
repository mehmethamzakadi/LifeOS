using System.Text.Json.Serialization;

namespace LifeOS.Infrastructure.Models.Ollama;

/// <summary>
/// Ollama API chat mesaj modeli.
/// </summary>
public sealed class OllamaMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
