// AuthResponse.cs — Kimlik doğrulama yanıt DTO'su.
// Register ve Login işlemlerinin ortak dönüş tipi.

namespace ImageForge.Application.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
