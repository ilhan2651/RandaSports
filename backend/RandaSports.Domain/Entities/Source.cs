using RandaSports.Domain.Common;
using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class Source : BaseEntity
{
    public required string Name { get; set; }
    public SourceType Type { get; set; }
    public required string Url { get; set; }
    public string Language { get; set; } = "tr";

    public Guid? SportId { get; set; }
    public Sport? Sport { get; set; }

    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }
    public int FetchIntervalMinutes { get; set; } = 10;
    public DateTimeOffset? LastFetchedAt { get; set; }
    public string? LastError { get; set; }

    public ICollection<Article> Articles { get; set; } = [];
}
