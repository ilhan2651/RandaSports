using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class SportsReadRepository(RandaSportsDbContext context) : ISportsReadRepository
{
    public async Task<List<SportSummary>> GetSportsAsync(CancellationToken cancellationToken = default)
    {
        // Sayım yayına çıkmış haberler üzerinden: yazılmamış ya da adressiz konular
        // sitede görünmüyor, menüde de sayılmamalı.
        var rows = await context.Sports
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Select(x => new
            {
                x.Name,
                x.Slug,
                x.DisplayOrder,
                StoryCount = context.Stories.Count(s =>
                    s.SportId == x.Id && s.AiProcessedAt != null && s.Slug != null),
                HasCompetitions = context.Competitions.Any(c => c.SportId == x.Id && c.IsActive)
            })
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new SportSummary(x.Name, x.Slug, x.DisplayOrder, x.StoryCount, x.HasCompetitions))
            .ToList();
    }

    public Task<Competition?> GetCompetitionAsync(
        string? competitionSlug,
        string sportSlug,
        CancellationToken cancellationToken = default)
    {
        var query = context.Competitions
            .AsNoTracking()
            .Where(x => x.Sport.Slug == sportSlug && x.IsActive);

        if (!string.IsNullOrWhiteSpace(competitionSlug))
            query = query.Where(x => x.Slug == competitionSlug);

        return query.OrderBy(x => x.Name).FirstOrDefaultAsync(cancellationToken);
    }

    public Task<Season?> GetCurrentSeasonAsync(Guid competitionId, CancellationToken cancellationToken = default) =>
        context.Seasons
            .AsNoTracking()
            .Where(x => x.CompetitionId == competitionId)
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<Standing>> GetStandingsAsync(Guid seasonId, CancellationToken cancellationToken = default) =>
        context.Standings
            .AsNoTracking()
            .Include(x => x.Team)
            .Where(x => x.SeasonId == seasonId)
            .OrderBy(x => x.GroupName)
            .ThenBy(x => x.Rank)
            .ToListAsync(cancellationToken);

    public Task<List<Event>> GetFixturesAsync(
        string sportSlug,
        DateTimeOffset from,
        DateTimeOffset to,
        string? teamSlug = null,
        CancellationToken cancellationToken = default)
    {
        var query = FixtureQuery()
            .Where(x => x.Season.Competition.Sport.Slug == sportSlug
                        && x.StartTime >= from
                        && x.StartTime <= to);

        if (!string.IsNullOrWhiteSpace(teamSlug))
            query = query.Where(x => x.Participants.Any(p => p.Team != null && p.Team.Slug == teamSlug));

        return query.OrderBy(x => x.StartTime).ToListAsync(cancellationToken);
    }

    public Task<List<Event>> GetTeamFixturesAsync(
        Guid teamId,
        bool upcoming,
        int take,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var query = FixtureQuery()
            .Where(x => x.Participants.Any(p => p.TeamId == teamId));

        query = upcoming
            ? query.Where(x => x.StartTime >= now).OrderBy(x => x.StartTime)
            : query.Where(x => x.StartTime < now).OrderByDescending(x => x.StartTime);

        return query.Take(take).ToListAsync(cancellationToken);
    }

    public Task<Team?> GetTeamBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        context.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<Standing?> GetTeamStandingAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        context.Standings
            .AsNoTracking()
            .Include(x => x.Team)
            .Where(x => x.TeamId == teamId)
            .OrderByDescending(x => x.Season.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<Athlete>> GetSquadAsync(Guid teamId, CancellationToken cancellationToken = default) =>
        context.Athletes
            .AsNoTracking()
            .Where(x => x.TeamId == teamId)
            .OrderBy(x => x.ShirtNumber == null)
            .ThenBy(x => x.ShirtNumber)
            .ThenBy(x => x.FullName)
            .ToListAsync(cancellationToken);

    public Task<List<Sport>> GetAllSportsAsync(CancellationToken cancellationToken = default) =>
        context.Sports.AsNoTracking().OrderBy(x => x.DisplayOrder).ToListAsync(cancellationToken);

    public Task<Sport?> GetSportBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default) =>
        context.Sports.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<List<Team>> GetTeamsWithoutLogoAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        context.Teams
            .Where(x => x.LogoUrl == null)
            // Milli takımlar önce: bayrakları neredeyse her zaman bulunuyor, yani
            // aynı turda en çok sonucu onlar veriyor.
            .OrderByDescending(x => x.IsNational)
            .ThenBy(x => x.Name)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<List<Team>> GetAllTeamsAsync(CancellationToken cancellationToken = default) =>
        context.Teams
            .OrderBy(x => x.LogoUrl == null ? 0 : 1)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<List<Team>> GetTeamsByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default) =>
        ids.Count == 0
            ? Task.FromResult(new List<Team>())
            : context.Teams.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

    public Task<List<Sport>> GetSportsByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default) =>
        ids.Count == 0
            ? Task.FromResult(new List<Sport>())
            : context.Sports.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

    public Task<Athlete?> GetAthleteBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        context.Athletes
            .AsNoTracking()
            .Include(x => x.Team)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    /// <summary>Maç sorguları hep aynı ilişkileri istiyor; tek yerde topluyoruz.</summary>
    private IQueryable<Event> FixtureQuery() =>
        context.Events
            .AsNoTracking()
            .Include(x => x.Participants)
                .ThenInclude(x => x.Team);
}
