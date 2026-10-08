namespace RandaSports.Application.Interfaces.Services;

public sealed record YouTubeFeedItem(
    string VideoId,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    DateTimeOffset PublishedAt);

/// <summary>
/// Kanalın YouTube RSS beslemesini okur. Besleme ücretsiz ve anahtarsız;
/// son 15 videoyu veriyor.
/// </summary>
public interface IYouTubeFeedReader
{
    Task<IReadOnlyList<YouTubeFeedItem>> ReadChannelAsync(
        string youTubeChannelId,
        CancellationToken cancellationToken = default);
}
