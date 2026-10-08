using RandaSports.Domain.Common;
using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class EventParticipant : BaseEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? AthleteId { get; set; }
    public Athlete? Athlete { get; set; }

    public int Order { get; set; }
    public int? Score { get; set; }
    public string? ScoreDetail { get; set; }
    public ParticipantResult? Result { get; set; }
    public string? StatsJson { get; set; }
}
