using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class SportsRepository(RandaSportsDbContext context) : ISportsRepository
{
    public async Task<Guid?> GetSportIdBySlugAsync(string sportSlug, CancellationToken cancellationToken = default)
    {
        var id = await context.Sports
            .AsNoTracking()
            .Where(x => x.Slug == sportSlug)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return id == Guid.Empty ? null : id;
    }

    public Task<Competition?> GetCompetitionByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default) =>
        context.Competitions
            .Include(x => x.Teams)
            .FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken);

    public Task<Season?> GetSeasonAsync(
        Guid competitionId,
        string externalId,
        CancellationToken cancellationToken = default) =>
        context.Seasons
            .FirstOrDefaultAsync(
                x => x.CompetitionId == competitionId && x.ExternalId == externalId,
                cancellationToken);

    public Task<Season?> GetCurrentSeasonAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default) =>
        context.Seasons
            .FirstOrDefaultAsync(x => x.CompetitionId == competitionId && x.IsCurrent, cancellationToken);

    public Task<List<Team>> GetTeamsBySportAsync(Guid sportId, CancellationToken cancellationToken = default) =>
        context.Teams
            .Where(x => x.SportId == sportId)
            .ToListAsync(cancellationToken);

    public Task<Team?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        context.Teams.FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);

    public Task<List<Guid>> GetTeamIdsForSquadSyncAsync(
        string competitionExternalId,
        int take,
        DateTimeOffset staleBefore,
        CancellationToken cancellationToken = default) =>
        context.Competitions
            .AsNoTracking()
            .Where(x => x.ExternalId == competitionExternalId)
            .SelectMany(x => x.Teams)
            .Where(x => x.ExternalId != null
                        && (x.SquadSyncedAt == null || x.SquadSyncedAt < staleBefore))
            .OrderBy(x => x.SquadSyncedAt)
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<Athlete?> GetAthleteAsync(Guid athleteId, CancellationToken cancellationToken = default) =>
        context.Athletes.FirstOrDefaultAsync(x => x.Id == athleteId, cancellationToken);

    public Task<List<Athlete>> GetAthletesByTeamAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        context.Athletes
            .Where(x => x.TeamId == teamId)
            .ToListAsync(cancellationToken);

    public Task<List<Athlete>> GetAthletesByExternalIdsAsync(
        IReadOnlyCollection<string> externalIds,
        CancellationToken cancellationToken = default) =>
        context.Athletes
            .Where(x => x.ExternalId != null && externalIds.Contains(x.ExternalId))
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> GetAthleteIdsForStatsSyncAsync(
        int take,
        DateTimeOffset staleBefore,
        CancellationToken cancellationToken = default) =>
        context.Athletes
            .AsNoTracking()
            .Where(x => x.ExternalId != null
                        && (x.StatsUpdatedAt == null || x.StatsUpdatedAt < staleBefore))
            .OrderBy(x => x.StatsUpdatedAt)
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

    public async Task<HashSet<string>> GetAthleteSlugsAsync(
        Guid sportId,
        CancellationToken cancellationToken = default)
    {
        var slugs = await context.Athletes
            .AsNoTracking()
            .Where(x => x.SportId == sportId)
            .Select(x => x.Slug)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(slugs, StringComparer.Ordinal);
    }

    public Task<List<Event>> GetEventsByExternalIdsAsync(
        IReadOnlyCollection<string> externalIds,
        CancellationToken cancellationToken = default) =>
        context.Events
            .Include(x => x.Participants)
            .Where(x => x.ExternalId != null && externalIds.Contains(x.ExternalId))
            .ToListAsync(cancellationToken);

    public Task<List<Standing>> GetStandingsBySeasonAsync(
        Guid seasonId,
        CancellationToken cancellationToken = default) =>
        context.Standings
            .Where(x => x.SeasonId == seasonId)
            .ToListAsync(cancellationToken);

    public async Task AddEventAsync(Event sportsEvent, CancellationToken cancellationToken = default) =>
        await context.Events.AddAsync(sportsEvent, cancellationToken);

    public async Task AddStandingAsync(Standing standing, CancellationToken cancellationToken = default) =>
        await context.Standings.AddAsync(standing, cancellationToken);

    public async Task AddCompetitionAsync(Competition competition, CancellationToken cancellationToken = default) =>
        await context.Competitions.AddAsync(competition, cancellationToken);

    public async Task AddSeasonAsync(Season season, CancellationToken cancellationToken = default) =>
        await context.Seasons.AddAsync(season, cancellationToken);

    public async Task AddTeamAsync(Team team, CancellationToken cancellationToken = default) =>
        await context.Teams.AddAsync(team, cancellationToken);

    public async Task AddAthleteAsync(Athlete athlete, CancellationToken cancellationToken = default) =>
        await context.Athletes.AddAsync(athlete, cancellationToken);

    public Task<List<Team>> GetTeamsByExternalIdsAsync(
        IReadOnlyCollection<string> externalIds,
        CancellationToken cancellationToken = default) =>
        context.Teams
            .Where(x => x.ExternalId != null && externalIds.Contains(x.ExternalId))
            .ToListAsync(cancellationToken);
}
