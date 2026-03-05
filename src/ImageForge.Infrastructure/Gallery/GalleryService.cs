// GalleryService.cs — Galeri iş mantığı servisi.
// Görsel listeleme, detay, favori toggle, paylaşım toggle, silme (R2 + DB) ve indirme.

using ImageForge.Application.DTOs.Gallery;
using ImageForge.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageForge.Infrastructure.Gallery;

public class GalleryService : IGalleryService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IObjectStorageService _storageService;
    private readonly ILogger<GalleryService> _logger;

    public GalleryService(
        IApplicationDbContext dbContext,
        IObjectStorageService storageService,
        ILogger<GalleryService> logger)
    {
        _dbContext = dbContext;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<PagedResult<GalleryImageResponse>> GetUserImagesAsync(
        Guid userId, int page = 1, int pageSize = 20, bool? onlyFavorites = null, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;

        var query = _dbContext.GeneratedImages
            .Include(img => img.Prompt)
            .Where(img => img.Prompt.UserId == userId);

        if (onlyFavorites == true)
            query = query.Where(img => img.IsFavorite);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(img => img.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(img => new GalleryImageResponse
            {
                Id = img.Id,
                StorageUrl = img.StorageUrl,
                FileName = img.FileName,
                Width = img.Width,
                Height = img.Height,
                IsFavorite = img.IsFavorite,
                IsPublic = img.IsPublic,
                CreatedAt = img.CreatedAt,
                PromptText = img.Prompt.PromptText,
                SelectedModel = img.Prompt.SelectedModel.ToString()
            })
            .ToListAsync(ct);

        return new PagedResult<GalleryImageResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<GalleryImageDetailResponse?> GetImageDetailAsync(
        Guid imageId, Guid userId, CancellationToken ct = default)
    {
        var img = await _dbContext.GeneratedImages
            .Include(i => i.Prompt)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Prompt.UserId == userId, ct);

        return img == null ? null : MapToDetail(img);
    }

    public async Task<GalleryImageResponse?> ToggleFavoriteAsync(
        Guid imageId, Guid userId, CancellationToken ct = default)
    {
        var img = await _dbContext.GeneratedImages
            .Include(i => i.Prompt)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Prompt.UserId == userId, ct);

        if (img == null) return null;

        img.IsFavorite = !img.IsFavorite;
        img.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Favori toggle. ImageId: {ImageId}, IsFavorite: {IsFavorite}", imageId, img.IsFavorite);

        return new GalleryImageResponse
        {
            Id = img.Id,
            StorageUrl = img.StorageUrl,
            FileName = img.FileName,
            Width = img.Width,
            Height = img.Height,
            IsFavorite = img.IsFavorite,
            IsPublic = img.IsPublic,
            CreatedAt = img.CreatedAt,
            PromptText = img.Prompt.PromptText,
            SelectedModel = img.Prompt.SelectedModel.ToString()
        };
    }

    public async Task<ShareLinkResponse?> TogglePublicShareAsync(
        Guid imageId, Guid userId, CancellationToken ct = default)
    {
        var img = await _dbContext.GeneratedImages
            .Include(i => i.Prompt)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Prompt.UserId == userId, ct);

        if (img == null) return null;

        if (img.IsPublic)
        {
            // Paylaşımı kapat
            img.IsPublic = false;
            img.PublicShareToken = null;
            _logger.LogInformation("Paylaşım kapatıldı. ImageId: {ImageId}", imageId);
        }
        else
        {
            // Paylaşım aç — benzersiz token üret
            img.IsPublic = true;
            img.PublicShareToken = Guid.NewGuid().ToString("N")[..16];
            _logger.LogInformation("Paylaşım açıldı. ImageId: {ImageId}, Token: {Token}", imageId, img.PublicShareToken);
        }

        img.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);

        return new ShareLinkResponse
        {
            ImageId = img.Id,
            IsPublic = img.IsPublic,
            ShareToken = img.PublicShareToken
        };
    }

    public async Task<bool> DeleteImageAsync(
        Guid imageId, Guid userId, CancellationToken ct = default)
    {
        var img = await _dbContext.GeneratedImages
            .Include(i => i.Prompt)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Prompt.UserId == userId, ct);

        if (img == null) return false;

        // Önce R2'den sil
        try
        {
            await _storageService.DeleteAsync(img.StorageKey, ct);
            _logger.LogInformation("R2'den silindi. StorageKey: {StorageKey}", img.StorageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "R2'den silme başarısız. StorageKey: {StorageKey} — DB'den yine de silinecek.", img.StorageKey);
        }

        // DB'den sil
        _dbContext.GeneratedImages.Remove(img);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Görsel silindi. ImageId: {ImageId}", imageId);
        return true;
    }

    public async Task<GalleryImageDetailResponse?> GetImageByShareTokenAsync(
        string shareToken, CancellationToken ct = default)
    {
        var img = await _dbContext.GeneratedImages
            .Include(i => i.Prompt)
            .FirstOrDefaultAsync(i => i.IsPublic && i.PublicShareToken == shareToken, ct);

        return img == null ? null : MapToDetail(img);
    }

    public async Task<(Stream Stream, string FileName, string ContentType)?> GetDownloadStreamAsync(
        Guid imageId, Guid userId, CancellationToken ct = default)
    {
        var img = await _dbContext.GeneratedImages
            .Include(i => i.Prompt)
            .FirstOrDefaultAsync(i => i.Id == imageId && i.Prompt.UserId == userId, ct);

        if (img == null) return null;

        var stream = await _storageService.DownloadAsync(img.StorageKey, ct);
        var contentType = GetContentType(img.FileName);

        return (stream, img.FileName, contentType);
    }

    // ─── Private helpers ───

    private static GalleryImageDetailResponse MapToDetail(Domain.Entities.GeneratedImage img)
    {
        return new GalleryImageDetailResponse
        {
            Id = img.Id,
            StorageUrl = img.StorageUrl,
            FileName = img.FileName,
            FileSizeBytes = img.FileSizeBytes,
            Width = img.Width,
            Height = img.Height,
            Seed = img.Seed,
            IsFavorite = img.IsFavorite,
            IsPublic = img.IsPublic,
            PublicShareToken = img.PublicShareToken,
            CreatedAt = img.CreatedAt,
            UpdatedAt = img.UpdatedAt,
            PromptId = img.PromptId,
            PromptText = img.Prompt.PromptText,
            NegativePrompt = img.Prompt.NegativePrompt,
            SelectedModel = img.Prompt.SelectedModel.ToString(),
            Steps = img.Prompt.Steps,
            CfgScale = img.Prompt.CfgScale
        };
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }
}
