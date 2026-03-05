// AuthController.cs — Kayıt, giriş ve mevcut kullanıcı bilgisi endpoint'leri.

using System.Security.Claims;
using ImageForge.Application.DTOs.Auth;
using ImageForge.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IApplicationDbContext _dbContext;

    public AuthController(IAuthService authService, IApplicationDbContext dbContext)
    {
        _authService = authService;
        _dbContext = dbContext;
    }

    /// <summary>Yeni kullanıcı kaydı.</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.RegisterAsync(request, ct);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Error = ex.Message });
        }
    }

    /// <summary>Kullanıcı girişi.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        try
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }

    /// <summary>Mevcut kullanıcı bilgilerini döndürür (JWT gerekli).</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(new { Error = "Geçersiz token." });

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user == null)
            return NotFound(new { Error = "Kullanıcı bulunamadı." });

        return Ok(new
        {
            user.Id,
            user.Email,
            user.Username,
            user.DisplayName,
            user.IsActive,
            user.LastLoginAt,
            user.CreatedAt
        });
    }
}
