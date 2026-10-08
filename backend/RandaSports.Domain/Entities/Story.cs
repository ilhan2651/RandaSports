using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Story : BaseEntity
{
    public Guid? SportId { get; set; }
    public Sport? Sport { get; set; }

    public required string ClusterKey { get; set; }
    public string? Slug { get; set; }
    public string? Category { get; set; }

    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? Body { get; set; }
    public string? Analysis { get; set; }
    public bool IsLive { get; set; }

    public string? ImageUrl { get; set; }
    public int ImagePriority { get; set; } = int.MinValue;

    public DateTimeOffset FirstPublishedAt { get; set; }
    public DateTimeOffset LastPublishedAt { get; set; }
    public int SourceCount { get; set; }

    public bool NeedsRewrite { get; set; } = true;
    public DateTimeOffset? AiProcessedAt { get; set; }
    public int AiAttempts { get; set; }
    public string? AiDataJson { get; set; }

    /// <summary>
    /// Haberin konusu olan takımlar. Kümeleme sırasında zaten eşleştiriyoruz;
    /// burada saklamazsak takıma göre haber listelemek imkânsız oluyordu —
    /// takım adları yalnızca ClusterKey metninin içinde duruyordu.
    /// </summary>
    public ICollection<Team> Teams { get; set; } = [];

    public ICollection<Article> Articles { get; set; } = [];
}
