// PublicController.cs — Anonim erişim endpoint'leri.
// Public paylaşım token'ı ile görsellere erişim sağlar.

using ImageForge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImageForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PublicController : ControllerBase
{
    private readonly IGalleryService _galleryService;

    public PublicController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    /// <summary>Public paylaşım token'ı ile görsel detayını görüntüler.</summary>
    [HttpGet("share/{token}")]
    public async Task<IActionResult> GetSharedImage(string token, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { Error = "Token gereklidir." });

        var result = await _galleryService.GetImageByShareTokenAsync(token, ct);
        if (result == null) return NotFound(new { Error = "Paylaşılan görsel bulunamadı veya paylaşım kapatılmış." });

        return Ok(result);
    }
}
