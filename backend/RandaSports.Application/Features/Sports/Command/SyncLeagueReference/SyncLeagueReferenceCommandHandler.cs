using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sports.Command.SyncLeagueReference;

public sealed class SyncLeagueReferenceCommandHandler(
    ISportsDataProvider provider,
    ISportsRepository sportsRepository,
    IUnitOfWork unitOfWork,
    ILogger<SyncLeagueReferenceCommandHandler> logger)
    : IRequestHandler<SyncLeagueReferenceCommand, Result<LeagueSyncResult>>
{
    private const string FootballSlug = "futbol";

    public async Task<Result<LeagueSyncResult>> Handle(
        SyncLeagueReferenceCommand request,
        CancellationToken cancellationToken)
    {
        var sportId = await sportsRepository.GetSportIdBySlugAsync(FootballSlug, cancellationToken);
        if (sportId is null)
            return Result<LeagueSyncResult>.Fail("Futbol branşı bulunamadı.", HttpStatusCode.NotFound);

        var league = await provider.GetLeagueAsync(request.LeagueExternalId, cancellationToken);
        if (league is null)
            return Result<LeagueSyncResult>.Fail("Lig bilgisi alınamadı.", HttpStatusCode.BadGateway);

        logger.LogInformation("Lig doğrulandı: {League} ({Country})", league.Name, league.Country);

        var competition = await UpsertCompetitionAsync(sportId.Value, league, cancellationToken);
        await UpsertSeasonAsync(competition, request.Season, cancellationToken);

        var providerTeams = await provider.GetTeamsAsync(
            request.LeagueExternalId, request.Season, cancellationToken);

        if (providerTeams.Count == 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<LeagueSyncResult>.Ok(new LeagueSyncResult(league.Name, 0, 0));
        }

        var existing = await sportsRepository.GetTeamsBySportAsync(sportId.Value, cancellationToken);
        var lookup = BuildLookup(existing);

        var matched = 0;
        var created = 0;

        foreach (var providerTeam in providerTeams)
        {
            var slug = TextNormalizer.Slugify(providerTeam.Name, 150);
            if (string.IsNullOrEmpty(slug))
                continue;

            if (lookup.TryGetValue(slug, out var team))
            {
                matched++;
            }
            else
            {
                team = new Team
                {
                    SportId = sportId.Value,
                    Name = providerTeam.Name,
                    Slug = slug,
                    IsNational = providerTeam.IsNational
                };

                await sportsRepository.AddTeamAsync(team, cancellationToken);
                lookup[slug] = team;
                existing.Add(team);
                created++;

                logger.LogInformation("Sözlükte yoktu, eklendi: {Team}", providerTeam.Name);
            }

            team.ExternalId = providerTeam.ExternalId;
            team.LogoUrl ??= providerTeam.LogoUrl;
            team.Country ??= providerTeam.Country;

            if (competition.Teams.All(x => x.Slug != team.Slug))
                competition.Teams.Add(team);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "{League}: {Matched} takım eşleşti, {Created} takım eklendi.",
            league.Name, matched, created);

        return Result<LeagueSyncResult>.Ok(new LeagueSyncResult(league.Name, matched, created));
    }

    private async Task<Competition> UpsertCompetitionAsync(
        Guid sportId,
        ProviderLeague league,
        CancellationToken cancellationToken)
    {
        var competition = await sportsRepository.GetCompetitionByExternalIdAsync(
            league.ExternalId, cancellationToken);

        if (competition is not null)
        {
            competition.LogoUrl ??= league.LogoUrl;
            return competition;
        }

        competition = new Competition
        {
            SportId = sportId,
            Name = league.Name,
            Slug = TextNormalizer.Slugify(league.Name, 150),
            Country = league.Country,
            LogoUrl = league.LogoUrl,
            ExternalId = league.ExternalId
        };

        await sportsRepository.AddCompetitionAsync(competition, cancellationToken);
        return competition;
    }

    private async Task UpsertSeasonAsync(Competition competition, int year, CancellationToken cancellationToken)
    {
        var externalId = year.ToString();

        var season = await sportsRepository.GetSeasonAsync(competition.Id, externalId, cancellationToken);
        if (season is not null)
            return;

        // Aynı ligde tek bir sezon "güncel" olabilir; eskisini indiriyoruz.
        var current = await sportsRepository.GetCurrentSeasonAsync(competition.Id, cancellationToken);
        if (current is not null)
            current.IsCurrent = false;

        await sportsRepository.AddSeasonAsync(
            new Season
            {
                CompetitionId = competition.Id,
                Competition = competition,
                Name = $"{year}/{(year + 1) % 100:D2}",
                ExternalId = externalId,
                IsCurrent = true
            },
            cancellationToken);
    }

    /// <summary>Takım adları ve takma adları slug'a çevrilip tek sözlükte toplanıyor.</summary>
    private static Dictionary<string, Team> BuildLookup(List<Team> teams)
    {
        var lookup = new Dictionary<string, Team>(StringComparer.Ordinal);

        foreach (var team in teams)
        {
            lookup.TryAdd(team.Slug, team);

            foreach (var alias in team.Aliases)
            {
                var aliasSlug = TextNormalizer.Slugify(alias, 150);
                if (aliasSlug.Length > 2)
                    lookup.TryAdd(aliasSlug, team);
            }
        }

        return lookup;
    }
}
