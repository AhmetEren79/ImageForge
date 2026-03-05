// GenerationService.cs — Görsel üretim iş mantığı servisi.
// Prompt CRUD, AI servisi orchestration, webhook callback işleme, kullanıcı geçmişi.

using ImageForge.Application.DTOs.Generation;
using ImageForge.Application.Interfaces;
using ImageForge.Domain.Entities;
using ImageForge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ImageForge.Infrastructure.AiGeneration;

public class GenerationService : IGenerationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IAiGenerationService _aiService;
    private readonly ILogger<GenerationService> _logger;

    public GenerationService(
        IApplicationDbContext dbContext,
        IAiGenerationService aiService,
        ILogger<GenerationService> logger)
    {
        _dbContext = dbContext;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<GenerationStatusResponse> CreateAsync(
        Guid userId, CreateGenerationRequest request, CancellationToken ct = default)
    {
        // Validasyon
        if (string.IsNullOrWhiteSpace(request.PromptText))
            throw new ArgumentException("Prompt metni gereklidir.");

        if (request.ImageCount < 1 || request.ImageCount > 3)
            throw new ArgumentException("Görsel sayısı 1-3 arasında olmalıdır.");

        if (request.Width < 256 || request.Width > 2048 || request.Height < 256 || request.Height > 2048)
            throw new ArgumentException("Genişlik ve yükseklik 256-2048 px arasında olmalıdır.");

        // Prompt entity oluştur
        var prompt = new Prompt
        {
            Id = Guid.NewGuid(),
            PromptText = request.PromptText.Trim(),
            NegativePrompt = request.NegativePrompt?.Trim(),
            SelectedModel = request.SelectedModel,
            Status = GenerationStatus.Pending,
            ImageCount = request.ImageCount,
            Width = request.Width,
            Height = request.Height,
            Steps = request.Steps,
            CfgScale = request.CfgScale,
            Seed = request.Seed,
            UserId = userId
        };

        _dbContext.Prompts.Add(prompt);
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Prompt oluşturuldu. PromptId: {PromptId}, UserId: {UserId}", prompt.Id, userId);

        // AI servisine fire-and-forget istek (hata olursa prompt Pending kalır)
        try
        {
            prompt.Status = GenerationStatus.Processing;
            await _dbContext.SaveChangesAsync(ct);

            await _aiService.RequestGenerationAsync(
                prompt.Id,
                prompt.PromptText,
                prompt.NegativePrompt,
                prompt.SelectedModel,
                prompt.ImageCount,
                prompt.Width,
                prompt.Height,
                prompt.Steps,
                prompt.CfgScale,
                prompt.Seed,
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "AI servisine istek gönderilemedi. PromptId: {PromptId} — Pending durumunda bırakıldı.",
                prompt.Id);
            prompt.Status = GenerationStatus.Pending;
            await _dbContext.SaveChangesAsync(ct);
        }

        return MapToResponse(prompt);
    }

    public async Task<GenerationStatusResponse?> GetStatusAsync(
        Guid promptId, Guid userId, CancellationToken ct = default)
    {
        var prompt = await _dbContext.Prompts
            .Include(p => p.GeneratedImages)
            .FirstOrDefaultAsync(p => p.Id == promptId && p.UserId == userId, ct);

        return prompt == null ? null : MapToResponse(prompt);
    }

    public async Task<List<GenerationStatusResponse>> GetUserHistoryAsync(
        Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 50) pageSize = 50;

        var prompts = await _dbContext.Prompts
            .Include(p => p.GeneratedImages)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return prompts.Select(MapToResponse).ToList();
    }

    public async Task ProcessCallbackAsync(AiCallbackPayload payload, CancellationToken ct = default)
    {
        if (!Guid.TryParse(payload.PromptId, out var promptId))
        {
            _logger.LogWarning("Geçersiz callback prompt_id: {PromptId}", payload.PromptId);
            return;
        }

        var prompt = await _dbContext.Prompts
            .FirstOrDefaultAsync(p => p.Id == promptId, ct);

        if (prompt == null)
        {
            _logger.LogWarning("Callback için prompt bulunamadı. PromptId: {PromptId}", promptId);
            return;
        }

        if (payload.Status == "completed")
        {
            prompt.Status = GenerationStatus.Completed;

            var images = payload.Images.Select(img => new GeneratedImage
            {
                Id = Guid.NewGuid(),
                StorageUrl = img.Url,
                StorageKey = img.StorageKey,
                FileName = img.FileName,
                FileSizeBytes = img.FileSize,
                Width = img.Width,
                Height = img.Height,
                Seed = img.Seed,
                IsFavorite = false,
                IsPublic = false,
                PromptId = promptId
            }).ToList();

            _dbContext.GeneratedImages.AddRange(images);

            _logger.LogInformation(
                "Üretim tamamlandı. PromptId: {PromptId}, ImageCount: {Count}",
                promptId, payload.Images.Count);
        }
        else
        {
            prompt.Status = GenerationStatus.Failed;
            prompt.ErrorMessage = payload.Error ?? "Bilinmeyen hata.";
            _logger.LogError("Üretim başarısız. PromptId: {PromptId}, Error: {Error}",
                promptId, payload.Error);
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static GenerationStatusResponse MapToResponse(Prompt prompt)
    {
        return new GenerationStatusResponse
        {
            PromptId = prompt.Id,
            Status = prompt.Status.ToString(),
            PromptText = prompt.PromptText,
            SelectedModel = prompt.SelectedModel.ToString(),
            ImageCount = prompt.ImageCount,
            ErrorMessage = prompt.ErrorMessage,
            CreatedAt = prompt.CreatedAt,
            Images = prompt.GeneratedImages.Select(img => new GeneratedImageDto
            {
                Id = img.Id,
                StorageUrl = img.StorageUrl,
                FileName = img.FileName,
                FileSizeBytes = img.FileSizeBytes,
                Width = img.Width,
                Height = img.Height,
                Seed = img.Seed,
                IsFavorite = img.IsFavorite,
                CreatedAt = img.CreatedAt
            }).ToList()
        };
    }
}
