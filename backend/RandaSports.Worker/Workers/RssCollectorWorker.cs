using MediatR;
using RandaSports.Application.Features.Articles.Command.CollectFromSource;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

public sealed class RssCollectorWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    ILogger<RssCollectorWorker> logger) : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RSS toplayıcı başladı.");

        using var timer = new PeriodicTimer(TickInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CollectDueSourcesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "RSS toplama turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task CollectDueSourcesAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        List<Guid> dueSourceIds;

        using (var scope = scopeFactory.CreateScope())
        {
            var sourceRepository = scope.ServiceProvider.GetRequiredService<ISourceRepository>();
            var sources = await sourceRepository.GetActiveRssSourcesAsync(cancellationToken);

            dueSourceIds = sources
                .Where(x => x.LastFetchedAt is null
                            || x.LastFetchedAt.Value.AddMinutes(x.FetchIntervalMinutes) <= now)
                .Select(x => x.Id)
                .ToList();
        }

        if (dueSourceIds.Count == 0)
            return;

        logger.LogInformation("{Count} kaynak toplanacak.", dueSourceIds.Count);

        foreach (var sourceId in dueSourceIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new CollectSourceCommand(sourceId), cancellationToken);

                if (result.IsFail)
                    logger.LogWarning("Kaynak toplanamadı ({SourceId}): {Message}", sourceId, result.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kaynak işlenirken hata ({SourceId}). Diğer kaynaklara devam ediliyor.", sourceId);
            }
        }
    }
}
