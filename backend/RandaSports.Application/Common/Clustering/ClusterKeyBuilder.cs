using System.Globalization;
using RandaSports.Application.Common.Text;

namespace RandaSports.Application.Common.Clustering;

/// <summary>
/// Aynı olayı anlatan haberleri bir araya getiren anahtarı üretir.
/// Örnek: "futbol|mac-sonucu|galatasaray+kasimpasa|2026-09-28"
/// </summary>
public static class ClusterKeyBuilder
{
    /// <summary>Gün sınırı Türkiye saatine göre belirlenir.</summary>
    private static readonly TimeSpan TurkeyOffset = TimeSpan.FromHours(3);

    private const int MaxEntities = 3;
    private const string Fallback = "genel";

    public static string Build(
        string? sportSlug,
        string? category,
        IEnumerable<string>? entities,
        DateTimeOffset publishedAt,
        Guid articleId)
    {
        var sport = Or(TextNormalizer.Slugify(sportSlug, 40), Fallback);
        var topic = Or(TextNormalizer.Slugify(category, 40), Fallback);
        var day = publishedAt.ToOffset(TurkeyOffset).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var picked = Pick(entities);

        // Tanınabilir takım/sporcu yoksa haberi kendi konusunda tutuyoruz;
        // yoksa aynı gün aynı kategorideki alakasız haberler tek konuda birleşirdi.
        return picked.Count == 0
            ? $"{sport}|{topic}|tek-{articleId:N}|{day}"
            : $"{sport}|{topic}|{string.Join('+', picked)}|{day}";
    }

    private static List<string> Pick(IEnumerable<string>? values) =>
        values is null
            ? []
            : values
                .Select(x => TextNormalizer.Slugify(x, 60))
                .Where(x => x.Length > 2)
                .Distinct(StringComparer.Ordinal)
                .Order(StringComparer.Ordinal)
                .Take(MaxEntities)
                .ToList();

    private static string Or(string value, string fallback) =>
        string.IsNullOrEmpty(value) ? fallback : value;
}
