// ApplicationDbContext.cs — EF Core veritabanı context implementasyonu.
// SaveChangesAsync override ile CreatedAt/UpdatedAt otomatik yönetimi sağlar.
// IEntityTypeConfiguration'lar ile Fluent API konfigürasyonlarını uygular.

using ImageForge.Application.Interfaces;
using ImageForge.Domain.Common;
using ImageForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageForge.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<GeneratedImage> GeneratedImages => Set<GeneratedImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tüm IEntityTypeConfiguration'ları bu assembly'den otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                        entry.Entity.Id = Guid.NewGuid();
                    entry.Entity.CreatedAt = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
