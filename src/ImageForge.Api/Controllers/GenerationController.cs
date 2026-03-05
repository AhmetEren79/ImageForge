// GenerationController.cs — Görsel üretim endpoint'leri.
// Tüm endpoint'ler JWT ile korumalıdır.

using System.Security.Claims;
using ImageForge.Application.DTOs.Generation;
using ImageForge.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;

    public GenerationController(IGenerationService generationService)
    {
        _generationService = generationService;
    }

    /// <summary>Yeni görsel üretim isteği oluşturur.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGenerationRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var response = await _generationService.CreateAsync(userId.Value, request, ct);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>Prompt'un mevcut durumunu polling ile sorgular.</summary>
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var response = await _generationService.GetStatusAsync(id, userId.Value, ct);
        if (response == null) return NotFound(new { Error = "Üretim isteği bulunamadı." });

        return Ok(response);
    }

    /// <summary>Kullanıcının geçmiş üretim isteklerini listeler.</summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var history = await _generationService.GetUserHistoryAsync(userId.Value, page, pageSize, ct);
        return Ok(history);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
