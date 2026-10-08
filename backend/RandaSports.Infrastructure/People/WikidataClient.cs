using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;

namespace RandaSports.Infrastructure.People;

/// <summary>
/// Wikidata'nın iki ucunu saran ince katman: isimle aday ara, adayların iddialarını
/// çek. Hem portre hem logo araması aynı uçları kullandığı için burada duruyor.
///
/// İstekler süreç genelinde teke indiriliyor ve aralarına zorunlu bir boşluk
/// konuyor. Wikidata anonim istemcileri sınırlıyor; iki worker aynı anda arka
/// arkaya istek atınca sunucu 429 döndürüp bir süre hepsini reddediyor. Kapı
/// statik çünkü sınır istemci nesnesi başına değil, sunucuya giden trafiğin
/// tamamı için geçerli.
/// </summary>
public sealed class WikidataClient(HttpClient httpClient, ILogger<WikidataClient> logger)
{
    public const string InstanceOf = "P31";
    public const string Image = "P18";
    public const string Logo = "P154";
    public const string Flag = "P41";
    public const string Human = "Q5";

    private const int CandidateLimit = 5;
    private const int MaxRetries = 3;

    /// <summary>İki istek arasındaki en az boşluk.</summary>
    private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(400);

    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static DateTimeOffset _nextAllowed = DateTimeOffset.MinValue;

    public async Task<List<string>> SearchAsync(string term, CancellationToken cancellationToken)
    {
        var url =
            $"w/api.php?action=wbsearchentities&format=json&type=item&language=tr&uselang=tr&limit={CandidateLimit}&search={WebUtility.UrlEncode(term)}";

        using var document = await GetAsync(url, $"arama: {term}", cancellationToken);

        if (document is null || !document.RootElement.TryGetProperty("search", out var results))
            return [];

        return [.. results
            .EnumerateArray()
            .Select(x => x.TryGetProperty("id", out var id) ? id.GetString() : null)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)];
    }

    public Task<JsonDocument?> GetEntitiesAsync(
        List<string> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return Task.FromResult<JsonDocument?>(null);

        var url =
            $"w/api.php?action=wbgetentities&format=json&props=claims%7Clabels&languages=tr%7Cen&ids={string.Join('|', ids)}";

        return GetAsync(url, "varlık çekme", cancellationToken);
    }

    /// <summary>
    /// Tek sıraya dizilmiş, aralıklı ve 429'da geri çekilen istek. Sunucu
    /// <c>Retry-After</c> söylüyorsa ona uyuyoruz; söylemiyorsa katlanarak artan
    /// bir bekleme uyguluyoruz. 429 gelince sıradaki istekler de erteleniyor,
    /// yoksa aynı duvara arka arkaya toslamaya devam ediyoruz.
    /// </summary>
    private async Task<JsonDocument?> GetAsync(
        string url,
        string neIcin,
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            HttpResponseMessage response;

            await Gate.WaitAsync(cancellationToken);

            try
            {
                var bekle = _nextAllowed - DateTimeOffset.UtcNow;

                if (bekle > TimeSpan.Zero)
                    await Task.Delay(bekle, cancellationToken);

                response = await httpClient.GetAsync(url, cancellationToken);
                _nextAllowed = DateTimeOffset.UtcNow + MinInterval;
            }
            finally
            {
                Gate.Release();
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                    return await JsonDocument.ParseAsync(
                        await response.Content.ReadAsStreamAsync(cancellationToken),
                        cancellationToken: cancellationToken);

                if (response.StatusCode is not HttpStatusCode.TooManyRequests || attempt >= MaxRetries)
                {
                    logger.LogWarning(
                        "Wikidata isteği başarısız ({StatusCode}) — {NeIcin}",
                        (int)response.StatusCode,
                        neIcin);

                    return null;
                }

                var gecikme = RetryAfter(response) ?? TimeSpan.FromSeconds(Math.Pow(2, attempt + 1));

                // Sıradaki istekler de beklesin; tek tek aynı sınıra çarpmasınlar.
                _nextAllowed = DateTimeOffset.UtcNow + gecikme;

                logger.LogInformation(
                    "Wikidata hız sınırı, {Saniye} sn sonra tekrar denenecek — {NeIcin}",
                    (int)gecikme.TotalSeconds,
                    neIcin);

                await Task.Delay(gecikme, cancellationToken);
            }
        }
    }

    private static TimeSpan? RetryAfter(HttpResponseMessage response)
    {
        var header = response.Headers.RetryAfter;

        if (header?.Delta is { } delta)
            return delta;

        return header?.Date is { } date && date > DateTimeOffset.UtcNow
            ? date - DateTimeOffset.UtcNow
            : null;
    }

    /// <summary>Commons dosya adını, istediğimiz genişlikte kalıcı bir adrese çeviriyor.</summary>
    public static string FilePath(string fileName, int width) =>
        $"https://commons.wikimedia.org/wiki/Special:FilePath/{WebUtility.UrlEncode(fileName)}?width={width}";

    public static IEnumerable<JsonElement> Claims(JsonElement entity, string property)
    {
        if (!entity.TryGetProperty("claims", out var claims)
            || !claims.TryGetProperty(property, out var list))
            return [];

        return list.EnumerateArray();
    }

    /// <summary>P18/P154/P41 gibi dosya alanlarının değeri düz bir dosya adı.</summary>
    public static string? FileName(JsonElement entity, string property) =>
        Claims(entity, property)
            .Select(Value)
            .Select(x => x?.ValueKind == JsonValueKind.String ? x.Value.GetString() : null)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

    /// <summary>P31 gibi varlığa işaret eden alanların değeri bir kimlik taşıyor.</summary>
    public static string? EntityId(JsonElement claim) =>
        Value(claim) is { ValueKind: JsonValueKind.Object } value
        && value.TryGetProperty("id", out var id)
            ? id.GetString()
            : null;

    public static bool IsInstanceOf(JsonElement entity, params string[] types) =>
        Claims(entity, InstanceOf).Any(x => EntityId(x) is { } id && types.Contains(id));

    /// <summary>Etiketlerden biri aranan adla birebir eşleşiyor mu.</summary>
    public static bool LabelMatches(JsonElement entity, string normalizedTerm)
    {
        if (!entity.TryGetProperty("labels", out var labels))
            return false;

        foreach (var label in labels.EnumerateObject())
        {
            if (!label.Value.TryGetProperty("value", out var value))
                continue;

            if (TextNormalizer.Slugify(value.GetString(), 200) == normalizedTerm)
                return true;
        }

        return false;
    }

    private static JsonElement? Value(JsonElement claim) =>
        claim.TryGetProperty("mainsnak", out var snak)
        && snak.TryGetProperty("datavalue", out var data)
        && data.TryGetProperty("value", out var value)
            ? value
            : null;
}
