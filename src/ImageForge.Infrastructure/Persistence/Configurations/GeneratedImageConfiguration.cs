// GeneratedImageConfiguration.cs — GeneratedImage entity'si için EF Core Fluent API konfigürasyonu.
// PublicShareToken için filtered unique index ve PromptId index tanımlanır.

using ImageForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageForge.Infrastructure.Persistence.Configurations;

public class GeneratedImageConfiguration : IEntityTypeConfiguration<GeneratedImage>
{
    public void Configure(EntityTypeBuilder<GeneratedImage> builder)
    {
        builder.ToTable("GeneratedImages");

        builder.HasKey(gi => gi.Id);

        builder.Property(gi => gi.StorageUrl)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(gi => gi.StorageKey)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(gi => gi.FileName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(gi => gi.IsFavorite)
            .HasDefaultValue(false);

        builder.Property(gi => gi.IsPublic)
            .HasDefaultValue(false);

        builder.Property(gi => gi.PublicShareToken)
            .HasMaxLength(64);

        // Filtered unique index: sadece null olmayan PublicShareToken'lar benzersiz olmalı
        builder.HasIndex(gi => gi.PublicShareToken)
            .IsUnique()
            .HasFilter("\"PublicShareToken\" IS NOT NULL");

        // PromptId index (performans)
        builder.HasIndex(gi => gi.PromptId);
    }
}
