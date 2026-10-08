using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Matching;

public class SportDictionary(RandaSportsDbContext context, IMemoryCache cache) : ISportDictionary
{
    private const string CacheKey = "sport-dictionary";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public Task<List<SportEntry>> GetAllAsync(CancellationToken cancellationToken = default) =>
        cache.GetOrCreateAsync(CacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var rows = await context.Sports
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Id, x.Name, x.Slug })
                .ToListAsync(cancellationToken);

            return rows.Select(x => new SportEntry(x.Id, x.Name, x.Slug)).ToList();
        })!;

    public async Task<SportEntry?> FindAsync(string? slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var trimmed = slug.Trim();
        var sports = await GetAllAsync(cancellationToken);

        return sports.FirstOrDefault(x => string.Equals(x.Slug, trimmed, StringComparison.OrdinalIgnoreCase));
    }
}
