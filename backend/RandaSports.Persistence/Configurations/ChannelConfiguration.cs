using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.HasMany(x => x.Sports)
            .WithMany()
            .UsingEntity(j => j.ToTable("channel_sports"));

        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.Handle).HasMaxLength(100);
        builder.Property(x => x.YouTubeChannelId).HasMaxLength(50);
        builder.Property(x => x.LastError).HasMaxLength(1000);

        builder.HasMany(x => x.RegularCommentators)
            .WithMany(x => x.Channels)
            .UsingEntity(j => j.ToTable("channel_commentators"));

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Handle).IsUnique();

        // Kanal kimliği çözülene kadar boş kalıyor. PostgreSQL benzersiz indekste
        // NULL'ları birbirinden farklı saydığı için filtreye gerek yok: aynı kimlik
        // iki kez girmiyor, ama çözülmemiş kanallar yan yana durabiliyor.
        builder.HasIndex(x => x.YouTubeChannelId).IsUnique();
    }
}
