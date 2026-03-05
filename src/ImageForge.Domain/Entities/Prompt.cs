// Prompt.cs — AI görsel üretim isteği entity'si.
// Kullanıcının girdiği prompt bilgilerini, model seçimini ve üretim parametrelerini tutar.

using ImageForge.Domain.Common;
using ImageForge.Domain.Enums;

namespace ImageForge.Domain.Entities;

public class Prompt : BaseEntity
{
    /// <summary>Kullanıcının girdiği prompt metni (max 2000).</summary>
    public string PromptText { get; set; } = string.Empty;

    /// <summary>Negatif prompt (opsiyonel, max 2000).</summary>
    public string? NegativePrompt { get; set; }

    /// <summary>Seçilen AI modeli (LoRA).</summary>
    public AiModelType SelectedModel { get; set; }

    /// <summary>Üretim durumu.</summary>
    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;

    /// <summary>Üretilecek görsel sayısı (default 2, max 3).</summary>
    public int ImageCount { get; set; } = 2;

    /// <summary>Görsel genişliği (px).</summary>
    public int Width { get; set; } = 1024;

    /// <summary>Görsel yüksekliği (px).</summary>
    public int Height { get; set; } = 1024;

    /// <summary>Diffusion adım sayısı.</summary>
    public int Steps { get; set; } = 30;

    /// <summary>Classifier-free guidance scale.</summary>
    public double CfgScale { get; set; } = 7.0;

    /// <summary>Seed değeri (opsiyonel, tekrarlanabilirlik için).</summary>
    public long? Seed { get; set; }

    /// <summary>Hata mesajı (üretim başarısız olursa, max 1000).</summary>
    public string? ErrorMessage { get; set; }

    // Foreign key
    public Guid UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<GeneratedImage> GeneratedImages { get; set; } = new List<GeneratedImage>();
}
