// GenerationStatus.cs — Görsel üretim isteğinin mevcut durumunu tanımlar.

namespace ImageForge.Domain.Enums;

public enum GenerationStatus
{
    /// <summary>İstek oluşturuldu, henüz işlenmedi.</summary>
    Pending = 0,

    /// <summary>AI servisi tarafından işleniyor.</summary>
    Processing = 1,

    /// <summary>Görseller başarıyla üretildi.</summary>
    Completed = 2,

    /// <summary>Üretim sırasında hata oluştu.</summary>
    Failed = 3
}
