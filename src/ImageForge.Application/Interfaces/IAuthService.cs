// IAuthService.cs — Kimlik doğrulama servis interface'i.
// Register ve Login işlemlerini tanımlar.

using ImageForge.Application.DTOs.Auth;

namespace ImageForge.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
