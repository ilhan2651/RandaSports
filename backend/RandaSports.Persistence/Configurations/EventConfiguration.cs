using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Slug).HasMaxLength(200);
        builder.Property(x => x.StatusDetail).HasMaxLength(50);
        builder.Property(x => x.Round).HasMaxLength(100);
        builder.Property(x => x.Venue).HasMaxLength(200);
        builder.Property(x => x.ExternalId).HasMaxLength(50);
        builder.Property(x => x.StatsJson).HasColumnType("jsonb");

        builder.HasOne(x => x.Season)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.Status, x.StartTime });
        builder.HasIndex(x => new { x.SeasonId, x.StartTime });
        builder.HasIndex(x => x.ExternalId);
    }
}
