using MediatR;
using Microsoft.Extensions.Options;
using RandaSports.Application.Features.Commentary.Command.DiscoverChannelVideos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Worker.Workers;

/// <summary>Kayıtlı YouTube kanallarının beslemesini tarar, yeni videoları kuyruğa alır.</summary>
public sealed class VideoDiscoveryWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<VideoDiscoveryOptions> options,
    TimeProvider timeProvider,
    ILogger<VideoDiscoveryWorker> logger) : BackgroundService
{
    private readonly VideoDiscoveryOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Video keşfi kapalı.");
            return;
        }

        logger.LogInformation("Video keşfi başladı.");

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
                logger.LogError(ex, "Video keşfi turunda beklenmeyen hata.");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
                break;
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var cutoff = timeProvider.GetUtcNow().AddMinutes(-_options.RecheckMinutes);

        List<Guid> channelIds;
        List<string> channelNames;

        using (var scope = scopeFactory.CreateScope())
        {
            var channelRepository = scope.ServiceProvider.GetRequiredService<IChannelRepository>();

            var channels = await channelRepository.GetDueForCheckAsync(
                _options.BatchSize,
                cutoff,
                cancellationToken);

            channelIds = channels.Select(x => x.Id).ToList();
            channelNames = channels.Select(x => x.Name).ToList();
        }

        if (channelIds.Count == 0)
            return;

        // Tur sessiz çalışıyordu: kanaldan yeni video çıkmayınca hiçbir şey
        // yazılmadığı için "taradı mı taramadı mı" logdan anlaşılmıyordu.
        logger.LogInformation("{Count} kanal taranacak: {Channels}", channelIds.Count, string.Join(", ", channelNames));

        foreach (var channelId in channelIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var result = await mediator.Send(
                    new DiscoverChannelVideosCommand(
                        channelId,
                        _options.MaxAgeDays,
                        _options.MaxPerChannel,
                        _options.MinDurationSeconds,
                        _options.MaxDurationSeconds,
                        _options.SkipLive),
                    cancellationToken);

                if (result.IsFail)
                    logger.LogWarning("Kanal taranamadı ({ChannelId}): {Message}", channelId, result.Message);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kanal taranırken hata ({ChannelId}).", channelId);
            }

            if (_options.DelaySeconds > 0)
                await Task.Delay(TimeSpan.FromSeconds(_options.DelaySeconds), cancellationToken);
        }
    }
}
