using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class StandingConfiguration : IEntityTypeConfiguration<Standing>
{
    public void Configure(EntityTypeBuilder<Standing> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_standings_team_or_athlete",
            "team_id IS NOT NULL OR athlete_id IS NOT NULL"));

        builder.Property(x => x.GroupName).HasMaxLength(50);
        builder.Property(x => x.Form).HasMaxLength(10);
        builder.Property(x => x.StatsJson).HasColumnType("jsonb");

        builder.HasOne(x => x.Season)
            .WithMany(x => x.Standings)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Athlete)
            .WithMany()
            .HasForeignKey(x => x.AthleteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SeasonId, x.GroupName, x.Rank });
    }
}
