using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Enums;

namespace RandaSports.Infrastructure.People;

/// <summary>
/// Rolü modele soruyor. Tek çağrıda birden çok kişi gönderiliyor: kota pahalı ve
/// kişi başına ayrı istek atmanın bir faydası yok.
/// </summary>
public sealed class GeminiPersonRoleClassifier(
    IGeminiClient geminiClient,
    ILogger<GeminiPersonRoleClassifier> logger) : IPersonRoleClassifier
{
    public async Task<List<PersonRoleVerdict>> ClassifyAsync(
        IReadOnlyList<PersonRoleCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
            return [];

        string? json;

        try
        {
            json = await geminiClient.GenerateJsonAsync(
                PersonRolePromptBuilder.Build(candidates),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // İstisna dışarı çıkarsa çağıran tur komple düşer; rol tespiti
            // yapılamaması bir hata değil, bir sonraki turda tekrar denenir.
            logger.LogWarning(ex, "Rol tespiti için modele ulaşılamadı.");
            return [];
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            logger.LogWarning("Rol tespitinde model cevap vermedi.");
            return [];
        }

        var rows = PersonRolePromptBuilder.Parse(json);

        if (rows.Count == 0)
        {
            logger.LogWarning("Rol tespitinde model cevabı okunamadı.");
            return [];
        }

        // Eşleştirme kısa ad (slug) üzerinden: model "İlker Sırt" yerine "Ilker Sirt"
        // yazdığında düz metin karşılaştırması tutmuyor ve karar sessizce atılıyordu.
        // OrdinalIgnoreCase Türkçe İ/ı çiftini hiç tanımıyor.
        var bySlug = candidates
            .GroupBy(x => TextNormalizer.Slugify(x.FullName, 200))
            .ToDictionary(x => x.Key, x => x.First());

        var verdicts = new List<PersonRoleVerdict>();

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.FullName))
                continue;

            // Model uydurma bir isim döndürmüş olabilir; yalnızca sorduklarımızı alıyoruz.
            if (!bySlug.TryGetValue(TextNormalizer.Slugify(row.FullName, 200), out var candidate))
            {
                logger.LogInformation("Rol cevabı eşleşmedi, atlandı: {Name}", row.FullName);
                continue;
            }

            verdicts.Add(new PersonRoleVerdict(
                candidate.FullName,
                ParseRole(row.Role),
                Math.Clamp(row.Confidence ?? 0, 0, 1),
                row.Reason));
        }

        return verdicts;
    }

    private static PersonRole ParseRole(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "commentator" => PersonRole.Commentator,
            "athlete" => PersonRole.Athlete,
            "coach" => PersonRole.Coach,
            "official" => PersonRole.Official,
            _ => PersonRole.Unknown
        };
}
