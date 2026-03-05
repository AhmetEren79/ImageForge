// AiServiceRequest.cs — Python FastAPI servisine gönderilecek request payload.

using System.Text.Json.Serialization;

namespace ImageForge.Application.DTOs.Generation;

public class AiServiceRequest
{
    [JsonPropertyName("prompt_id")]
    public string PromptId { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("negative_prompt")]
    public string? NegativePrompt { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("image_count")]
    public int ImageCount { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("steps")]
    public int Steps { get; set; }

    [JsonPropertyName("cfg_scale")]
    public double CfgScale { get; set; }

    [JsonPropertyName("seed")]
    public long? Seed { get; set; }

    [JsonPropertyName("callback_url")]
    public string CallbackUrl { get; set; } = string.Empty;
}
