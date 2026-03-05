// DependencyInjection.cs — Infrastructure katmanı servis kayıtları.
// DbContext, JWT Authentication ve Auth servislerini konfigüre eder.

using System.Text;
using ImageForge.Application.Interfaces;
using ImageForge.Infrastructure.Auth;
using ImageForge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ImageForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // --- Veritabanı ---
        var provider = (configuration["DatabaseProvider"] ?? "sqlite").ToLowerInvariant();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider)
            {
                case "postgresql":
                    options.UseNpgsql(
                        configuration.GetConnectionString("PostgreSQL"),
                        npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                    break;

                case "sqlite":
                default:
                    options.UseSqlite(
                        configuration.GetConnectionString("SQLite"),
                        sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                    break;
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // --- JWT Authentication ---
        var jwtSecret = configuration["Jwt:Secret"]
                        ?? throw new InvalidOperationException("Jwt:Secret yapılandırılmamış.");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "ImageForge";
        var jwtAudience = configuration["Jwt:Audience"] ?? "ImageForge";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // --- Auth Servisleri ---
        services.AddSingleton<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        // --- AI Generation Servisleri ---
        var aiServiceBaseUrl = configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
        services.AddHttpClient("AiService", client =>
        {
            client.BaseAddress = new Uri(aiServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddScoped<IAiGenerationService, AiGeneration.AiGenerationService>();
        services.AddScoped<IGenerationService, AiGeneration.GenerationService>();

        // --- Gallery Servisi ---
        services.AddScoped<IObjectStorageService, Storage.R2ObjectStorageService>();
        services.AddScoped<IGalleryService, Gallery.GalleryService>();

        return services;
    }
}
