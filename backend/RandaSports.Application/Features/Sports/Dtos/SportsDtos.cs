namespace RandaSports.Application.Features.Sports.Dtos;

public sealed record SportDto(
    string Name,
    string Slug,
    int DisplayOrder,
    int StoryCount,
    bool HasCompetitions);

public sealed record TeamSummaryDto(
    string Name,
    string Slug,
    string? LogoUrl);

public sealed record FixtureSideDto(
    string Name,
    string Slug,
    string? LogoUrl,
    int? Score);

public sealed record FixtureDto(
    Guid Id,
    string Slug,
    DateTimeOffset StartTime,
    string Status,
    string? StatusDetail,
    string? Round,
    string? Venue,
    FixtureSideDto Home,
    FixtureSideDto Away);

public sealed record StandingRowDto(
    int Rank,
    TeamSummaryDto Team,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int Points,
    int GoalsFor,
    int GoalsAgainst,
    int GoalDifference,
    string? Form);

public sealed record StandingsDto(
    string CompetitionName,
    string CompetitionSlug,
    string? SeasonName,
    IReadOnlyList<StandingRowDto> Rows);

public sealed record AthleteListItemDto(
    string FullName,
    string Slug,
    string? PhotoUrl,
    string? Position,
    int? ShirtNumber,
    int? Goals,
    int? Assists);

public sealed record TeamDetailDto(
    Guid Id,
    string Name,
    string Slug,
    string? LogoUrl,
    string? Country,
    StandingRowDto? Standing,
    IReadOnlyList<AthleteListItemDto> Squad,
    IReadOnlyList<FixtureDto> RecentFixtures,
    IReadOnlyList<FixtureDto> UpcomingFixtures);

/// <summary>Oyuncu kartındaki turnuva kırılımı: kulüp ligi, Avrupa, millî takım ayrı.</summary>
public sealed record AthleteCompetitionDto(
    string? LeagueName,
    string? TeamName,
    bool IsNationalTeam,
    int Appearances,
    int Minutes,
    int Goals,
    int Assists,
    int ShotsTotal,
    int ShotsOn,
    int PassesTotal,
    int PassesKey,
    int? PassAccuracy,
    int DuelsTotal,
    int DuelsWon,
    int DribblesAttempts,
    int DribblesSuccess,
    int YellowCards,
    int RedCards,
    double? Rating);

public sealed record AthleteDetailDto(
    Guid Id,
    string FullName,
    string Slug,
    string? PhotoUrl,
    string? Position,
    int? ShirtNumber,
    string? Nationality,
    DateOnly? BirthDate,
    int? Age,
    string? BirthPlace,
    int? HeightCm,
    int? WeightKg,
    TeamSummaryDto? Team,
    int? Appearances,
    int? Goals,
    int? Assists,
    int? MinutesPlayed,
    double? Rating,
    int? StatsSeason,
    IReadOnlyList<AthleteCompetitionDto> Competitions);
