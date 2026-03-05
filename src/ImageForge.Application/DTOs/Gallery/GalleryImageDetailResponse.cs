// GalleryImageDetailResponse.cs — Tekil görsel detay DTO'su.
// Liste DTO'sunun üstüne ek metadata ve prompt parametreleri içerir.

namespace ImageForge.Application.DTOs.Gallery;

public class GalleryImageDetailResponse
{
    public Guid Id { get; set; }
    public string StorageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long? Seed { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPublic { get; set; }
    public string? PublicShareToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Prompt bilgileri
    public Guid PromptId { get; set; }
    public string PromptText { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public string SelectedModel { get; set; } = string.Empty;
    public int Steps { get; set; }
    public double CfgScale { get; set; }
}
