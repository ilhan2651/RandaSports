using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Standing : BaseEntity
{
    public Guid SeasonId { get; set; }
    public Season Season { get; set; } = null!;

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? AthleteId { get; set; }
    public Athlete? Athlete { get; set; }

    public string? GroupName { get; set; }
    public int Rank { get; set; }
    public int Played { get; set; }
    public int Won { get; set; }
    public int Drawn { get; set; }
    public int Lost { get; set; }
    public int? Points { get; set; }
    public string? Form { get; set; }
    public string? StatsJson { get; set; }
}
