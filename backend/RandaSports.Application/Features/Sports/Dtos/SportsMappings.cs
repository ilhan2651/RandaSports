using System.Text.Json;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sports.Dtos;

public static class SportsMappings
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static TeamSummaryDto ToSummary(this Team team) =>
        new(team.Name, team.Slug, team.LogoUrl);

    public static FixtureDto ToDto(this Event sportsEvent)
    {
        var home = sportsEvent.Participants.FirstOrDefault(x => x.Order == 0);
        var away = sportsEvent.Participants.FirstOrDefault(x => x.Order == 1);

        return new FixtureDto(
            sportsEvent.Id,
            sportsEvent.Slug,
            sportsEvent.StartTime,
            sportsEvent.Status.ToString(),
            sportsEvent.StatusDetail,
            sportsEvent.Round,
            sportsEvent.Venue,
            ToSide(home),
            ToSide(away));
    }

    private static FixtureSideDto ToSide(EventParticipant? participant) =>
        participant?.Team is null
            ? new FixtureSideDto("-", "-", null, participant?.Score)
            : new FixtureSideDto(
                participant.Team.Name,
                participant.Team.Slug,
                participant.Team.LogoUrl,
                participant.Score);

    public static StandingRowDto ToDto(this Standing standing)
    {
        var goals = ReadGoals(standing.StatsJson);

        return new StandingRowDto(
            standing.Rank,
            standing.Team?.ToSummary() ?? new TeamSummaryDto("-", "-", null),
            standing.Played,
            standing.Won,
            standing.Drawn,
            standing.Lost,
            standing.Points ?? 0,
            goals.For,
            goals.Against,
            goals.For - goals.Against,
            standing.Form);
    }

    public static AthleteListItemDto ToListItem(this Athlete athlete) =>
        new(
            athlete.FullName,
            athlete.Slug,
            athlete.PhotoUrl,
            athlete.Position,
            athlete.ShirtNumber,
            athlete.Goals,
            athlete.Assists);

    public static AthleteDetailDto ToDetail(this Athlete athlete) =>
        new(
            athlete.Id,
            athlete.FullName,
            athlete.Slug,
            athlete.PhotoUrl,
            athlete.Position,
            athlete.ShirtNumber,
            athlete.Nationality,
            athlete.BirthDate,
            CalculateAge(athlete.BirthDate),
            athlete.BirthPlace,
            athlete.HeightCm,
            athlete.WeightKg,
            athlete.Team?.ToSummary(),
            athlete.Appearances,
            athlete.Goals,
            athlete.Assists,
            athlete.MinutesPlayed,
            athlete.Rating,
            athlete.StatsSeason,
            ReadCompetitions(athlete.StatsJson));

    private static int? CalculateAge(DateOnly? birthDate)
    {
        if (birthDate is null)
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Value.Year;

        if (birthDate.Value > today.AddYears(-age))
            age--;

        return age;
    }

    private static (int For, int Against) ReadGoals(string? statsJson)
    {
        if (string.IsNullOrWhiteSpace(statsJson))
            return (0, 0);

        try
        {
            using var document = JsonDocument.Parse(statsJson);
            var root = document.RootElement;

            var forGoals = root.TryGetProperty("goalsFor", out var f) && f.TryGetInt32(out var fv) ? fv : 0;
            var against = root.TryGetProperty("goalsAgainst", out var a) && a.TryGetInt32(out var av) ? av : 0;

            return (forGoals, against);
        }
        catch (JsonException)
        {
            return (0, 0);
        }
    }

    /// <summary>Senkron sırasında yazılan turnuva kırılımını okur; bozuksa boş liste döner.</summary>
    private static List<AthleteCompetitionDto> ReadCompetitions(string? statsJson)
    {
        if (string.IsNullOrWhiteSpace(statsJson))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<AthleteCompetitionDto>>(statsJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
