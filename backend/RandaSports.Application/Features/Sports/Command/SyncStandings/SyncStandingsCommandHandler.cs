using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Sports.Command.SyncStandings;

public sealed class SyncStandingsCommandHandler(
    ISportsDataProvider provider,
    ISportsRepository sportsRepository,
    IUnitOfWork unitOfWork,
    ILogger<SyncStandingsCommandHandler> logger)
    : IRequestHandler<SyncStandingsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(SyncStandingsCommand request, CancellationToken cancellationToken)
    {
        var competition = await sportsRepository.GetCompetitionByExternalIdAsync(
            request.LeagueExternalId, cancellationToken);

        if (competition is null)
            return Result<int>.Fail("Lig kaydı yok, önce referans senkronu çalışmalı.", HttpStatusCode.NotFound);

        var season = await sportsRepository.GetSeasonAsync(
            competition.Id, request.Season.ToString(), cancellationToken);

        if (season is null)
            return Result<int>.Fail("Sezon kaydı bulunamadı.", HttpStatusCode.NotFound);

        var rows = await provider.GetStandingsAsync(
            request.LeagueExternalId, request.Season, cancellationToken);

        if (rows.Count == 0)
            return Result<int>.Ok(0, "Puan durumu gelmedi.");

        var teams = (await sportsRepository.GetTeamsByExternalIdsAsync(
                rows.Select(x => x.TeamExternalId).ToList(), cancellationToken))
            .Where(x => x.ExternalId is not null)
            .ToDictionary(x => x.ExternalId!, StringComparer.Ordinal);

        var existing = (await sportsRepository.GetStandingsBySeasonAsync(season.Id, cancellationToken))
            .Where(x => x.TeamId is not null)
            .ToDictionary(x => x.TeamId!.Value);

        var written = 0;

        foreach (var row in rows)
        {
            if (!teams.TryGetValue(row.TeamExternalId, out var team))
            {
                logger.LogWarning("Puan durumu satırı atlandı, takım eşleşmedi: {Team}", row.TeamName);
                continue;
            }

            if (!existing.TryGetValue(team.Id, out var standing))
            {
                standing = new Standing
                {
                    SeasonId = season.Id,
                    Season = season,
                    TeamId = team.Id
                };

                await sportsRepository.AddStandingAsync(standing, cancellationToken);
            }

            standing.GroupName = row.GroupName;
            standing.Rank = row.Rank;
            standing.Played = row.Played;
            standing.Won = row.Won;
            standing.Drawn = row.Drawn;
            standing.Lost = row.Lost;
            standing.Points = row.Points;
            standing.Form = row.Form;
            standing.StatsJson = JsonSerializer.Serialize(new
            {
                goalsFor = row.GoalsFor,
                goalsAgainst = row.GoalsAgainst,
                goalDifference = row.GoalsFor - row.GoalsAgainst
            });

            written++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Puan durumu güncellendi: {Count} takım.", written);

        return Result<int>.Ok(written);
    }
}
