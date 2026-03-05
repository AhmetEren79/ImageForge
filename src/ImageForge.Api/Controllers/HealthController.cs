// HealthController.cs — Basit sağlık kontrolü endpoint'i.
// API'nin çalıştığını ve DB bağlantısının aktif olduğunu doğrular.

using ImageForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ImageForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public HealthController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>API ve veritabanı sağlık kontrolü.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var canConnect = await _dbContext.Database.CanConnectAsync();
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Database = canConnect ? "Connected" : "Disconnected"
        });
    }
}
