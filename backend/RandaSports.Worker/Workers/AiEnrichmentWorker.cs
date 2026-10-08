using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Articles.Command.EnrichArticle;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

public sealed class AiEnrichmentWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AiEnrichmentOptions> options,
    ILogger<AiEnrichmentWorker> logger) : BackgroundService
{
    private readonly AiEnrichmentOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("AI zenginleştirme kapalı.");
            return;
        }

        logger.LogInformation("AI zenginleştirme başladı.");

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
                logger.LogError(ex, "AI zenginleştirme turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<Guid> articleIds;

        using (var scope = scopeFactory.CreateScope())
        {
            var articleRepository = scope.ServiceProvider.GetRequiredService<IArticleRepository>();

            articleIds = await articleRepository.GetPendingAiArticleIdsAsync(
                _options.BatchSize, _options.MaxAttempts, cancellationToken);
        }

        if (articleIds.Count == 0)
            return;

        logger.LogInformation("{Count} haber AI'dan geçecek.", articleIds.Count);

        foreach (var articleId in articleIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new EnrichArticleCommand(articleId), cancellationToken);

                if (result.IsFail)
                    logger.LogWarning("Haber işlenemedi ({ArticleId}): {Message}", articleId, result.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Haber işlenirken hata ({ArticleId}).", articleId);
            }

            if (_options.DelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), cancellationToken);
        }
    }
}
