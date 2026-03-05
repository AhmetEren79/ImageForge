// CreateGenerationRequest.cs — Görsel üretim isteği DTO'su.

using ImageForge.Domain.Enums;

namespace ImageForge.Application.DTOs.Generation;

public class CreateGenerationRequest
{
    public string PromptText { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public AiModelType SelectedModel { get; set; } = AiModelType.DiscoElysium;
    public int ImageCount { get; set; } = 2;
    public int Width { get; set; } = 1024;
    public int Height { get; set; } = 1024;
    public int Steps { get; set; } = 30;
    public double CfgScale { get; set; } = 7.0;
    public long? Seed { get; set; }
}
