using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Athlete : BaseEntity
{
    public Guid SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public required string FullName { get; set; }
    public string? Nickname { get; set; }
    public required string Slug { get; set; }
    public string? Nationality { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? BirthPlace { get; set; }
    public string? Position { get; set; }
    public int? ShirtNumber { get; set; }
    public int? HeightCm { get; set; }
    public int? WeightKg { get; set; }
    public string? PhotoUrl { get; set; }
    public string? ExternalId { get; set; }

    /// <summary>Oyuncu kartındaki sayaçlar için özet sezon istatistikleri.</summary>
    public int? Appearances { get; set; }
    public int? Goals { get; set; }
    public int? Assists { get; set; }
    public int? MinutesPlayed { get; set; }
    public double? Rating { get; set; }

    /// <summary>Radar grafik ve detay kırılımları için sağlayıcının ham istatistik gövdesi.</summary>
    public string? StatsJson { get; set; }
    public int? StatsSeason { get; set; }
    public DateTimeOffset? StatsUpdatedAt { get; set; }
}
