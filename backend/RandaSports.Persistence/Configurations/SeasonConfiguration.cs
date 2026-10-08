using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(20);
        builder.Property(x => x.ExternalId).HasMaxLength(50);

        builder.HasOne(x => x.Competition)
            .WithMany(x => x.Seasons)
            .HasForeignKey(x => x.CompetitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompetitionId, x.Name }).IsUnique();

        builder.HasIndex(x => x.CompetitionId)
            .IsUnique()
            .HasFilter("is_current = true");
    }
}
