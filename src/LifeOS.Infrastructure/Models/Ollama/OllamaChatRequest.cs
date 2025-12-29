using System.Text.Json.Serialization;

namespace LifeOS.Infrastructure.Models.Ollama;

/// <summary>
/// Ollama API chat completion istek modeli.
/// </summary>
public sealed class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public OllamaMessage[] Messages { get; set; } = Array.Empty<OllamaMessage>();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }
}
