// GalleryController.cs — Galeri endpoint'leri.
// Tüm endpoint'ler JWT ile korumalıdır.

using System.Security.Claims;
using ImageForge.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GalleryController : ControllerBase
{
    private readonly IGalleryService _galleryService;

    public GalleryController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    /// <summary>Kullanıcının görsellerini sayfalanmış listeler.</summary>
    [HttpGet]
    public async Task<IActionResult> GetImages(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? onlyFavorites = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _galleryService.GetUserImagesAsync(userId.Value, page, pageSize, onlyFavorites, ct);
        return Ok(result);
    }

    /// <summary>Tekil görsel detayını döndürür.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _galleryService.GetImageDetailAsync(id, userId.Value, ct);
        if (result == null) return NotFound(new { Error = "Görsel bulunamadı." });

        return Ok(result);
    }

    /// <summary>Favori durumunu tersine çevirir.</summary>
    [HttpPatch("{id}/favorite")]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _galleryService.ToggleFavoriteAsync(id, userId.Value, ct);
        if (result == null) return NotFound(new { Error = "Görsel bulunamadı." });

        return Ok(result);
    }

    /// <summary>Public paylaşım linkini oluşturur veya kaldırır.</summary>
    [HttpPatch("{id}/share")]
    public async Task<IActionResult> ToggleShare(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _galleryService.TogglePublicShareAsync(id, userId.Value, ct);
        if (result == null) return NotFound(new { Error = "Görsel bulunamadı." });

        return Ok(result);
    }

    /// <summary>Görseli R2'den ve veritabanından siler.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var deleted = await _galleryService.DeleteImageAsync(id, userId.Value, ct);
        if (!deleted) return NotFound(new { Error = "Görsel bulunamadı." });

        return Ok(new { Success = true, Message = "Görsel başarıyla silindi." });
    }

    /// <summary>Görseli R2'den indirir (dosya stream).</summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _galleryService.GetDownloadStreamAsync(id, userId.Value, ct);
        if (result == null) return NotFound(new { Error = "Görsel bulunamadı." });

        var (stream, fileName, contentType) = result.Value;
        return File(stream, contentType, fileName);
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
