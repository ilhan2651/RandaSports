using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Sports.Command.SyncAthleteProfile;
using RandaSports.Application.Features.Sports.Command.SyncFixtures;
using RandaSports.Application.Features.Sports.Command.SyncLeagueReference;
using RandaSports.Application.Features.Sports.Command.SyncSquad;
using RandaSports.Application.Features.Sports.Command.SyncStandings;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Infrastructure.Sports;

namespace RandaSports.Worker.Workers;

/// <summary>
/// Spor verisini kotaya saygılı biçimde tazeler. Öncelik sırası kotanın azaldığı
/// yerde belli oluyor: önce lig ve maçlar, sonra puan durumu, en son profiller.
/// </summary>
public sealed class SportsSyncWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<SportsSyncOptions> options,
    IOptions<ApiSportsOptions> accountOptions,
    IOptions<ApiFootballOptions> providerOptions,
    TimeProvider timeProvider,
    ILogger<SportsSyncWorker> logger) : BackgroundService
{
    private readonly SportsSyncOptions _options = options.Value;
    private readonly ApiSportsOptions _account = accountOptions.Value;
    private readonly ApiFootballOptions _provider = providerOptions.Value;

    private DateTimeOffset? _lastReferenceSync;
    private DateTimeOffset? _lastFixtureSync;
    private DateTimeOffset? _lastStandingsSync;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled || !_account.HasKey)
        {
            logger.LogInformation("Spor verisi senkronu kapalı (ayar ya da API anahtarı yok).");
            return;
        }

        logger.LogInformation("Spor verisi senkronu başladı.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Spor verisi senkronunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();

        using var scope = scopeFactory.CreateScope();
        var quota = scope.ServiceProvider.GetRequiredService<IProviderQuota>();

        var remaining = await quota.RemainingAsync(ApiFootballClient.ProviderName, cancellationToken);

        // Canlı skora ayrılan payı yemeyelim.
        if (remaining <= _options.ReservedRequests)
        {
            logger.LogInformation("Kota az kaldı ({Remaining}), senkron turu atlandı.", remaining);
            return;
        }

        await SyncReferenceAsync(scope, now, cancellationToken);
        await SyncFixturesAsync(scope, now, cancellationToken);
        await SyncStandingsAsync(scope, now, cancellationToken);
        await SyncSquadsAsync(scope, now, cancellationToken);
        await SyncAthletesAsync(scope, now, quota, cancellationToken);
    }

    private async Task SyncReferenceAsync(IServiceScope scope, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!IsDue(_lastReferenceSync, now, TimeSpan.FromHours(_options.ReferenceSyncHours)))
            return;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new SyncLeagueReferenceCommand(_provider.LeagueId, _provider.Season), cancellationToken);

        if (result.IsFail)
        {
            logger.LogWarning("Lig senkronu başarısız: {Message}", result.Message);
            return;
        }

        _lastReferenceSync = now;
    }

    private async Task SyncFixturesAsync(IServiceScope scope, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!IsDue(_lastFixtureSync, now, TimeSpan.FromHours(_options.FixtureSyncHours)))
            return;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new SyncFixturesCommand(
                _provider.LeagueId,
                _provider.Season,
                _options.FixtureDaysBack,
                _options.FixtureDaysAhead),
            cancellationToken);

        if (result.IsFail)
        {
            logger.LogWarning("Fikstür senkronu başarısız: {Message}", result.Message);
            return;
        }

        _lastFixtureSync = now;
    }

    private async Task SyncStandingsAsync(IServiceScope scope, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!IsDue(_lastStandingsSync, now, TimeSpan.FromHours(_options.StandingsSyncHours)))
            return;

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.Send(
            new SyncStandingsCommand(_provider.LeagueId, _provider.Season), cancellationToken);

        if (result.IsFail)
        {
            logger.LogWarning("Puan durumu senkronu başarısız: {Message}", result.Message);
            return;
        }

        _lastStandingsSync = now;
    }

    private async Task SyncSquadsAsync(IServiceScope scope, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var sportsRepository = scope.ServiceProvider.GetRequiredService<ISportsRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var teamIds = await sportsRepository.GetTeamIdsForSquadSyncAsync(
            _provider.LeagueId,
            _options.TeamsPerTick,
            now.AddDays(-_options.SquadSyncDays),
            cancellationToken);

        foreach (var teamId in teamIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await mediator.Send(new SyncSquadCommand(teamId), cancellationToken);

            if (result.IsFail)
                logger.LogWarning("Kadro çekilemedi ({TeamId}): {Message}", teamId, result.Message);
        }
    }

    private async Task SyncAthletesAsync(
        IServiceScope scope,
        DateTimeOffset now,
        IProviderQuota quota,
        CancellationToken cancellationToken)
    {
        var sportsRepository = scope.ServiceProvider.GetRequiredService<ISportsRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var athleteIds = await sportsRepository.GetAthleteIdsForStatsSyncAsync(
            _options.AthletesPerTick,
            now.AddDays(-_options.StatsSyncDays),
            cancellationToken);

        foreach (var athleteId in athleteIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var remaining = await quota.RemainingAsync(ApiFootballClient.ProviderName, cancellationToken);
            if (remaining <= _options.ReservedRequests)
            {
                logger.LogInformation("Kota sınırına gelindi, oyuncu senkronu durduruldu.");
                return;
            }

            var result = await mediator.Send(
                new SyncAthleteProfileCommand(athleteId, _provider.Season), cancellationToken);

            if (result.IsFail)
                logger.LogWarning("Oyuncu çekilemedi ({AthleteId}): {Message}", athleteId, result.Message);
        }
    }

    private static bool IsDue(DateTimeOffset? last, DateTimeOffset now, TimeSpan period) =>
        last is null || now - last >= period;
}
