// User.cs — Kullanıcı entity'si.
// Email ve Username benzersizdir (unique index). Şifre BCrypt ile hashlenmiş saklanır.

using ImageForge.Domain.Common;

namespace ImageForge.Domain.Entities;

public class User : BaseEntity
{
    /// <summary>Kullanıcı e-posta adresi (unique, max 256).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Kullanıcı adı (unique, max 50).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>BCrypt ile hashlenmiş şifre (max 512).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Görünen ad (opsiyonel, max 100).</summary>
    public string? DisplayName { get; set; }

    /// <summary>Hesap aktif mi?</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Son giriş tarihi (UTC).</summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation property
    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
