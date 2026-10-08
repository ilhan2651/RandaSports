namespace RandaSports.Application.Interfaces.Services;

public sealed record RssFeedItem(
    string Title,
    string Url,
    string? Summary,
    string? ImageUrl,
    string? Author,
    DateTimeOffset PublishedAt);

public interface IRssFeedReader
{
    Task<IReadOnlyList<RssFeedItem>> ReadAsync(string feedUrl, CancellationToken cancellationToken = default);
}
