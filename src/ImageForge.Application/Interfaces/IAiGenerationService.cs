// IAiGenerationService.cs — AI görsel üretim servisine HTTP isteği gönderen interface.
// Task 4+'te implement edilecek.

using ImageForge.Domain.Enums;

namespace ImageForge.Application.Interfaces;

public interface IAiGenerationService
{
    /// <summary>Python FastAPI servisine görsel üretim isteği gönderir.</summary>
    Task RequestGenerationAsync(
        Guid promptId,
        string promptText,
        string? negativePrompt,
        AiModelType model,
        int imageCount,
        int width,
        int height,
        int steps,
        double cfgScale,
        long? seed,
        CancellationToken cancellationToken = default);
}
