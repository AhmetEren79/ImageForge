// IGenerationService.cs — Görsel üretim iş mantığı interface'i.

using ImageForge.Application.DTOs.Generation;

namespace ImageForge.Application.Interfaces;

public interface IGenerationService
{
    /// <summary>Yeni görsel üretim isteği oluşturur ve AI servisine gönderir.</summary>
    Task<GenerationStatusResponse> CreateAsync(
        Guid userId, CreateGenerationRequest request, CancellationToken ct = default);

    /// <summary>Prompt'un mevcut durumunu döndürür (polling).</summary>
    Task<GenerationStatusResponse?> GetStatusAsync(
        Guid promptId, Guid userId, CancellationToken ct = default);

    /// <summary>Kullanıcının geçmiş üretim isteklerini sayfalı listeler.</summary>
    Task<List<GenerationStatusResponse>> GetUserHistoryAsync(
        Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default);

    /// <summary>AI servisinden gelen webhook callback'i işler.</summary>
    Task ProcessCallbackAsync(AiCallbackPayload payload, CancellationToken ct = default);
}
