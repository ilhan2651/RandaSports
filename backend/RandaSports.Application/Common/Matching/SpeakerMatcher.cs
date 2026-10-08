using RandaSports.Application.Common.Text;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Common.Matching;

/// <summary>
/// Modelin duyduğu konuşmacı adını kayıtlı yorumcu listesiyle eşleştirir.
/// Modelin ismi doğru söylediğine güvenmiyoruz: sözlükte karşılığı yoksa görüş
/// isimsiz kalıyor.
/// </summary>
public static class SpeakerMatcher
{
    /// <summary>Kısa parçalar ("ali", "cem") yanlış kişiye bağlanmasın diye alt sınır.</summary>
    private const int MinPatternLength = 5;

    private const int TitleNameMaxLength = 40;
    private const int TitleNameMaxWords = 4;

    /// <summary>
    /// Kanallar kısa kliplerde konuşmacıyı başlığın sonuna koyuyor:
    /// "Artık insanların bakışı değişecek | Emek Ege". Bu, modelin açıklamadaki
    /// konuk listesinden isim seçmesine göre çok daha güvenilir bir işaret.
    /// </summary>
    public static string? ExtractTitleSpeaker(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        var pipe = title.LastIndexOf('|');

        if (pipe < 0 || pipe == title.Length - 1)
            return null;

        var candidate = title[(pipe + 1)..].Trim();

        if (candidate.Length is 0 or > TitleNameMaxLength)
            return null;

        // "| Bölüm 3", "| CANLI" gibi ekleri eleyelim: isim rakam içermez ve kısadır.
        if (candidate.Any(char.IsDigit))
            return null;

        var words = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return words.Length is >= 1 and <= TitleNameMaxWords ? candidate : null;
    }

    /// <summary>
    /// Eşleşme bulunursa yorumcuyu verir. Hiç eşleşmezse ya da birden fazla kişiye
    /// uyuyorsa null döner — ikisinde de görüş isme bağlanmaz.
    /// </summary>
    public static Commentator? Match(string? spokenName, IReadOnlyCollection<Commentator> candidates)
    {
        if (string.IsNullOrWhiteSpace(spokenName) || candidates.Count == 0)
            return null;

        var spoken = TextNormalizer.Compact(spokenName);

        if (spoken.Length < MinPatternLength)
            return null;

        Commentator? found = null;

        foreach (var candidate in candidates)
        {
            if (!Matches(spoken, candidate))
                continue;

            if (found is not null && found.Id != candidate.Id)
                return null;

            found = candidate;
        }

        return found;
    }

    private static bool Matches(string spoken, Commentator candidate)
    {
        foreach (var pattern in Patterns(candidate))
        {
            // İki yön de kabul: model "Erman Toroğlu" ya da sadece "Toroğlu" demiş olabilir.
            if (spoken.Contains(pattern, StringComparison.Ordinal)
                || pattern.Contains(spoken, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static IEnumerable<string> Patterns(Commentator candidate)
    {
        var full = TextNormalizer.Compact(candidate.FullName);
        if (full.Length >= MinPatternLength)
            yield return full;

        foreach (var alias in candidate.Aliases)
        {
            var compact = TextNormalizer.Compact(alias);
            if (compact.Length >= MinPatternLength)
                yield return compact;
        }
    }
}
