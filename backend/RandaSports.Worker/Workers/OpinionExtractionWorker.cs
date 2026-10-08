using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

/// <summary>Kuyruktaki videoları Gemini'ye izletir, çıkan görüşleri onaya düşürür.</summary>
public sealed class OpinionExtractionWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<OpinionExtractionOptions> options,
    ILogger<OpinionExtractionWorker> logger) : BackgroundService
{
    private readonly OpinionExtractionOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Görüş çıkarma kapalı.");
            return;
        }

        logger.LogInformation("Görüş çıkarma başladı.");

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
                logger.LogError(ex, "Görüş çıkarma turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<Guid> videoIds;

        using (var scope = scopeFactory.CreateScope())
        {
            var videoRepository = scope.ServiceProvider.GetRequiredService<IVideoRepository>();

            videoIds = await videoRepository.GetPendingExtractionIdsAsync(
                _options.BatchSize,
                _options.MaxAttempts,
                _options.PreferShortVideos,
                cancellationToken);
        }

        if (videoIds.Count == 0)
            return;

        logger.LogInformation("{Count} video işlenecek.", videoIds.Count);

        foreach (var videoId in videoIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(
                    new ExtractVideoOpinionsCommand(videoId, _options.StoryMatchDays),
                    cancellationToken);

                if (result.IsFail)
                    logger.LogWarning("Video işlenemedi ({VideoId}): {Message}", videoId, result.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Video işlenirken hata ({VideoId}).", videoId);
            }

            if (_options.DelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), cancellationToken);
        }
    }
}
