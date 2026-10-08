using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Sports.Command.SyncFixtures;

public sealed class SyncFixturesCommandHandler(
    ISportsDataProvider provider,
    ISportsRepository sportsRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<SyncFixturesCommandHandler> logger)
    : IRequestHandler<SyncFixturesCommand, Result<FixtureSyncResult>>
{
    public async Task<Result<FixtureSyncResult>> Handle(
        SyncFixturesCommand request,
        CancellationToken cancellationToken)
    {
        var competition = await sportsRepository.GetCompetitionByExternalIdAsync(
            request.LeagueExternalId, cancellationToken);

        if (competition is null)
            return Result<FixtureSyncResult>.Fail("Lig kaydı yok, önce referans senkronu çalışmalı.", HttpStatusCode.NotFound);

        var season = await sportsRepository.GetSeasonAsync(
            competition.Id, request.Season.ToString(), cancellationToken);

        if (season is null)
            return Result<FixtureSyncResult>.Fail("Sezon kaydı bulunamadı.", HttpStatusCode.NotFound);

        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var fixtures = await provider.GetFixturesAsync(
            request.LeagueExternalId,
            request.Season,
            today.AddDays(-request.DaysBack),
            today.AddDays(request.DaysAhead),
            cancellationToken);

        if (fixtures.Count == 0)
            return Result<FixtureSyncResult>.Ok(new FixtureSyncResult(0, 0), "Maç verisi gelmedi.");

        var teamExternalIds = fixtures
            .SelectMany(x => new[] { x.HomeTeamExternalId, x.AwayTeamExternalId })
            .Distinct()
            .ToList();

        var teams = (await sportsRepository.GetTeamsByExternalIdsAsync(teamExternalIds, cancellationToken))
            .Where(x => x.ExternalId is not null)
            .ToDictionary(x => x.ExternalId!, StringComparer.Ordinal);

        var existing = (await sportsRepository.GetEventsByExternalIdsAsync(
                fixtures.Select(x => x.ExternalId).ToList(), cancellationToken))
            .Where(x => x.ExternalId is not null)
            .ToDictionary(x => x.ExternalId!, StringComparer.Ordinal);

        var added = 0;
        var updated = 0;

        foreach (var fixture in fixtures)
        {
            teams.TryGetValue(fixture.HomeTeamExternalId, out var home);
            teams.TryGetValue(fixture.AwayTeamExternalId, out var away);

            if (home is null || away is null)
            {
                logger.LogWarning(
                    "Maç atlandı, takım eşleşmedi: {Home} - {Away}",
                    fixture.HomeTeamName, fixture.AwayTeamName);
                continue;
            }

            if (existing.TryGetValue(fixture.ExternalId, out var sportsEvent))
            {
                Apply(sportsEvent, fixture);
                UpdateParticipants(sportsEvent, fixture);
                updated++;
                continue;
            }

            sportsEvent = new Event
            {
                SeasonId = season.Id,
                Season = season,
                Name = $"{home.Name} - {away.Name}",
                Slug = BuildSlug(home, away, fixture.StartTime),
                ExternalId = fixture.ExternalId
            };

            Apply(sportsEvent, fixture);

            sportsEvent.Participants.Add(new EventParticipant { TeamId = home.Id, Order = 0 });
            sportsEvent.Participants.Add(new EventParticipant { TeamId = away.Id, Order = 1 });
            UpdateParticipants(sportsEvent, fixture);

            await sportsRepository.AddEventAsync(sportsEvent, cancellationToken);
            added++;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fikstür: {Added} yeni maç, {Updated} güncelleme.", added, updated);

        return Result<FixtureSyncResult>.Ok(new FixtureSyncResult(added, updated));
    }

    private static void Apply(Event sportsEvent, ProviderFixture fixture)
    {
        sportsEvent.StartTime = fixture.StartTime;
        sportsEvent.Status = MapStatus(fixture.StatusCode);
        sportsEvent.StatusDetail = fixture.Elapsed is > 0
            ? $"{fixture.Elapsed}'"
            : fixture.StatusDetail;
        sportsEvent.Round = fixture.Round;
        sportsEvent.Venue = fixture.Venue;
    }

    private static void UpdateParticipants(Event sportsEvent, ProviderFixture fixture)
    {
        var home = sportsEvent.Participants.FirstOrDefault(x => x.Order == 0);
        var away = sportsEvent.Participants.FirstOrDefault(x => x.Order == 1);

        if (home is null || away is null)
            return;

        home.Score = fixture.HomeScore;
        away.Score = fixture.AwayScore;

        // Sonuç sadece maç bittiğinde yazılıyor; devam eden maçta "kazandı" demiyoruz.
        if (sportsEvent.Status != EventStatus.Finished
            || fixture.HomeScore is null
            || fixture.AwayScore is null)
        {
            home.Result = null;
            away.Result = null;
            return;
        }

        home.Result = fixture.HomeScore > fixture.AwayScore ? ParticipantResult.Win
            : fixture.HomeScore < fixture.AwayScore ? ParticipantResult.Loss
            : ParticipantResult.Draw;

        away.Result = home.Result switch
        {
            ParticipantResult.Win => ParticipantResult.Loss,
            ParticipantResult.Loss => ParticipantResult.Win,
            _ => ParticipantResult.Draw
        };
    }

    /// <summary>Sağlayıcının durum kodlarını kendi enum'umuza çeviriyoruz.</summary>
    private static EventStatus MapStatus(string code) => code switch
    {
        "TBD" or "NS" => EventStatus.Scheduled,
        "1H" or "HT" or "2H" or "ET" or "BT" or "P" or "INT" or "LIVE" => EventStatus.Live,
        "FT" or "AET" or "PEN" => EventStatus.Finished,
        "PST" => EventStatus.Postponed,
        "SUSP" or "CANC" or "ABD" or "AWD" or "WO" => EventStatus.Cancelled,
        _ => EventStatus.Scheduled
    };

    private static string BuildSlug(Team home, Team away, DateTimeOffset startTime) =>
        $"{home.Slug}-{away.Slug}-{startTime:yyyy-MM-dd}";
}
