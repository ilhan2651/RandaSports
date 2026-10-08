using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class StoryRepository(RandaSportsDbContext context)
    : GenericRepository<Story>(context), IStoryRepository
{
    public Task<Story?> GetByClusterKeyAsync(string clusterKey, CancellationToken cancellationToken = default) =>
        Context.Stories
            .Include(x => x.Teams)
            .FirstOrDefaultAsync(x => x.ClusterKey == clusterKey, cancellationToken);

    public Task<List<Guid>> GetSourceIdsAsync(Guid storyId, CancellationToken cancellationToken = default) =>
        Context.Articles
            .AsNoTracking()
            .Where(x => x.StoryId == storyId)
            .Select(x => x.SourceId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public Task<Story?> GetWithArticlesAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Stories
            .Include(x => x.Sport)
            .Include(x => x.Articles)
                .ThenInclude(x => x.Source)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Guid>> GetPendingWriteIdsAsync(
        int take,
        int maxAttempts,
        int minSourceCount,
        DateTimeOffset singleSourceCutoff,
        CancellationToken cancellationToken = default) =>
        Context.Stories
            .AsNoTracking()
            .Where(x => x.NeedsRewrite
                        && x.AiAttempts < maxAttempts
                        && (x.SourceCount >= minSourceCount || x.LastPublishedAt <= singleSourceCutoff))
            .OrderByDescending(x => x.LastPublishedAt)
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid excludeStoryId, CancellationToken cancellationToken = default) =>
        Context.Stories
            .AsNoTracking()
            .AnyAsync(x => x.Slug == slug && x.Id != excludeStoryId, cancellationToken);

    public async Task<Paged<Story>> GetPagedAsync(
        int page,
        int pageSize,
        string? sportSlug = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Stories
            .AsNoTracking()
            .Include(x => x.Sport)
            .Where(x => x.AiProcessedAt != null && x.Slug != null && x.Headline != null);

        if (!string.IsNullOrWhiteSpace(sportSlug))
            query = query.Where(x => x.Sport != null && x.Sport.Slug == sportSlug);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Headline!, term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.LastPublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Paged<Story>(items, total, page, pageSize);
    }

    public async Task<List<Story>> GetForFollowedAsync(
        List<Guid> teamIds,
        List<Guid> sportIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (teamIds.Count == 0 && sportIds.Count == 0)
            return [];

        var query = Context.Stories
            .AsNoTracking()
            .Include(x => x.Sport)
            .Include(x => x.Teams)
            .Where(x => x.Slug != null && x.Headline != null);

        // Tek sorguda iki ölçüt: takımı geçen VEYA branşı tutan. Ayrı sorgu atıp
        // birleştirmek sıralamayı bozardı. Boş liste Contains içinde "hiçbiri"
        // demek, yani o ölçüt kendiliğinden devre dışı kalıyor.
        query = query.Where(x =>
            x.Teams.Any(t => teamIds.Contains(t.Id))
            || (x.SportId != null && sportIds.Contains(x.SportId.Value)));

        return await query
            .OrderByDescending(x => x.LastPublishedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<Story?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Context.Stories
            .AsNoTracking()
            .Include(x => x.Sport)
            .Include(x => x.Articles)
                .ThenInclude(x => x.Source)
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<List<StoryClusterRef>> GetRecentClusterRefsAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default) =>
        Context.Stories
            .AsNoTracking()
            .Where(x => x.Slug != null && x.LastPublishedAt >= since)
            .OrderByDescending(x => x.LastPublishedAt)
            .Select(x => new StoryClusterRef(x.Id, x.ClusterKey, x.LastPublishedAt))
            .ToListAsync(cancellationToken);
}
