// AuthService.cs — Kayıt ve giriş işlemlerini yöneten servis.
// BCrypt ile şifre hash/verify, email/username uniqueness kontrolü, JWT token üretimi.

using ImageForge.Application.DTOs.Auth;
using ImageForge.Application.Interfaces;
using ImageForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageForge.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtService _jwtService;

    public AuthService(IApplicationDbContext dbContext, IJwtService jwtService)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    /// <summary>Yeni kullanıcı kaydı oluşturur.</summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validasyon
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("E-posta adresi gereklidir.");

        if (string.IsNullOrWhiteSpace(request.Username))
            throw new ArgumentException("Kullanıcı adı gereklidir.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ArgumentException("Şifre en az 6 karakter olmalıdır.");

        // Email uniqueness kontrolü
        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);
        if (emailExists)
            throw new InvalidOperationException("Bu e-posta adresi zaten kullanılıyor.");

        // Username uniqueness kontrolü
        var usernameExists = await _dbContext.Users
            .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower(), cancellationToken);
        if (usernameExists)
            throw new InvalidOperationException("Bu kullanıcı adı zaten kullanılıyor.");

        // Kullanıcı oluştur
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLower(),
            Username = request.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName?.Trim(),
            IsActive = true,
            LastLoginAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // JWT token üret ve döndür
        var token = _jwtService.GenerateToken(user);
        return BuildAuthResponse(user, token);
    }

    /// <summary>Mevcut kullanıcı ile giriş yapar.</summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.EmailOrUsername))
            throw new ArgumentException("E-posta veya kullanıcı adı gereklidir.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Şifre gereklidir.");

        // Email veya Username ile kullanıcıyı bul
        var identifier = request.EmailOrUsername.Trim().ToLower();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == identifier ||
                u.Username.ToLower() == identifier,
                cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Geçersiz kimlik bilgileri.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Hesap devre dışı bırakılmış.");

        // BCrypt ile şifre doğrula
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Geçersiz kimlik bilgileri.");

        // LastLoginAt güncelle
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        // JWT token üret ve döndür
        var token = _jwtService.GenerateToken(user);
        return BuildAuthResponse(user, token);
    }

    private static AuthResponse BuildAuthResponse(User user, string token)
    {
        return new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            DisplayName = user.DisplayName
        };
    }
}
