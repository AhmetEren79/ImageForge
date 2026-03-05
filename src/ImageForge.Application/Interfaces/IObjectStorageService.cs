// IObjectStorageService.cs — Cloudflare R2 nesne depolama interface'i.
// Task 4+'te implement edilecek.

namespace ImageForge.Application.Interfaces;

public interface IObjectStorageService
{
    /// <summary>Dosyayı R2'ye yükler, public URL ve object key döner.</summary>
    Task<(string StorageUrl, string StorageKey)> UploadAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>R2'den dosya siler.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>R2'den dosya indirir.</summary>
    Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default);
}
