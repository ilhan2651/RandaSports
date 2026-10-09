using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Commentary.Dtos;

public sealed record OpinionDto(
    Guid Id,
    string? CommentatorName,
    string? CommentatorSlug,
    string? CommentatorPhotoUrl,
    string? SpeakerLabel,
    string? SpeakerSource,
    double? AttributionConfidence,
    string Topic,
    string Summary,
    string Quote,
    int? TimestampSeconds,
    string Stance,
    string? Prediction,
    string Status,
    bool IsQuoteVerified,
    string YouTubeVideoId,
    string VideoTitle,
    string? VideoThumbnailUrl,
    string ChannelName,
    DateTimeOffset VideoPublishedAt,
    string? TeamName,
    string? TeamSlug,
    string? SportSlug,
    Guid? StoryId,
    string? StorySlug,
    string? StoryHeadline,

    /// <summary>Makinenin ön kontrolü; onay ekranı buna göre renkleniyor.</summary>
    string QuoteCheck,
    string? QuoteCheckHeard,
    string? QuoteCheckSpeaker,
    int? QuoteCheckTimestampSeconds);

public sealed record ChannelDto(
    Guid Id,
    string Name,
    string Slug,
    string Handle,
    string? YouTubeChannelId,
    bool IsActive,
    DateTimeOffset? LastCheckedAt,
    string? LastError,
    int CommentatorCount,
    List<string> SportSlugs,
    List<string> SportNames);

/// <summary>
/// Hat teşhisi: videonun boru hattında nerede durduğunu gösteriyor. Görüş gelmiyorsa
/// sebebi burada görünüyor — video hiç eklenmemiş mi, atlanmış mı, hata mı almış.
/// </summary>
public sealed record VideoStatusDto(
    Guid Id,
    string Title,
    string ChannelName,
    string YouTubeVideoId,
    DateTimeOffset PublishedAt,
    int? DurationSeconds,
    string Status,
    int Attempts,
    string? Error,
    DateTimeOffset? ProcessedAt,
    int OpinionCount);

public sealed record VideoStatusReportDto(
    Dictionary<string, int> Counts,
    List<VideoStatusDto> Videos);

public sealed record SportFacetDto(
    string Name,
    string Slug,
    int OpinionCount);

public sealed record TeamFacetDto(
    string Name,
    string Slug,
    string? SportSlug,
    string? LogoUrl,
    int OpinionCount);

/// <param name="ApprovedCount">Kaç görüşü otomatik yayına girmiş: reddedersen onlar geri çekilir.</param>
public sealed record UnverifiedCommentatorDto(
    Guid Id,
    string FullName,
    string Slug,
    string? PhotoUrl,
    int OpinionCount,
    int ApprovedCount,
    DateTimeOffset? LastOpinionAt,
    List<string> Channels,
    List<OpinionSampleDto> Sample);

public sealed record OpinionSampleDto(
    string Topic,
    string Quote,
    string VideoTitle,
    string YouTubeVideoId,
    int? TimestampSeconds);

public sealed record CommentatorDto(
    Guid Id,
    string FullName,
    string Slug,
    string? PhotoUrl,
    string? Bio,
    bool IsActive,
    int OpinionCount,
    DateTimeOffset? LastOpinionAt,
    string PersonRole,
    List<string> SportSlugs);

public static class CommentaryMappings
{
    public static OpinionDto ToDto(this Opinion opinion) =>
        new(
            opinion.Id,
            opinion.Commentator?.FullName,
            opinion.Commentator?.Slug,
            opinion.Commentator?.PhotoUrl,
            opinion.SpeakerLabel,
            opinion.SpeakerSource,
            opinion.AttributionConfidence,
            opinion.Topic,
            opinion.Summary,
            opinion.Quote,
            opinion.TimestampSeconds,
            opinion.Stance.ToString(),
            opinion.Prediction,
            opinion.Status.ToString(),
            opinion.IsQuoteVerified,
            opinion.Video.YouTubeVideoId,
            opinion.Video.Title,
            opinion.Video.ThumbnailUrl,
            opinion.Video.Channel.Name,
            opinion.Video.PublishedAt,
            opinion.Team?.Name,
            opinion.Team?.Slug,
            opinion.Sport?.Slug ?? opinion.Team?.Sport?.Slug,
            opinion.StoryId,
            opinion.Story?.Slug,
            opinion.Story?.Headline,
            opinion.QuoteCheck.ToString(),
            opinion.QuoteCheckHeard,
            opinion.QuoteCheckSpeaker,
            opinion.QuoteCheckTimestampSeconds);

    public static ChannelDto ToDto(this Channel channel) =>
        new(
            channel.Id,
            channel.Name,
            channel.Slug,
            channel.Handle,
            channel.YouTubeChannelId,
            channel.IsActive,
            channel.LastCheckedAt,
            channel.LastError,
            channel.RegularCommentators.Count,
            [.. channel.Sports.Select(x => x.Slug)],
            [.. channel.Sports.Select(x => x.Name)]);

    public static VideoStatusDto ToDto(this VideoStatusRow row) =>
        new(
            row.Id,
            row.Title,
            row.ChannelName,
            row.YouTubeVideoId,
            row.PublishedAt,
            row.DurationSeconds,
            row.Status,
            row.Attempts,
            row.Error,
            row.ProcessedAt,
            row.OpinionCount);

    public static SportFacetDto ToDto(this OpinionSportFacet facet) =>
        new(facet.Name, facet.Slug, facet.OpinionCount);

    public static TeamFacetDto ToDto(this OpinionTeamFacet facet) =>
        new(facet.Name, facet.Slug, facet.SportSlug, facet.LogoUrl, facet.OpinionCount);

    public static CommentatorDto ToDto(this CommentatorProfile profile) =>
        new(
            profile.Commentator.Id,
            profile.Commentator.FullName,
            profile.Commentator.Slug,
            profile.Commentator.PhotoUrl,
            profile.Commentator.Bio,
            profile.Commentator.IsActive,
            profile.OpinionCount,
            profile.LastOpinionAt,
            profile.Commentator.PersonRole.ToString(),
            profile.SportSlugs);
}
