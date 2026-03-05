// AiGenerationService.cs — Python FastAPI servisine HTTP isteği gönderen servis.
// HttpClientFactory ile named client kullanarak AI servisine görsel üretim isteği iletir.

using System.Text;
using System.Text.Json;
using ImageForge.Application.DTOs.Generation;
using ImageForge.Application.Interfaces;
using ImageForge.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ImageForge.Infrastructure.AiGeneration;

public class AiGenerationService : IAiGenerationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _callbackBaseUrl;
    private readonly ILogger<AiGenerationService> _logger;

    public AiGenerationService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AiGenerationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _callbackBaseUrl = configuration["AiService:CallbackBaseUrl"] ?? "http://localhost:5265";
        _logger = logger;
    }

    public async Task RequestGenerationAsync(
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
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("AiService");

        var request = new AiServiceRequest
        {
            PromptId = promptId.ToString(),
            Prompt = promptText,
            NegativePrompt = negativePrompt,
            Model = model.ToString(),
            ImageCount = imageCount,
            Width = width,
            Height = height,
            Steps = steps,
            CfgScale = cfgScale,
            Seed = seed,
            CallbackUrl = $"{_callbackBaseUrl}/api/webhook/generation-complete"
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation(
            "AI servisine istek gönderiliyor. PromptId: {PromptId}, Model: {Model}",
            promptId, model);

        try
        {
            var response = await client.PostAsync("/api/generate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "AI servisi hata döndü. PromptId: {PromptId}, StatusCode: {StatusCode}, Body: {Body}",
                    promptId, response.StatusCode, errorBody);
                throw new HttpRequestException(
                    $"AI servisi hata döndü: {response.StatusCode}");
            }

            _logger.LogInformation("AI servisi isteği kabul etti. PromptId: {PromptId}", promptId);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused") ||
                                                ex.Message.Contains("No connection"))
        {
            _logger.LogWarning(
                "AI servisi erişilemez durumda. PromptId: {PromptId} — istek kuyruğa alındı olarak kabul edildi.",
                promptId);
            // AI servisi çalışmıyorsa sessizce devam et (development ortamı)
        }
    }
}
