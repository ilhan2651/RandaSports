using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(150);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.Property(x => x.ExternalId).HasMaxLength(50);

        builder.HasOne(x => x.Sport)
            .WithMany(x => x.Competitions)
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Teams)
            .WithMany(x => x.Competitions)
            .UsingEntity(j => j.ToTable("competition_teams"));

        builder.HasIndex(x => new { x.SportId, x.Slug }).IsUnique();
        builder.HasIndex(x => x.ExternalId);
    }
}
