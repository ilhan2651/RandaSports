using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class ArticleRepository(RandaSportsDbContext context)
    : GenericRepository<Article>(context), IArticleRepository
{
    public async Task<Paged<Article>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sportId = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Articles
            .AsNoTracking()
            .Include(x => x.Source)
            .Include(x => x.Sport)
            .AsQueryable();

        if (sportId.HasValue)
            query = query.Where(x => x.SportId == sportId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Paged<Article>(items, total, page, pageSize);
    }

    public Task<List<Guid>> GetPendingAiArticleIdsAsync(
        int take,
        int maxAttempts,
        CancellationToken cancellationToken = default) =>
        Context.Articles
            .AsNoTracking()
            .Where(x => x.AiProcessedAt == null && x.AiAttempts < maxAttempts)
            .OrderByDescending(x => x.PublishedAt)
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<List<string>> GetExistingUrlsAsync(
        IReadOnlyCollection<string> urls,
        CancellationToken cancellationToken = default) =>
        Context.Articles
            .AsNoTracking()
            .Where(x => urls.Contains(x.Url))
            .Select(x => x.Url)
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> GetUnclusteredArticleIdsAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        Context.Articles
            .AsNoTracking()
            .Where(x => x.StoryId == null && x.AiProcessedAt != null && x.AiSummary != null)
            .OrderBy(x => x.PublishedAt)
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<Article?> GetForClusteringAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Context.Articles
            .Include(x => x.Source)
            .Include(x => x.Sport)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Article?> GetForEnrichAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        Context.Articles
            .Include(x => x.Source)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
