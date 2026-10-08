using RandaSports.Domain.Common;
using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class Opinion : BaseEntity
{
    public Guid VideoId { get; set; }
    public Video Video { get; set; } = null!;

    public Guid? CommentatorId { get; set; }
    public Commentator? Commentator { get; set; }
    public string? SpeakerLabel { get; set; }
    public double? AttributionConfidence { get; set; }

    /// <summary>
    /// Konuşmacının adı nereden geldi: altbant, baslik, hitap, aciklama, tahmin.
    /// Hangi kaynağa güveneceğimizi bu belirliyor.
    /// </summary>
    public string? SpeakerSource { get; set; }

    public Guid? EventId { get; set; }
    public Event? Event { get; set; }


    /// <summary>
    /// Görüşün branşı. Takımdan ya da kanaldan geliyor; çıkarım anında yazılıyor
    /// ki filtreleme tek alana baksın. Takımı olmayan branşlarda (MMA, F1, tenis)
    /// tek kaynak kanalın branşı.
    /// </summary>
    public Guid? SportId { get; set; }
    public Sport? Sport { get; set; }
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? AthleteId { get; set; }
    public Athlete? Athlete { get; set; }

    /// <summary>Görüşün bağlandığı haber konusu; haber sayfasında bu sayede görünüyor.</summary>
    public Guid? StoryId { get; set; }
    public Story? Story { get; set; }

    public required string Topic { get; set; }
    public required string Summary { get; set; }
    public required string Quote { get; set; }
    public int TimestampSeconds { get; set; }
    public Stance Stance { get; set; }
    public string? Prediction { get; set; }

    public bool IsQuoteVerified { get; set; }
    public OpinionStatus Status { get; set; } = OpinionStatus.Pending;
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNote { get; set; }
}
