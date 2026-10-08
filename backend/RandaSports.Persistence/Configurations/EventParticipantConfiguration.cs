using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
{
    public void Configure(EntityTypeBuilder<EventParticipant> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_event_participants_team_or_athlete",
            "team_id IS NOT NULL OR athlete_id IS NOT NULL"));

        builder.Property(x => x.ScoreDetail).HasMaxLength(100);
        builder.Property(x => x.StatsJson).HasColumnType("jsonb");

        builder.HasOne(x => x.Event)
            .WithMany(x => x.Participants)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Athlete)
            .WithMany()
            .HasForeignKey(x => x.AthleteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EventId, x.Order }).IsUnique();
    }
}
