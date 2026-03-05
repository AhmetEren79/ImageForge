// BaseEntity.cs — Tüm entity'ler için ortak temel sınıf.
// Id, CreatedAt ve UpdatedAt alanlarını merkezi olarak yönetir.

namespace ImageForge.Domain.Common;

public abstract class BaseEntity
{
    /// <summary>Benzersiz tanımlayıcı (Primary Key).</summary>
    public Guid Id { get; set; }

    /// <summary>Oluşturulma tarihi (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Son güncelleme tarihi (UTC, nullable).</summary>
    public DateTime? UpdatedAt { get; set; }
}
