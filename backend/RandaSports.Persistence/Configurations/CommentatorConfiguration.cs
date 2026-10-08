using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class CommentatorConfiguration : IEntityTypeConfiguration<Commentator>
{
    public void Configure(EntityTypeBuilder<Commentator> builder)
    {
        builder.Property(x => x.RoleNote).HasMaxLength(300);

        builder.Property(x => x.FullName).HasMaxLength(150);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);

        builder.HasMany(x => x.Sports)
            .WithMany()
            .UsingEntity(j => j.ToTable("commentator_sports"));

        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
