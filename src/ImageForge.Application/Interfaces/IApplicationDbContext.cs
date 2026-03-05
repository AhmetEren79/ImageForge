// IApplicationDbContext.cs — Veritabanı context interface'i.
// Application katmanının Infrastructure'a bağımlı olmaması için soyutlama sağlar.

using ImageForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageForge.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Prompt> Prompts { get; }
    DbSet<GeneratedImage> GeneratedImages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
