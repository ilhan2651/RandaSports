using System.Globalization;
using Microsoft.Extensions.Options;

namespace RandaSports.Application.Common.Filtering;

public sealed class ArticleFilter(IOptions<ArticleFilterOptions> options) : IArticleFilter
{
    private static readonly CultureInfo Turkish = new("tr-TR");
    private readonly ArticleFilterOptions _options = options.Value;

    public bool ShouldSkip(string title, string url, out string reason)
    {
        reason = string.Empty;

        if (!_options.Enabled || string.IsNullOrWhiteSpace(title))
            return false;

        var loweredUrl = url.ToLower(Turkish);

        var urlPattern = _options.BlockedUrlPatterns
            .FirstOrDefault(x => loweredUrl.Contains(x.ToLower(Turkish), StringComparison.Ordinal));

        if (urlPattern is not null)
        {
            reason = $"adres kalıbı: {urlPattern}";
            return true;
        }

        var lowered = title.ToLower(Turkish);

        var pattern = _options.BlockedTitlePatterns
            .FirstOrDefault(x => lowered.Contains(x.ToLower(Turkish), StringComparison.Ordinal));

        if (pattern is not null)
        {
            reason = $"kalıp: {pattern}";
            return true;
        }

        if (IsMostlyUpperCase(title))
        {
            reason = "başlık büyük harf";
            return true;
        }

        return false;
    }

    private bool IsMostlyUpperCase(string title)
    {
        if (title.Length < _options.UpperCaseCheckMinLength)
            return false;

        var letters = 0;
        var upperCase = 0;

        foreach (var character in title)
        {
            if (!char.IsLetter(character))
                continue;

            letters++;

            if (char.IsUpper(character))
                upperCase++;
        }

        return letters > 0 && (double)upperCase / letters > _options.MaxUpperCaseRatio;
    }
}
