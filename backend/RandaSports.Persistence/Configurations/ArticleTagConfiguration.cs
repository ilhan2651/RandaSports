using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTag>
{
    public void Configure(EntityTypeBuilder<ArticleTag> builder)
    {
        builder.HasKey(x => new { x.ArticleId, x.EntityType, x.EntityId });

        builder.HasOne(x => x.Article)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.EntityType, x.EntityId });
    }
}
