// GeneratedImage.cs — Üretilen görsel entity'si.
// R2 depolama bilgilerini, favori/paylaşım durumunu ve görsel metadata'sını tutar.

using ImageForge.Domain.Common;

namespace ImageForge.Domain.Entities;

public class GeneratedImage : BaseEntity
{
    /// <summary>Cloudflare R2 public URL (max 1024).</summary>
    public string StorageUrl { get; set; } = string.Empty;

    /// <summary>R2 object key — silme işlemi için (max 512).</summary>
    public string StorageKey { get; set; } = string.Empty;

    /// <summary>Dosya adı (max 256).</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Dosya boyutu (byte).</summary>
    public long FileSizeBytes { get; set; }

    /// <summary>Görsel genişliği (px).</summary>
    public int Width { get; set; }

    /// <summary>Görsel yüksekliği (px).</summary>
    public int Height { get; set; }

    /// <summary>Bu görselin üretildiği spesifik seed değeri.</summary>
    public long? Seed { get; set; }

    /// <summary>Favori olarak işaretlenmiş mi?</summary>
    public bool IsFavorite { get; set; }

    /// <summary>Public paylaşım linki aktif mi?</summary>
    public bool IsPublic { get; set; }

    /// <summary>Benzersiz paylaşım token'ı (max 64, unique filtered index).</summary>
    public string? PublicShareToken { get; set; }

    // Foreign key
    public Guid PromptId { get; set; }

    // Navigation property
    public Prompt Prompt { get; set; } = null!;
}
