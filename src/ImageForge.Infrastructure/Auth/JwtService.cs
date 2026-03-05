// JwtService.cs — JWT token oluşturma ve doğrulama servisi.
// appsettings.json'daki Jwt konfigürasyonunu kullanarak token üretir ve validate eder.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ImageForge.Application.Interfaces;
using ImageForge.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ImageForge.Infrastructure.Auth;

public class JwtService : IJwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"]
                   ?? throw new InvalidOperationException("Jwt:Secret yapılandırılmamış.");
        _issuer = configuration["Jwt:Issuer"] ?? "ImageForge";
        _audience = configuration["Jwt:Audience"] ?? "ImageForge";
        _expirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var mins)
            ? mins
            : 1440; // varsayılan 24 saat
    }

    /// <summary>Kullanıcı bilgilerinden JWT token oluşturur.</summary>
    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>Token'ı validate eder ve UserId döndürür. Geçersizse null döner.</summary>
    public Guid? ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)
                              ?? principal.FindFirst(ClaimTypes.NameIdentifier);

            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : null;
        }
        catch
        {
            return null;
        }
    }
}
