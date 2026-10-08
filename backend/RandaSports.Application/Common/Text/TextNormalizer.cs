using System.Globalization;
using System.Text;

namespace RandaSports.Application.Common.Text;

/// <summary>Türkçe metni URL ve karşılaştırma için sadeleştirir.</summary>
public static class TextNormalizer
{
    private static readonly CultureInfo Turkish = new("tr-TR");

    /// <summary>"Kasımpaşa - Galatasaray" → "kasimpasa-galatasaray"</summary>
    public static string Slugify(string? value, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var lowered = value
            .ToLower(Turkish)
            .Replace('ı', 'i')
            .Replace('İ', 'i')
            .Replace('ş', 's')
            .Replace('ğ', 'g')
            .Replace('ç', 'c')
            .Replace('ö', 'o')
            .Replace('ü', 'u');

        var decomposed = lowered.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var lastWasDash = false;

        foreach (var ch in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsAsciiLetterOrDigit(ch))
            {
                builder.Append(ch);
                lastWasDash = false;
                continue;
            }

            if (builder.Length > 0 && !lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        var slug = builder.ToString().Trim('-');

        return slug.Length <= maxLength
            ? slug
            : slug[..maxLength].Trim('-');
    }

    /// <summary>Alıntı doğrulaması için: küçük harf, sadece harf ve rakam.</summary>
    public static string Compact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var lowered = value.ToLower(Turkish);
        return string.Concat(lowered.Where(char.IsLetterOrDigit));
    }
}
