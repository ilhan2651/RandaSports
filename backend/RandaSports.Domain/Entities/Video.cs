using RandaSports.Domain.Common;
using RandaSports.Domain.Enums;

namespace RandaSports.Domain.Entities;

public class Video : BaseEntity
{
    public Guid ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;

    public required string YouTubeVideoId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTimeOffset PublishedAt { get; set; }
    public int? DurationSeconds { get; set; }

    public VideoProcessingStatus ProcessingStatus { get; set; } = VideoProcessingStatus.Pending;
    public int ProcessingAttempts { get; set; }
    public string? ProcessingError { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? AiSummary { get; set; }

    public ICollection<Opinion> Opinions { get; set; } = [];
}
