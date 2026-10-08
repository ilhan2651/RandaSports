using RandaSports.Domain.Common;
using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class Event : BaseEntity
{
    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public string? Name { get; set; }
    public required string Slug { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
    public string? StatusDetail { get; set; }
    public string? Round { get; set; }
    public string? Venue { get; set; }
    public string? StatsJson { get; set; }
    public string? ExternalId { get; set; }

    public ICollection<EventParticipant> Participants { get; set; } = [];
}
