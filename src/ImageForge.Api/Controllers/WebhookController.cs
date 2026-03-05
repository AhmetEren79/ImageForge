// WebhookController.cs — Python AI servisinden gelen callback endpoint'i.
// AI servisi üretimi tamamladığında veya hata aldığında bu endpoint'e POST yapar.

using ImageForge.Application.DTOs.Generation;
using ImageForge.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ImageForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly IGenerationService _generationService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        IGenerationService generationService,
        ILogger<WebhookController> logger)
    {
        _generationService = generationService;
        _logger = logger;
    }

    /// <summary>AI servisi üretim sonucunu bu endpoint'e gönderir.</summary>
    [HttpPost("generation-complete")]
    public async Task<IActionResult> GenerationComplete(
        [FromBody] AiCallbackPayload payload, CancellationToken ct)
    {
        _logger.LogInformation(
            "Webhook callback alındı. PromptId: {PromptId}, Status: {Status}",
            payload.PromptId, payload.Status);

        try
        {
            await _generationService.ProcessCallbackAsync(payload, ct);
            return Ok(new { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook callback işlenirken hata. PromptId: {PromptId}", payload.PromptId);
            return StatusCode(500, new { Error = "Callback işlenirken hata oluştu.", Detail = ex.ToString() });
        }
    }
}
