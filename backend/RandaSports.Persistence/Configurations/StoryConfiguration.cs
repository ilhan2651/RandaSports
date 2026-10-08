using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.HasMany(x => x.Teams)
            .WithMany()
            .UsingEntity(j => j.ToTable("story_teams"));

        builder.Property(x => x.ClusterKey).HasMaxLength(400);
        builder.Property(x => x.Slug).HasMaxLength(300);
        builder.Property(x => x.Category).HasMaxLength(50);
        builder.Property(x => x.Headline).HasMaxLength(300);
        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.Property(x => x.AiDataJson).HasColumnType("jsonb");

        builder.HasOne(x => x.Sport)
            .WithMany()
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.ClusterKey).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique().HasFilter("slug IS NOT NULL");
        builder.HasIndex(x => x.LastPublishedAt).IsDescending();
        builder.HasIndex(x => new { x.SportId, x.LastPublishedAt }).IsDescending(false, true);
        builder.HasIndex(x => x.NeedsRewrite).HasFilter("needs_rewrite = true");
    }
}
