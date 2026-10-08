using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Stories.Command.WriteStory;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

/// <summary>Kaynakları birikmiş konuları Gemini'ye yazdırır.</summary>
public sealed class StoryWriterWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<StoryWriterOptions> options,
    TimeProvider timeProvider,
    ILogger<StoryWriterWorker> logger) : BackgroundService
{
    private readonly StoryWriterOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Konu yazımı kapalı.");
            return;
        }

        logger.LogInformation("Konu yazımı başladı.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Konu yazımı turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().AddMinutes(-_options.SingleSourceWaitMinutes);

        List<Guid> storyIds;

        using (var scope = scopeFactory.CreateScope())
        {
            var storyRepository = scope.ServiceProvider.GetRequiredService<IStoryRepository>();

            storyIds = await storyRepository.GetPendingWriteIdsAsync(
                _options.BatchSize,
                _options.MaxAttempts,
                _options.MinSourceCount,
                cutoff,
                cancellationToken);
        }

        if (storyIds.Count == 0)
            return;

        logger.LogInformation("{Count} konu yazılacak.", storyIds.Count);

        foreach (var storyId in storyIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new WriteStoryCommand(storyId), cancellationToken);

                if (result.IsFail)
                    logger.LogWarning("Konu yazılamadı ({StoryId}): {Message}", storyId, result.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Konu yazılırken hata ({StoryId}).", storyId);
            }

            if (_options.DelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), cancellationToken);
        }
    }
}
