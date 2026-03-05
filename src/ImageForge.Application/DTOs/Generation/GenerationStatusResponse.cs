// GenerationStatusResponse.cs — Üretim durumu yanıt DTO'su.
// Polling endpoint'inin ve history'nin dönüş tipi.

using ImageForge.Domain.Enums;

namespace ImageForge.Application.DTOs.Generation;

public class GenerationStatusResponse
{
    public Guid PromptId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PromptText { get; set; } = string.Empty;
    public string SelectedModel { get; set; } = string.Empty;
    public int ImageCount { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GeneratedImageDto> Images { get; set; } = new();
}

public class GeneratedImageDto
{
    public Guid Id { get; set; }
    public string StorageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long? Seed { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
}
