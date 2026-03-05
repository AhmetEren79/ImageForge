// IGalleryService.cs — Galeri iş mantığı interface'i.
// Görsel listeleme, detay, favori, paylaşım, silme ve indirme işlemleri.

using ImageForge.Application.DTOs.Gallery;

namespace ImageForge.Application.Interfaces;

public interface IGalleryService
{
    /// <summary>Kullanıcının görsellerini sayfalı listeler (opsiyonel favori filtresi).</summary>
    Task<PagedResult<GalleryImageResponse>> GetUserImagesAsync(
        Guid userId, int page = 1, int pageSize = 20, bool? onlyFavorites = null, CancellationToken ct = default);

    /// <summary>Tekil görsel detayını döndürür.</summary>
    Task<GalleryImageDetailResponse?> GetImageDetailAsync(
        Guid imageId, Guid userId, CancellationToken ct = default);

    /// <summary>Favori durumunu tersine çevirir.</summary>
    Task<GalleryImageResponse?> ToggleFavoriteAsync(
        Guid imageId, Guid userId, CancellationToken ct = default);

    /// <summary>Public paylaşım linkini oluşturur veya kaldırır.</summary>
    Task<ShareLinkResponse?> TogglePublicShareAsync(
        Guid imageId, Guid userId, CancellationToken ct = default);

    /// <summary>Görseli R2'den ve veritabanından siler.</summary>
    Task<bool> DeleteImageAsync(
        Guid imageId, Guid userId, CancellationToken ct = default);

    /// <summary>Public paylaşım token'ı ile görsel detayını döndürür (anonim erişim).</summary>
    Task<GalleryImageDetailResponse?> GetImageByShareTokenAsync(
        string shareToken, CancellationToken ct = default);

    /// <summary>R2'den görsel dosyasını stream olarak indirir.</summary>
    Task<(Stream Stream, string FileName, string ContentType)?> GetDownloadStreamAsync(
        Guid imageId, Guid userId, CancellationToken ct = default);
}
