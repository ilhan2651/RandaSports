using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.ShortName).HasMaxLength(10);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.ExternalId).HasMaxLength(50);
        builder.Property(x => x.Aliases).HasColumnType("text[]");

        builder.HasOne(x => x.Sport)
            .WithMany()
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SportId, x.Slug }).IsUnique();
        builder.HasIndex(x => x.ExternalId);
    }
}
