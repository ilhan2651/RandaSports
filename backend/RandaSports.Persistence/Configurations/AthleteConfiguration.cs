using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class AthleteConfiguration : IEntityTypeConfiguration<Athlete>
{
    public void Configure(EntityTypeBuilder<Athlete> builder)
    {
        builder.Property(x => x.FullName).HasMaxLength(150);
        builder.Property(x => x.Nickname).HasMaxLength(100);
        builder.Property(x => x.Slug).HasMaxLength(150);
        builder.Property(x => x.Nationality).HasMaxLength(100);
        builder.Property(x => x.BirthPlace).HasMaxLength(150);
        builder.Property(x => x.Position).HasMaxLength(50);
        builder.Property(x => x.PhotoUrl).HasMaxLength(500);
        builder.Property(x => x.ExternalId).HasMaxLength(50);
        builder.Property(x => x.StatsJson).HasColumnType("jsonb");

        builder.HasOne(x => x.Sport)
            .WithMany()
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Team)
            .WithMany(x => x.Athletes)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.SportId, x.Slug }).IsUnique();
        builder.HasIndex(x => x.ExternalId);
        builder.HasIndex(x => x.TeamId);
    }
}
