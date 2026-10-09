using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Commentary.Command.VerifyOpinionQuote;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

/// <summary>
/// Onay sırasında bekleyen görüşlerin alıntılarını, damganın etrafındaki kısa
/// pencereyi modele tekrar izleterek önden kontrol eder.
///
/// Amacı görüşü onaylamak değil, onay ekranını hazırlamak: ekrana giren kişi her
/// kayıt için videoyu baştan açmak yerine hazır bir kontrol sonucuna bakıyor.
/// Son söz hâlâ insanda.
/// </summary>
public sealed class QuoteCheckWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<QuoteCheckOptions> options,
    ILogger<QuoteCheckWorker> logger) : BackgroundService
{
    private readonly QuoteCheckOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Alıntı kontrolü kapalı.");
            return;
        }

        logger.LogInformation("Alıntı kontrolü başladı.");

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
                logger.LogError(ex, "Alıntı kontrolü turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<Guid> opinionIds;

        using (var scope = scopeFactory.CreateScope())
        {
            var opinionRepository = scope.ServiceProvider.GetRequiredService<IOpinionRepository>();
            opinionIds = await opinionRepository.GetAwaitingQuoteCheckAsync(_options.BatchSize, cancellationToken);
        }

        if (opinionIds.Count == 0)
            return;

        logger.LogInformation("{Count} görüşün alıntısı kontrol edilecek.", opinionIds.Count);

        foreach (var opinionId in opinionIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(
                    new VerifyOpinionQuoteCommand(opinionId, _options.WindowSeconds),
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Alıntı kontrol edilirken hata ({OpinionId}).", opinionId);
            }

            if (_options.DelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), cancellationToken);
        }
    }
}
