using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(500);
        builder.Property(x => x.Excerpt).HasMaxLength(2000);
        builder.Property(x => x.Url).HasMaxLength(1000);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.Property(x => x.Author).HasMaxLength(150);
        builder.Property(x => x.AiHeadline).HasMaxLength(300);
        builder.Property(x => x.AiDataJson).HasColumnType("jsonb");

        builder.HasOne(x => x.Source)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sport)
            .WithMany()
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Story)
            .WithMany(x => x.Articles)
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Url).IsUnique();
        builder.HasIndex(x => x.PublishedAt).IsDescending();
        builder.HasIndex(x => new { x.SportId, x.PublishedAt }).IsDescending(false, true);
        builder.HasIndex(x => x.AiProcessedAt).HasFilter("ai_processed_at IS NULL");
        builder.HasIndex(x => x.StoryId).HasFilter("story_id IS NULL");
    }
}
