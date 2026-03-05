// GalleryImageResponse.cs — Galeri listesi öğesi DTO'su.
// Sayfalanmış listede her görsel için döndürülen bilgiler.

namespace ImageForge.Application.DTOs.Gallery;

public class GalleryImageResponse
{
    public Guid Id { get; set; }
    public string StorageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }

    // Prompt bilgileri
    public string PromptText { get; set; } = string.Empty;
    public string SelectedModel { get; set; } = string.Empty;
}
