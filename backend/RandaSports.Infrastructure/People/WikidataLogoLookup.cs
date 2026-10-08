using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.People;

/// <summary>
/// Takım logosunu Wikidata'dan arar. İki ayrı yol var çünkü iki ayrı şey arıyoruz:
///
/// Milli takım: kulüp arması değil, ülkenin bayrağı aranıyor (P41). Ülke adları
/// benzersiz ve bayraklar Commons'ta özgür lisanslı, yani bu yol neredeyse her
/// zaman tutuyor.
///
/// Kulüp: armanın kendisi (P154). Armalar ticari marka olduğu için çoğu Commons'ta
/// özgür lisansla bulunmuyor — Wikipedia onları yerel olarak barındırıyor. Yani bu
/// yol sık sık boş dönecek ve bu bir hata değil; elle giriş o yüzden duruyor.
/// </summary>
public sealed class WikidataLogoLookup(
    WikidataClient client,
    ILogger<WikidataLogoLookup> logger) : ILogoLookup
{
    private const int LogoWidth = 200;

    /// <summary>Ülke ve bağımsız devlet türleri; milli takımda ülkeyi ayırt etmek için.</summary>
    private static readonly string[] CountryTypes = ["Q3624078", "Q6256", "Q7275"];

    public async Task<string?> FindAsync(
        LogoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return null;

        try
        {
            return request.IsNational
                ? await FindFlagAsync(request, cancellationToken)
                : await FindCrestAsync(request.Name, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Logo araması başarısız: {Name}", request.Name);
            return null;
        }
    }

    private async Task<string?> FindFlagAsync(LogoRequest request, CancellationToken cancellationToken)
    {
        // Takım adı ülke adıyla aynı olmayabiliyor ("Türkiye" vs "Türkiye A Millî Takımı");
        // ülke alanı doluysa onu tercih ediyoruz.
        var term = string.IsNullOrWhiteSpace(request.Country) ? request.Name : request.Country;

        return await AraAsync(
            term,
            WikidataClient.Flag,
            entity => WikidataClient.IsInstanceOf(entity, CountryTypes),
            cancellationToken);
    }

    private Task<string?> FindCrestAsync(string name, CancellationToken cancellationToken) =>
        // Tür kontrolü yok: kulüp türleri Wikidata'da çok çeşitli (futbol kulübü,
        // spor kulübü, şirket...). Bunun yerine P154'ün varlığı filtre görevi
        // görüyor — bir semtin ya da stadın logosu olmuyor.
        AraAsync(name, WikidataClient.Logo, _ => true, cancellationToken);

    private async Task<string?> AraAsync(
        string term,
        string property,
        Func<System.Text.Json.JsonElement, bool> uygunMu,
        CancellationToken cancellationToken)
    {
        var ids = await client.SearchAsync(term, cancellationToken);

        if (ids.Count == 0)
            return null;

        using var entities = await client.GetEntitiesAsync(ids, cancellationToken);

        if (entities is null || !entities.RootElement.TryGetProperty("entities", out var map))
            return null;

        var aranan = TextNormalizer.Slugify(term, 200);

        // Arama sonuçları alaka sırasında; ilk uyan kabul ediliyor.
        foreach (var id in ids)
        {
            if (!map.TryGetProperty(id, out var entity))
                continue;

            if (!WikidataClient.LabelMatches(entity, aranan) || !uygunMu(entity))
                continue;

            if (WikidataClient.FileName(entity, property) is { } file)
                return WikidataClient.FilePath(file, LogoWidth);
        }

        return null;
    }
}
