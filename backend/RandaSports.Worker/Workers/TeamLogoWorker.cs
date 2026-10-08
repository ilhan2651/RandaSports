using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Sports.Command.FetchTeamLogos;

namespace RandaSports.Worker.Workers;

/// <summary>
/// Logosu olmayan takımlara logo arar. Kendi kendini tamamlayan bir iş: seed'e yeni
/// takım eklendiğinde bir sonraki turda sıraya giriyor, bulunamayanlar her turda
/// yeniden deneniyor (Wikidata'ya sonradan eklenmiş olabilir).
/// </summary>
public sealed class TeamLogoWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TeamLogoOptions> options,
    ILogger<TeamLogoWorker> logger) : BackgroundService
{
    private readonly TeamLogoOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Takım logosu araması kapalı.");
            return;
        }

        logger.LogInformation("Takım logosu araması başladı.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_options.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(
                    new FetchTeamLogosCommand(_options.BatchSize),
                    stoppingToken);

                if (result.IsFail)
                    logger.LogWarning("Logo araması başarısız: {Message}", result.Message);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Logo arama turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }
}
