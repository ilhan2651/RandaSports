using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Season : BaseEntity
{
    public Guid CompetitionId { get; set; }
    public Competition Competition { get; set; } = null!;

    public required string Name { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? ExternalId { get; set; }

    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Standing> Standings { get; set; } = [];
}
