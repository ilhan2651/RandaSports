using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Commentary.Command.DiscoverChannelVideos;

public sealed class DiscoverChannelVideosCommandHandler(
    IChannelRepository channelRepository,
    IVideoRepository videoRepository,
    IYouTubeChannelResolver channelResolver,
    IYouTubeFeedReader feedReader,
    IYouTubeVideoDurationReader durationReader,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<DiscoverChannelVideosCommandHandler> logger)
    : IRequestHandler<DiscoverChannelVideosCommand, Result<int>>
{
    private const int TitleMaxLength = 300;
    private const int DescriptionMaxLength = 4000;

    /// <summary>
    /// Süre elemesi bazı videoları düşüreceği için beslemeden hedefin birkaç katı
    /// aday alıyoruz; yoksa canlı yayına denk gelen tur elleri boş dönüyor.
    /// </summary>
    private const int CandidateMultiplier = 4;

    public async Task<Result<int>> Handle(DiscoverChannelVideosCommand request, CancellationToken cancellationToken)
    {
        var channel = await channelRepository.GetByIdAsync(request.ChannelId, cancellationToken);
        if (channel is null)
            return Result<int>.Fail("Kanal bulunamadı.", HttpStatusCode.NotFound);

        var now = timeProvider.GetUtcNow();
        channel.LastCheckedAt = now;

        // Kanal kimliği bir kez çözülüyor; besleme adresi kullanıcı adını kabul etmiyor.
        if (string.IsNullOrWhiteSpace(channel.YouTubeChannelId))
        {
            var resolved = await channelResolver.ResolveChannelIdAsync(channel.Handle, cancellationToken);

            if (string.IsNullOrWhiteSpace(resolved))
            {
                channel.LastError = $"Kanal kimliği çözülemedi: {channel.Handle}";
                await unitOfWork.SaveChangesAsync(cancellationToken);
                return Result<int>.Fail(channel.LastError, HttpStatusCode.BadGateway);
            }

            channel.YouTubeChannelId = resolved;
            logger.LogInformation("Kanal kimliği çözüldü: {Handle} → {ChannelId}", channel.Handle, resolved);
        }

        var items = await feedReader.ReadChannelAsync(channel.YouTubeChannelId, cancellationToken);

        if (items.Count == 0)
        {
            // Besleme boşsa kimlik büyük ihtimalle yanlış çözülmüştür (kanal sayfasında
            // başka bir kanalın kimliği geçebiliyor). Kimliği atıyoruz ki bir sonraki
            // taramada baştan çözülsün; yoksa kanal kalıcı olarak boş kalıyor.
            channel.YouTubeChannelId = null;
            channel.LastError = "Beslemede video bulunamadı; kanal kimliği yeniden çözülecek.";

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Ok(0, channel.LastError);
        }

        var cutoff = now.AddDays(-request.MaxAgeDays);

        var candidates = items
            .Where(x => x.PublishedAt >= cutoff)
            .OrderByDescending(x => x.PublishedAt)
            .Take(request.MaxPerRun * CandidateMultiplier)
            .ToList();

        if (candidates.Count == 0)
        {
            channel.LastError = null;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Ok(0, "Yeni video yok.");
        }

        var known = await videoRepository.GetExistingVideoIdsAsync(
            candidates.Select(x => x.VideoId).ToList(),
            cancellationToken);

        var unseen = candidates.Where(x => !known.Contains(x.VideoId)).ToList();

        if (unseen.Count == 0)
        {
            channel.LastError = null;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Ok(0, "Yeni video yok.");
        }

        // Besleme süre vermiyor; süreyi ayrıca okuyup hem canlı yayınları hem de
        // kotayı tek başına bitirecek uzunluktaki yayınları burada ayıklıyoruz.
        var durations = await durationReader.GetDurationsAsync(
            unseen.Select(x => x.VideoId).ToList(),
            cancellationToken);

        var added = 0;
        var skippedLive = 0;
        var skippedLength = 0;

        foreach (var item in unseen)
        {
            if (added >= request.MaxPerRun)
                break;

            if (!durations.TryGetValue(item.VideoId, out var seconds))
            {
                // Süre okunamadıysa video alınıyor; eleme için tahmin yürütmüyoruz.
                seconds = 0;
            }
            else if (seconds == 0)
            {
                // 0 saniye = devam eden ya da henüz yayınlanmamış yayın.
                if (request.SkipLive)
                {
                    skippedLive++;
                    continue;
                }
            }
            else if (seconds < request.MinDurationSeconds || seconds > request.MaxDurationSeconds)
            {
                skippedLength++;
                continue;
            }

            await videoRepository.AddAsync(
                new Video
                {
                    ChannelId = channel.Id,
                    YouTubeVideoId = item.VideoId,
                    Title = Truncate(item.Title, TitleMaxLength)!,
                    Description = Truncate(item.Description, DescriptionMaxLength),
                    ThumbnailUrl = item.ThumbnailUrl,
                    PublishedAt = item.PublishedAt,
                    DurationSeconds = seconds > 0 ? seconds : null
                },
                cancellationToken);

            added++;
        }

        channel.LastError = null;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (added > 0 || skippedLive > 0 || skippedLength > 0)
            logger.LogInformation(
                "{Channel}: {Added} video eklendi, {Live} canlı yayın ve {Length} süre dışı video atlandı.",
                channel.Name,
                added,
                skippedLive,
                skippedLength);

        return Result<int>.Ok(added);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
