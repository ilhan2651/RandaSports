using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Sources.Dtos;

public sealed record SourceDto(
    Guid Id,
    string Name,
    SourceType Type,
    string Url,
    string Language,
    Guid? SportId,
    string? SportName,
    bool IsActive,
    int FetchIntervalMinutes,
    DateTimeOffset? LastFetchedAt,
    string? LastError,
    DateTimeOffset CreatedAt);

public static class SourceMappings
{
    public static SourceDto ToDto(this Source source) => new(
        source.Id,
        source.Name,
        source.Type,
        source.Url,
        source.Language,
        source.SportId,
        source.Sport?.Name,
        source.IsActive,
        source.FetchIntervalMinutes,
        source.LastFetchedAt,
        source.LastError,
        source.CreatedAt);
}
