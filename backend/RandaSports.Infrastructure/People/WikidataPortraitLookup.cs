using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.People;

/// <summary>
/// Portreyi Wikidata'dan çeker. İki koruma var ve ikisi de şart: aday "insan"
/// (P31=Q5) olmalı ve etiketi aranan adla birebir eşleşmeli. Birden fazla insan
/// tam eşleşirse hiçbiri alınmıyor — yanlış kişinin fotoğrafını basmak,
/// fotoğrafsız kalmaktan kötü. Dönen görseller Commons'ta açık lisanslı.
/// </summary>
public sealed class WikidataPortraitLookup(
    WikidataClient client,
    ILogger<WikidataPortraitLookup> logger) : IPortraitLookup
{
    private const int ImageWidth = 400;

    public async Task<string?> FindAsync(string fullName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        try
        {
            var ids = await client.SearchAsync(fullName, cancellationToken);

            if (ids.Count == 0)
                return null;

            using var entities = await client.GetEntitiesAsync(ids, cancellationToken);

            if (entities is null || !entities.RootElement.TryGetProperty("entities", out var map))
                return null;

            var aranan = TextNormalizer.Slugify(fullName, 200);
            var eslesen = new List<string>();

            foreach (var id in ids)
            {
                if (!map.TryGetProperty(id, out var entity))
                    continue;

                if (!WikidataClient.IsInstanceOf(entity, WikidataClient.Human)
                    || !WikidataClient.LabelMatches(entity, aranan))
                    continue;

                if (WikidataClient.FileName(entity, WikidataClient.Image) is { } file)
                    eslesen.Add(file);
            }

            // Tek bir kişi tam eşleşmediyse karar vermiyoruz.
            if (eslesen.Count != 1)
            {
                if (eslesen.Count > 1)
                    logger.LogInformation(
                        "Portre alınmadı, {Count} ayrı kişi aynı adla eşleşti: {Name}",
                        eslesen.Count,
                        fullName);

                return null;
            }

            return WikidataClient.FilePath(eslesen[0], ImageWidth);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Portre araması başarısız: {Name}", fullName);
            return null;
        }
    }
}
