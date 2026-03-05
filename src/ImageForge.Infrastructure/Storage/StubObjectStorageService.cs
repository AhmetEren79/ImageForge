// StubObjectStorageService.cs — Geliştirme ortamı için stub R2 depolama servisi.
// Gerçek Cloudflare R2 entegrasyonu sonraki task'ta implement edilecek.

using ImageForge.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ImageForge.Infrastructure.Storage;

public class StubObjectStorageService : IObjectStorageService
{
    private readonly ILogger<StubObjectStorageService> _logger;

    public StubObjectStorageService(ILogger<StubObjectStorageService> logger)
    {
        _logger = logger;
    }

    public Task<(string StorageUrl, string StorageKey)> UploadAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("StubObjectStorageService.UploadAsync çağrıldı — gerçek R2 bağlantısı yok.");
        var key = $"stub/{Guid.NewGuid():N}/{fileName}";
        var url = $"https://stub-r2.example.com/{key}";
        return Task.FromResult((url, key));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("StubObjectStorageService.DeleteAsync çağrıldı — gerçek R2 bağlantısı yok. Key: {Key}", storageKey);
        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("StubObjectStorageService.DownloadAsync çağrıldı — gerçek R2 bağlantısı yok. Key: {Key}", storageKey);
        // Boş bir stream döndür (geliştirme ortamında)
        Stream emptyStream = new MemoryStream();
        return Task.FromResult(emptyStream);
    }
}
