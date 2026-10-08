using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Team : BaseEntity
{
    public Guid SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    public required string Name { get; set; }
    public string? ShortName { get; set; }
    public required string Slug { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsNational { get; set; }
    public string? ExternalId { get; set; }

    /// <summary>Haber metninde geçebilecek diğer adlar: "Cimbom", "G.Saray", "Trabzon".</summary>
    public List<string> Aliases { get; set; } = [];

    /// <summary>Kadro en son ne zaman sağlayıcıdan tazelendi. Kota sırası buna göre.</summary>
    public DateTimeOffset? SquadSyncedAt { get; set; }

    public ICollection<Competition> Competitions { get; set; } = [];
    public ICollection<Athlete> Athletes { get; set; } = [];
}
