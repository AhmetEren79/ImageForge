// Program.cs — ImageForge API giriş noktası.
// Infrastructure DI, JWT Authentication, CORS, Swagger ve controller routing'i konfigüre eder.
// Development modunda otomatik migration uygular.

using ImageForge.Infrastructure;
using ImageForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- .env dosyasını yükle (Local Development) ---
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}
else if (File.Exists(".env"))
{
    DotNetEnv.Env.Load(".env");
}

// Environment variable'ları Configuration'a ekle (Docker'da env_file ile gelir)
builder.Configuration.AddEnvironmentVariables();

// --- Servis Kayıtları ---

// Infrastructure katmanı servisleri (DbContext, JWT Auth, provider-agnostic)
builder.Services.AddInfrastructure(builder.Configuration);

// Controller'lar
builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — Frontend ve Docker erişimine izin ver
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5000",
                "http://localhost",
                "http://imageforge-frontend",
                "http://imageforge-frontend:80"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// --- Middleware Pipeline ---

// Development modunda otomatik migration
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ImageForge API v1");
    });
}

app.UseCors("AllowFrontend");

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
