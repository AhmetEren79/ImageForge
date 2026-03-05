// LoginRequest.cs — Giriş isteği DTO'su.
// EmailOrUsername alanı hem e-posta hem kullanıcı adı ile giriş yapılmasına izin verir.

namespace ImageForge.Application.DTOs.Auth;

public class LoginRequest
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
