// AiCallbackPayload.cs — Python AI servisinden webhook ile gelen yanıt.

using System.Text.Json.Serialization;

namespace ImageForge.Application.DTOs.Generation;

public class AiCallbackPayload
{
    [JsonPropertyName("prompt_id")]
    public string PromptId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // "completed" | "failed"

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("images")]
    public List<AiCallbackImage> Images { get; set; } = new();
}

public class AiCallbackImage
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("storage_key")]
    public string StorageKey { get; set; } = string.Empty;

    [JsonPropertyName("file_name")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("seed")]
    public long? Seed { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("file_size")]
    public long FileSize { get; set; }
}
