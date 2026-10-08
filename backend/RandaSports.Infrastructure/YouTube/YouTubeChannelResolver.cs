using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.YouTube;

/// <summary>
/// Kanal referansından UC... kimliğini çıkarır. Referans "@sporx", tam kanal
/// adresi ya da doğrudan kimliğin kendisi olabilir.
/// </summary>
public sealed partial class YouTubeChannelResolver(
    HttpClient httpClient,
    ILogger<YouTubeChannelResolver> logger) : IYouTubeChannelResolver
{
    public async Task<string?> ResolveChannelIdAsync(string reference, CancellationToken cancellationToken = default)
    {
        var trimmed = reference?.Trim();

        if (string.IsNullOrEmpty(trimmed))
            return null;

        // Kimliğin kendisi verilmişse ağa çıkmaya gerek yok.
        var direct = ChannelIdInTextRegex().Match(trimmed);
        if (direct.Success)
            return direct.Groups["id"].Value;

        var url = BuildChannelUrl(trimmed);

        try
        {
            var html = await httpClient.GetStringAsync(url, cancellationToken);

            // Önce canonical bağlantı: sayfa hangi kanala aitse onu gösterir.
            var match = CanonicalLinkRegex().Match(html);
            if (match.Success)
                return match.Groups["id"].Value;

            // JSON'daki ilk "channelId" sayfadaki başka bir kanalın (önerilen kanal,
            // başka kanaldan bir video) olabiliyor; bu yüzden ikinci sırada.
            match = ChannelIdJsonRegex().Match(html);
            if (match.Success)
                return match.Groups["id"].Value;

            logger.LogWarning("Kanal kimliği sayfada bulunamadı: {Reference}", trimmed);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Kanal sayfası açılamadı: {Reference}", trimmed);
            return null;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Kanal sayfası zaman aşımına uğradı: {Reference}", trimmed);
            return null;
        }
    }

    private static string BuildChannelUrl(string reference)
    {
        if (reference.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return reference;

        if (reference.StartsWith("youtube.com", StringComparison.OrdinalIgnoreCase)
            || reference.StartsWith("www.youtube.com", StringComparison.OrdinalIgnoreCase))
            return $"https://{reference}";

        var handle = reference.TrimStart('@');
        return $"https://www.youtube.com/@{handle}";
    }

    [GeneratedRegex(@"(?:^|/)(?<id>UC[\w-]{20,})(?:$|/|\?)")]
    private static partial Regex ChannelIdInTextRegex();

    [GeneratedRegex(@"""channelId""\s*:\s*""(?<id>UC[\w-]{20,})""")]
    private static partial Regex ChannelIdJsonRegex();

    [GeneratedRegex(@"youtube\.com/channel/(?<id>UC[\w-]{20,})")]
    private static partial Regex CanonicalLinkRegex();
}
