using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Competition : BaseEntity
{
    public Guid SportId { get; set; }
    public Sport Sport { get; set; } = null!;

    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? ExternalId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Season> Seasons { get; set; } = [];
    public ICollection<Team> Teams { get; set; } = [];
}
