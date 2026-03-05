// PromptConfiguration.cs — Prompt entity'si için EF Core Fluent API konfigürasyonu.
// Enum'lar int olarak saklanır. UserId index'i ve cascade delete tanımlanır.

using ImageForge.Domain.Entities;
using ImageForge.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImageForge.Infrastructure.Persistence.Configurations;

public class PromptConfiguration : IEntityTypeConfiguration<Prompt>
{
    public void Configure(EntityTypeBuilder<Prompt> builder)
    {
        builder.ToTable("Prompts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PromptText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.NegativePrompt)
            .HasMaxLength(2000);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(1000);

        // Enum → int dönüşümleri (provider-agnostic uyumluluk)
        builder.Property(p => p.SelectedModel)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.ImageCount)
            .HasDefaultValue(2);

        builder.Property(p => p.Width)
            .HasDefaultValue(1024);

        builder.Property(p => p.Height)
            .HasDefaultValue(1024);

        builder.Property(p => p.Steps)
            .HasDefaultValue(30);

        builder.Property(p => p.CfgScale)
            .HasDefaultValue(7.0);

        // UserId index (performans)
        builder.HasIndex(p => p.UserId);

        // One-to-many: Prompt → GeneratedImages (cascade delete)
        builder.HasMany(p => p.GeneratedImages)
            .WithOne(gi => gi.Prompt)
            .HasForeignKey(gi => gi.PromptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
