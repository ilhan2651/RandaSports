using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandaSports.Domain.Entities;

namespace RandaSports.Persistence.Configurations;

public class OpinionConfiguration : IEntityTypeConfiguration<Opinion>
{
    public void Configure(EntityTypeBuilder<Opinion> builder)
    {
        builder.HasOne(x => x.Sport)
            .WithMany()
            .HasForeignKey(x => x.SportId)
            .OnDelete(DeleteBehavior.SetNull);

        // Branş filtresi ve facet bu sütuna göre tarıyor.
        builder.HasIndex(x => x.SportId);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_opinions_attribution_confidence",
            "attribution_confidence IS NULL OR (attribution_confidence >= 0 AND attribution_confidence <= 1)"));

        builder.Property(x => x.SpeakerLabel).HasMaxLength(100);
        builder.Property(x => x.SpeakerSource).HasMaxLength(20);
        builder.Property(x => x.Topic).HasMaxLength(200);
        builder.Property(x => x.Summary).HasMaxLength(2000);
        builder.Property(x => x.Quote).HasMaxLength(2000);
        builder.Property(x => x.Prediction).HasMaxLength(300);
        builder.Property(x => x.ReviewNote).HasMaxLength(1000);
        builder.Property(x => x.QuoteCheckHeard).HasMaxLength(2000);
        builder.Property(x => x.QuoteCheckSpeaker).HasMaxLength(100);

        // Doğrulama turu "bekleyen ve henüz bakılmamış" kaydı bu indeksle buluyor.
        builder.HasIndex(x => new { x.Status, x.QuoteCheck });

        builder.HasOne(x => x.Video)
            .WithMany(x => x.Opinions)
            .HasForeignKey(x => x.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Commentator)
            .WithMany(x => x.Opinions)
            .HasForeignKey(x => x.CommentatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Event)
            .WithMany()
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Athlete)
            .WithMany()
            .HasForeignKey(x => x.AthleteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Story)
            .WithMany()
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.CommentatorId, x.Status });
        builder.HasIndex(x => new { x.EventId, x.Status });
        builder.HasIndex(x => new { x.TeamId, x.Status });
        builder.HasIndex(x => new { x.AthleteId, x.Status });
        builder.HasIndex(x => new { x.StoryId, x.Status });
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
