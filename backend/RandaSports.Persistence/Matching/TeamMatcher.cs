using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Matching;

public class TeamMatcher(RandaSportsDbContext context, IMemoryCache cache) : ITeamMatcher
{
    private const string CacheKey = "team-dictionary";
    private const int MinAliasLength = 4;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<List<MatchedTeam>> MatchAsync(
        string? title,
        string? body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body))
            return [];

        var dictionary = await GetDictionaryAsync(cancellationToken);
        if (dictionary.Count == 0)
            return [];

        var titleText = Pad(title);
        var bodyText = Pad(body);

        var matches = new List<MatchedTeam>();

        foreach (var entry in dictionary)
        {
            var inTitle = entry.Patterns.Any(titleText.Contains);
            var inBody = !inTitle && entry.Patterns.Any(bodyText.Contains);

            if (inTitle || inBody)
                matches.Add(new MatchedTeam(entry.Id, entry.Name, entry.Slug, entry.SportId, entry.SportSlug, inTitle));
        }

        return matches;
    }

    private Task<List<DictionaryEntry>> GetDictionaryAsync(CancellationToken cancellationToken) =>
        cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var teams = await context.Teams
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Slug,
                    x.SportId,
                    SportSlug = x.Sport.Slug,
                    x.Aliases
                })
                .ToListAsync(cancellationToken);

            return teams
                .Select(x => new DictionaryEntry(
                    x.Id,
                    x.Name,
                    x.Slug,
                    x.SportId,
                    x.SportSlug,
                    BuildPatterns(x.Name, x.Slug, x.Aliases)))
                .Where(x => x.Patterns.Length > 0)
                .ToList();
        })!;

    /// <summary>Ad ve takma adları "-galatasaray-" biçimine çevirir; kelime ortasında eşleşme olmaz.</summary>
    private static string[] BuildPatterns(string name, string slug, List<string> aliases)
    {
        var values = new List<string> { name, slug };
        values.AddRange(aliases);

        return values
            .Select(x => TextNormalizer.Slugify(x, 80))
            .Where(x => x.Length >= MinAliasLength)
            .Distinct(StringComparer.Ordinal)
            .Select(x => $"-{x}-")
            .ToArray();
    }

    private static string Pad(string? value) => $"-{TextNormalizer.Slugify(value, 4000)}-";

    private sealed record DictionaryEntry(
        Guid Id,
        string Name,
        string Slug,
        Guid SportId,
        string SportSlug,
        string[] Patterns);
}
