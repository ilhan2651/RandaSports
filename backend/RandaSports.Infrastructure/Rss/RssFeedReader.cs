using System.Globalization;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Rss;

public sealed partial class RssFeedReader(HttpClient httpClient) : IRssFeedReader
{
    private const string MediaNamespace = "http://search.yahoo.com/mrss/";

    public async Task<IReadOnlyList<RssFeedItem>> ReadAsync(string feedUrl, CancellationToken cancellationToken = default)
    {
        var bytes = await httpClient.GetByteArrayAsync(feedUrl, cancellationToken);

        using var stream = new MemoryStream(bytes);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreWhitespace = true,
            IgnoreComments = true
        });

        SyndicationFeed? feed;
        try
        {
            feed = SyndicationFeed.Load(reader);
        }
        catch (XmlException)
        {
            // Bazı siteler (örn. NTV Spor) standart dışı Atom üretiyor.
            // Sıkı parser reddedince kendi esnek okuyucumuza düşüyoruz.
            return ParseLoosely(bytes);
        }

        if (feed is null)
            return [];

        var items = new List<RssFeedItem>();

        foreach (var item in feed.Items)
        {
            var title = CleanText(item.Title?.Text);
            var url = GetItemUrl(item);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
                continue;

            items.Add(new RssFeedItem(
                title,
                url,
                CleanText(GetSummary(item)),
                GetImageUrl(item),
                item.Authors.FirstOrDefault()?.Name,
                GetPublishedAt(item)));
        }

        return items;
    }

    private static IReadOnlyList<RssFeedItem> ParseLoosely(byte[] bytes)
    {
        XDocument document;

        try
        {
            using var stream = new MemoryStream(bytes);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreComments = true
            });

            document = XDocument.Load(reader);
        }
        catch (XmlException)
        {
            // Beslemenin XML'i tamir edilemeyecek kadar bozuk (örn. Hürriyet'te kaçırılmamış
            // tırnak ve etiket). Son çare olarak metni desen eşlemeyle okuyoruz.
            return ParseWithPatterns(bytes);
        }

        var items = new List<RssFeedItem>();

        foreach (var node in document.Descendants().Where(x => x.Name.LocalName is "item" or "entry"))
        {
            var title = CleanText(Element(node, "title")?.Value);
            var url = LooseUrl(node);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
                continue;

            var html = Element(node, "description")?.Value
                       ?? Element(node, "summary")?.Value
                       ?? Element(node, "content")?.Value;

            items.Add(new RssFeedItem(
                title,
                url,
                CleanText(html),
                LooseImageUrl(node),
                CleanText(Element(node, "creator")?.Value ?? Element(node, "author")?.Value),
                LoosePublishedAt(node)));
        }

        return items;
    }

    /// <summary>XML kurallarına hiç uymayan beslemeler için metin üzerinden okuma.</summary>
    private static IReadOnlyList<RssFeedItem> ParseWithPatterns(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var items = new List<RssFeedItem>();

        foreach (var block in ItemBlockRegex().Matches(text).Cast<Match>())
        {
            var body = block.Groups["body"].Value;

            var title = CleanText(Value(TitleRegex(), body));
            var url = PatternUrl(body);

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
                continue;

            var html = Value(SummaryRegex(), body);

            items.Add(new RssFeedItem(
                title,
                url,
                CleanText(html),
                PatternImageUrl(body),
                null,
                PatternPublishedAt(body)));
        }

        return items;
    }

    private static string? PatternUrl(string body)
    {
        var candidates = new[]
        {
            Value(LinkHrefRegex(), body),
            Value(LinkTextRegex(), body),
            Value(GuidRegex(), body)
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
                continue;

            var decoded = WebUtility.HtmlDecode(candidate).Trim();

            if (Uri.TryCreate(decoded, UriKind.Absolute, out var uri))
                return uri.ToString();
        }

        return null;
    }

    private static string? PatternImageUrl(string body)
    {
        var url = Value(MediaUrlRegex(), body);
        if (!string.IsNullOrWhiteSpace(url))
            return WebUtility.HtmlDecode(url).Trim();

        // Son çare okuma: blok zaten item'ın tamamı, img'yi doğrudan içinde arıyoruz.
        return FindImageInHtml(body);
    }

    private static DateTimeOffset PatternPublishedAt(string body)
    {
        var raw = Value(DateRegex(), body);

        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.ToUniversalTime()
            : DateTimeOffset.UtcNow;
    }

    /// <summary>Etiketin içini döndürür; CDATA sarmalıysa içeriğini açar.</summary>
    private static string? Value(Regex regex, string body)
    {
        var match = regex.Match(body);
        if (!match.Success)
            return null;

        var value = match.Groups["v"].Value.Trim();
        var cdata = CdataRegex().Match(value);

        return cdata.Success ? cdata.Groups["v"].Value.Trim() : value;
    }

    private static XElement? Element(XElement parent, string localName) =>
        parent.Elements().FirstOrDefault(x => x.Name.LocalName == localName);

    private static string? LooseUrl(XElement node)
    {
        foreach (var link in node.Elements().Where(x => x.Name.LocalName == "link"))
        {
            var rel = link.Attribute("rel")?.Value;
            if (rel is not null && rel != "alternate")
                continue;

            var value = link.Attribute("href")?.Value ?? link.Value;
            if (Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri))
                return uri.ToString();
        }

        var id = Element(node, "guid")?.Value ?? Element(node, "id")?.Value;
        return Uri.TryCreate(id?.Trim(), UriKind.Absolute, out var idUri) ? idUri.ToString() : null;
    }

    private static string? LooseImageUrl(XElement node)
    {
        foreach (var element in node.Elements())
        {
            var name = element.Name.LocalName;

            if (name is "enclosure" or "content" or "thumbnail" or "image")
            {
                var url = element.Attribute("url")?.Value;
                if (!string.IsNullOrWhiteSpace(url))
                    return url;
            }
        }

        // Metin alanlarının hepsini deniyoruz: görsel çoğu beslemede açıklamada,
        // bazılarında yalnızca tam içerikte duruyor.
        return FindImageInHtml(Element(node, "description")?.Value)
               ?? FindImageInHtml(Element(node, "content")?.Value)
               ?? FindImageInHtml(Element(node, "summary")?.Value);
    }

    /// <summary>HTML parçasındaki ilk img adresini verir; yoksa null.</summary>
    private static string? FindImageInHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return null;

        var match = ImageTagRegex().Match(html);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["src"].Value).Trim() : null;
    }

    private static DateTimeOffset LoosePublishedAt(XElement node)
    {
        var raw = Element(node, "pubDate")?.Value
                  ?? Element(node, "published")?.Value
                  ?? Element(node, "updated")?.Value
                  ?? Element(node, "date")?.Value;

        return DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed.ToUniversalTime()
            : DateTimeOffset.UtcNow;
    }

    private static string? GetItemUrl(SyndicationItem item)
    {
        var link = item.Links.FirstOrDefault(x => x.RelationshipType is null or "alternate" && x.Uri is not null)
                   ?? item.Links.FirstOrDefault(x => x.MediaType is null && x.Uri is not null);

        if (link?.Uri is not null)
            return link.Uri.IsAbsoluteUri ? link.Uri.ToString() : null;

        return Uri.TryCreate(item.Id, UriKind.Absolute, out var id) ? id.ToString() : null;
    }

    private static string? GetSummary(SyndicationItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.Summary?.Text))
            return item.Summary.Text;

        return item.Content is TextSyndicationContent content ? content.Text : null;
    }

    private static DateTimeOffset GetPublishedAt(SyndicationItem item)
    {
        if (item.PublishDate != default)
            return item.PublishDate.ToUniversalTime();

        return item.LastUpdatedTime != default
            ? item.LastUpdatedTime.ToUniversalTime()
            : DateTimeOffset.UtcNow;
    }

    private static string? GetImageUrl(SyndicationItem item)
    {
        var enclosure = item.Links.FirstOrDefault(x =>
            x.RelationshipType == "enclosure" &&
            x.MediaType?.StartsWith("image", StringComparison.OrdinalIgnoreCase) == true);

        if (enclosure?.Uri is not null)
            return enclosure.Uri.ToString();

        foreach (var extension in item.ElementExtensions)
        {
            var element = extension.GetObject<XElement>();

            if (element.Name.NamespaceName != MediaNamespace)
                continue;

            if (element.Name.LocalName is not ("content" or "thumbnail"))
                continue;

            var url = element.Attribute("url")?.Value;
            if (!string.IsNullOrWhiteSpace(url))
                return url;
        }

        // Özette görsel olmayabilir ama içerikte olabilir (Vox beslemeleri böyle:
        // summary düz metin, img yalnızca content içinde). İkisine de bakıyoruz.
        return FindImageInHtml(item.Summary?.Text)
               ?? FindImageInHtml((item.Content as TextSyndicationContent)?.Text);
    }

    private static string? CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var withoutTags = HtmlTagRegex().Replace(value, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);

        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex("<img[^>]+src=[\"'](?<src>[^\"']+)[\"']", RegexOptions.IgnoreCase)]
    private static partial Regex ImageTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"<(?:item|entry)\b[^>]*>(?<body>.*?)</(?:item|entry)>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex ItemBlockRegex();

    [GeneratedRegex(@"<title\b[^>]*>(?<v>.*?)</title>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex TitleRegex();

    [GeneratedRegex(@"<link\b[^>]*?\bhref\s*=\s*[""'](?<v>[^""']+)[""']", RegexOptions.IgnoreCase)]
    private static partial Regex LinkHrefRegex();

    [GeneratedRegex(@"<link\b[^>]*>(?<v>.*?)</link>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex LinkTextRegex();

    [GeneratedRegex(@"<(?:guid|id)\b[^>]*>(?<v>.*?)</(?:guid|id)>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex GuidRegex();

    [GeneratedRegex(@"<(?:description|summary|content(?::encoded)?)\b[^>]*>(?<v>.*?)</(?:description|summary|content(?::encoded)?)>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex SummaryRegex();

    [GeneratedRegex(@"<(?:pubDate|published|updated|dc:date)\b[^>]*>(?<v>.*?)</(?:pubDate|published|updated|dc:date)>",
        RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"<(?:enclosure|media:content|media:thumbnail|image)\b[^>]*\burl\s*=\s*[""'](?<v>[^""']+)[""']",
        RegexOptions.IgnoreCase)]
    private static partial Regex MediaUrlRegex();

    [GeneratedRegex(@"<!\[CDATA\[(?<v>.*?)\]\]>", RegexOptions.Singleline)]
    private static partial Regex CdataRegex();
}
