using RandaSports.Domain.Common;

namespace RandaSports.Domain.Entities;

public class Article : BaseEntity
{
    public Guid SourceId { get; set; }
    public Source Source { get; set; } = null!;

    public Guid? SportId { get; set; }
    public Sport? Sport { get; set; }

    public Guid? StoryId { get; set; }
    public Story? Story { get; set; }

    public required string Title { get; set; }
    public string? Excerpt { get; set; }
    public required string Url { get; set; }
    public string? ImageUrl { get; set; }
    public string? Author { get; set; }
    public DateTimeOffset PublishedAt { get; set; }

    public string? AiHeadline { get; set; }
    public string? AiSummary { get; set; }
    public string? AiBody { get; set; }
    public bool AiIsLive { get; set; }
    public string? AiAnalysis { get; set; }
    public DateTimeOffset? AiProcessedAt { get; set; }
    public int AiAttempts { get; set; }
    public string? AiDataJson { get; set; }

    public ICollection<ArticleTag> Tags { get; set; } = [];
}
