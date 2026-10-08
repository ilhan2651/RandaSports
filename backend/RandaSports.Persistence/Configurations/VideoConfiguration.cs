using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.Property(x => x.YouTubeVideoId).HasMaxLength(20);
        builder.Property(x => x.Title).HasMaxLength(300);
        builder.Property(x => x.ThumbnailUrl).HasMaxLength(500);
        builder.Property(x => x.ProcessingError).HasMaxLength(2000);

        builder.HasOne(x => x.Channel)
            .WithMany(x => x.Videos)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.YouTubeVideoId).IsUnique();
        builder.HasIndex(x => new { x.ProcessingStatus, x.PublishedAt });
        builder.HasIndex(x => new { x.ChannelId, x.PublishedAt });
    }
}
