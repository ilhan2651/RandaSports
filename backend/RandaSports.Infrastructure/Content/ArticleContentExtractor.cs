using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Interfaces.Services;
using SmartReader;

namespace RandaSports.Infrastructure.Content;

public sealed partial class ArticleContentExtractor(
    HttpClient httpClient,
    ILogger<ArticleContentExtractor> logger) : IArticleContentExtractor
{
    public async Task<ExtractedArticle?> ExtractAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var html = await httpClient.GetStringAsync(url, cancellationToken);

            var reader = new Reader(url, html);
            var article = await reader.GetArticleAsync();

            var text = article.IsReadable ? article.TextContent?.Trim() : null;

            // SmartReader kimi sayfayı makale saymıyor: ESPN'in haber sayfaları ve AA gibi
            // kısa ajans haberleri böyle eleniyordu. Pes etmeden paragrafları kendimiz
            // topluyoruz; gövde gerçekten oradaysa haber kurtuluyor.
            if (string.IsNullOrWhiteSpace(text))
                text = CollectParagraphs(html);

            if (string.IsNullOrWhiteSpace(text))
            {
                // Sayfanın ne döndüğünü tek bakışta görebilmek için: bot koruma sayfası
                // birkaç KB ve "Access Denied" başlıklı gelir, gerçek makale yüz KB'larca.
                logger.LogWarning(
                    "Sayfadan içerik çıkmadı: {Url} (html {Length} bayt, başlık: {Title})",
                    url,
                    html.Length,
                    FindTitle(html) ?? "-");

                return null;
            }

            return new ExtractedArticle(text, FindShareImage(html, url));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Haber sayfası alınamadı: {Url}", url);
            return null;
        }
    }

    /// <summary>Paragraf metnini toplamanın anlamlı sayılması için gereken en az uzunluk.</summary>
    private const int MinParagraphLength = 40;

    /// <summary>
    /// Yedek çıkarım: sayfadaki paragrafları birleştirir. Önce makale gövdesini sınırlayan
    /// bir kap aranıyor (article, sonra main) — menü, ilgili haberler ve altbilgi metni
    /// böylece dışarıda kalıyor. Kap yoksa sayfanın tamamına düşülüyor.
    /// </summary>
    private static string? CollectParagraphs(string html)
    {
        var scope = FirstGroup(ArticleBlockRegex(), html)
                    ?? FirstGroup(MainBlockRegex(), html)
                    ?? html;

        var paragraphs = ParagraphRegex()
            .Matches(scope)
            .Cast<Match>()
            .Select(x => CleanText(x.Groups["v"].Value))
            .Where(x => x.Length >= MinParagraphLength)
            .ToList();

        if (paragraphs.Count == 0)
            return null;

        return string.Join("\n\n", paragraphs);
    }

    private static string? FirstGroup(Regex regex, string html)
    {
        var match = regex.Match(html);
        return match.Success ? match.Groups["v"].Value : null;
    }

    /// <summary>Etiketleri atıp HTML varlıklarını çözer, boşlukları tek boşluğa indirir.</summary>
    private static string CleanText(string value)
    {
        var withoutTags = HtmlTagRegex().Replace(value, " ");
        var decoded = WebUtility.HtmlDecode(withoutTags);

        return WhitespaceRegex().Replace(decoded, " ").Trim();
    }

    private static string? FindTitle(string html)
    {
        var title = FirstGroup(TitleRegex(), html);
        return string.IsNullOrWhiteSpace(title) ? null : CleanText(title);
    }

    /// <summary>
    /// Sosyal paylaşım görselini okur. Etiketlerin sırası siteye göre değiştiği için
    /// meta etiketleri tek tek geziliyor; önce og:image, yoksa twitter:image.
    /// </summary>
    private static string? FindShareImage(string html, string pageUrl)
    {
        string? twitterImage = null;

        foreach (var tag in MetaTagRegex().Matches(html).Cast<Match>().Select(x => x.Value))
        {
            var isOpenGraph = tag.Contains("og:image", StringComparison.OrdinalIgnoreCase);
            var isTwitter = tag.Contains("twitter:image", StringComparison.OrdinalIgnoreCase);

            if (!isOpenGraph && !isTwitter)
                continue;

            var value = ContentAttributeRegex().Match(tag).Groups["v"].Value;
            var absolute = ToAbsolute(value, pageUrl);

            if (absolute is null)
                continue;

            if (isOpenGraph)
                return absolute;

            twitterImage ??= absolute;
        }

        return twitterImage;
    }

    /// <summary>
    /// Göreli adresleri sayfanın adresine göre tamamlar; http/https dışındakileri
    /// (data:, javascript:) eler.
    ///
    /// Zaten tam olan adres olduğu gibi dönüyor: Uri.ToString() yüzde kaçışlarını
    /// normalleştirip bazı CDN adreslerini bozabiliyor (ESPN'in combiner adresi böyle).
    /// "/kapak.jpg" gibi bir yol Linux'ta mutlak bir file: adresi olarak ayrıştığından
    /// şema kontrolü ilk koşulun içinde: aksi hâlde göreli adresler işletim sistemine
    /// göre farklı davranıyor.
    /// </summary>
    private static string? ToAbsolute(string? value, string pageUrl)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var decoded = WebUtility.HtmlDecode(value).Trim();

        if (Uri.TryCreate(decoded, UriKind.Absolute, out var absolute) && IsWeb(absolute))
            return decoded;

        return Uri.TryCreate(new Uri(pageUrl), decoded, out var relative) && IsWeb(relative)
            ? relative.ToString()
            : null;
    }

    private static bool IsWeb(Uri uri) => uri.Scheme is "http" or "https";

    [GeneratedRegex("<meta[^>]+>", RegexOptions.IgnoreCase)]
    private static partial Regex MetaTagRegex();

    [GeneratedRegex("<article[^>]*>(?<v>.*?)</article>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ArticleBlockRegex();

    [GeneratedRegex("<main[^>]*>(?<v>.*?)</main>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex MainBlockRegex();

    [GeneratedRegex("<p[^>]*>(?<v>.*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex ParagraphRegex();

    [GeneratedRegex("<title[^>]*>(?<v>.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex TitleRegex();

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("""content\s*=\s*["'](?<v>[^"']+)["']""", RegexOptions.IgnoreCase)]
    private static partial Regex ContentAttributeRegex();
}
