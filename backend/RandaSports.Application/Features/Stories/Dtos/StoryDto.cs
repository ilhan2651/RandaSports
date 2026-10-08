using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Stories.Dtos;

public sealed record StoryListItemDto(
    Guid Id,
    string? Slug,
    string? Headline,
    string? Summary,
    string? ImageUrl,
    string? Category,
    string? SportSlug,
    bool IsLive,
    int SourceCount,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt);

public sealed record StoryDetailDto(
    Guid Id,
    string? Slug,
    string? Headline,
    string? Summary,
    string? Body,
    string? Analysis,
    string? ImageUrl,
    string? Category,
    string? SportSlug,
    bool IsLive,
    int SourceCount,
    DateTimeOffset PublishedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<StorySourceDto> Sources);

/// <summary>Atıf için: haberin hangi kaynaktan geldiği ve orijinal bağlantısı.</summary>
public sealed record StorySourceDto(
    string SourceName,
    string Title,
    string Url,
    DateTimeOffset PublishedAt);

public static class StoryMappings
{
    public static StoryListItemDto ToListItem(this Story story) =>
        new(
            story.Id,
            story.Slug,
            story.Headline,
            story.Summary,
            story.ImageUrl,
            story.Category,
            story.Sport?.Slug,
            story.IsLive,
            story.SourceCount,
            story.FirstPublishedAt,
            story.LastPublishedAt);

    public static StoryDetailDto ToDetail(this Story story) =>
        new(
            story.Id,
            story.Slug,
            story.Headline,
            story.Summary,
            story.Body,
            story.Analysis,
            story.ImageUrl,
            story.Category,
            story.Sport?.Slug,
            story.IsLive,
            story.SourceCount,
            story.FirstPublishedAt,
            story.LastPublishedAt,
            story.Articles
                .OrderBy(x => x.PublishedAt)
                .Select(x => new StorySourceDto(
                    x.Source.Name,
                    x.Title,
                    x.Url,
                    x.PublishedAt))
                .ToList());
}
