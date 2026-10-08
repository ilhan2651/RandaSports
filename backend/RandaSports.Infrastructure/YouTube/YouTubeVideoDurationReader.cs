using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.YouTube;

public sealed partial class YouTubeVideoDurationReader(
    HttpClient httpClient,
    ILogger<YouTubeVideoDurationReader> logger) : IYouTubeVideoDurationReader
{
    public async Task<IReadOnlyDictionary<string, int>> GetDurationsAsync(
        IReadOnlyCollection<string> youTubeVideoIds,
        CancellationToken cancellationToken = default)
    {
        var durations = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var videoId in youTubeVideoIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var seconds = await ReadDurationAsync(videoId, cancellationToken);

            if (seconds is not null)
                durations[videoId] = seconds.Value;
        }

        return durations;
    }

    private async Task<int?> ReadDurationAsync(string videoId, CancellationToken cancellationToken)
    {
        try
        {
            var html = await httpClient.GetStringAsync(
                $"https://www.youtube.com/watch?v={Uri.EscapeDataString(videoId)}",
                cancellationToken);

            var match = LengthSecondsRegex().Match(html);

            if (!match.Success)
                return null;

            // Canlı ve yayınlanmamış videolarda bu alan 0 geliyor; onu olduğu gibi
            // taşıyoruz, ayıklama kararını çağıran veriyor.
            return int.TryParse(match.Groups["seconds"].Value, CultureInfo.InvariantCulture, out var seconds)
                ? seconds
                : null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Video süresi okunamadı: {VideoId}", videoId);
            return null;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Video süresi zaman aşımına uğradı: {VideoId}", videoId);
            return null;
        }
    }

    [GeneratedRegex("\"lengthSeconds\"\\s*:\\s*\"(?<seconds>\\d+)\"")]
    private static partial Regex LengthSecondsRegex();
}
