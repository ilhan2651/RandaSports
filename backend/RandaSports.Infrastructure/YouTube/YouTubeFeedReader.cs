using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.YouTube;

public sealed class YouTubeFeedReader(
    HttpClient httpClient,
    ILogger<YouTubeFeedReader> logger) : IYouTubeFeedReader
{
    private static readonly XNamespace Atom = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace Yt = "http://www.youtube.com/xml/schemas/2015";
    private static readonly XNamespace Media = "http://search.yahoo.com/mrss/";

    public async Task<IReadOnlyList<YouTubeFeedItem>> ReadChannelAsync(
        string youTubeChannelId,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://www.youtube.com/feeds/videos.xml?channel_id={Uri.EscapeDataString(youTubeChannelId)}";

        string xml;
        try
        {
            xml = await httpClient.GetStringAsync(url, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Kanal beslemesi okunamadı: {ChannelId}", youTubeChannelId);
            return [];
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException ex)
        {
            logger.LogWarning(ex, "Kanal beslemesi çözümlenemedi: {ChannelId}", youTubeChannelId);
            return [];
        }

        var items = new List<YouTubeFeedItem>();

        foreach (var entry in document.Descendants(Atom + "entry"))
        {
            var videoId = entry.Element(Yt + "videoId")?.Value?.Trim();
            var title = entry.Element(Atom + "title")?.Value?.Trim();

            if (string.IsNullOrEmpty(videoId) || string.IsNullOrEmpty(title))
                continue;

            var group = entry.Element(Media + "group");

            items.Add(new YouTubeFeedItem(
                videoId,
                title,
                group?.Element(Media + "description")?.Value?.Trim(),
                group?.Element(Media + "thumbnail")?.Attribute("url")?.Value,
                ParseDate(entry.Element(Atom + "published")?.Value)));
        }

        return items;
    }

    private static DateTimeOffset ParseDate(string? value) =>
        DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : DateTimeOffset.UtcNow;
}
