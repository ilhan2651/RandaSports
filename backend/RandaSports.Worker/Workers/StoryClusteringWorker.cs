using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Stories.Command.AssignArticleToStory;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

/// <summary>AI'dan geçmiş haberleri konularına bağlar. Model çağrısı yok, sadece veritabanı.</summary>
public sealed class StoryClusteringWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<StoryClusteringOptions> options,
    ILogger<StoryClusteringWorker> logger) : BackgroundService
{
    private readonly StoryClusteringOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Konu kümeleme kapalı.");
            return;
        }

        logger.LogInformation("Konu kümeleme başladı.");

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
                logger.LogError(ex, "Konu kümeleme turunda beklenmeyen hata.");
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
            articleIds = await articleRepository.GetUnclusteredArticleIdsAsync(_options.BatchSize, cancellationToken);
        }

        if (articleIds.Count == 0)
            return;

        logger.LogInformation("{Count} haber konulara bağlanacak.", articleIds.Count);

        foreach (var articleId in articleIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(new AssignArticleToStoryCommand(articleId), cancellationToken);

                if (result.IsFail)
                    logger.LogWarning("Haber kümelenemedi ({ArticleId}): {Message}", articleId, result.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Haber kümelenirken hata ({ArticleId}).", articleId);
            }
        }
    }
}
