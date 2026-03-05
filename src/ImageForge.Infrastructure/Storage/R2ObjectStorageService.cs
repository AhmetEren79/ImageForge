// R2ObjectStorageService.cs — Cloudflare R2 gerçek depolama servisi.
// AWS S3 uyumlu client ile dosya yükleme, silme ve indirme.

using Amazon.S3;
using Amazon.S3.Model;
using ImageForge.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ImageForge.Infrastructure.Storage;

public class R2ObjectStorageService : IObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _publicUrl;
    private readonly ILogger<R2ObjectStorageService> _logger;

    public R2ObjectStorageService(
        IConfiguration configuration,
        ILogger<R2ObjectStorageService> logger)
    {
        _logger = logger;

        var endpoint = configuration["R2_ENDPOINT"]
            ?? Environment.GetEnvironmentVariable("R2_ENDPOINT")
            ?? throw new InvalidOperationException("R2_ENDPOINT yapılandırılmamış.");

        var accessKey = configuration["R2_ACCESS_KEY_ID"]
            ?? Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID")
            ?? throw new InvalidOperationException("R2_ACCESS_KEY_ID yapılandırılmamış.");

        var secretKey = configuration["R2_SECRET_ACCESS_KEY"]
            ?? Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY")
            ?? throw new InvalidOperationException("R2_SECRET_ACCESS_KEY yapılandırılmamış.");

        _bucketName = configuration["R2_BUCKET_NAME"]
            ?? Environment.GetEnvironmentVariable("R2_BUCKET_NAME")
            ?? "imageforge-dev";

        _publicUrl = configuration["R2_PUBLIC_URL"]
            ?? Environment.GetEnvironmentVariable("R2_PUBLIC_URL")
            ?? "";

        var s3Config = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true,
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, s3Config);

        _logger.LogInformation("R2 Storage başlatıldı. Bucket: {Bucket}, Endpoint: {Endpoint}",
            _bucketName, endpoint);
    }

    public async Task<(string StorageUrl, string StorageKey)> UploadAsync(
        Stream fileStream, string fileName, string contentType,
        CancellationToken cancellationToken = default)
    {
        var key = $"generations/{Guid.NewGuid():N}/{fileName}";

        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = fileStream,
            ContentType = contentType,
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);

        var publicUrl = string.IsNullOrEmpty(_publicUrl)
            ? $"https://{_bucketName}.r2.dev/{key}"
            : $"{_publicUrl}/{key}";

        _logger.LogInformation("R2'ye yüklendi: {Key} ({ContentType})", key, contentType);

        return (publicUrl, key);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
        };

        await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
        _logger.LogInformation("R2'den silindi: {Key}", storageKey);
    }

    public async Task<Stream> DownloadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = storageKey,
        };

        var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);

        // MemoryStream'e kopyala (ResponseStream dispose edilebilir)
        var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);
        ms.Position = 0;

        _logger.LogInformation("R2'den indirildi: {Key}", storageKey);
        return ms;
    }
}
