using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class OpinionRepository(RandaSportsDbContext context)
    : GenericRepository<Opinion>(context), IOpinionRepository
{
    public Task<List<Opinion>> GetApprovedByStoryAsync(Guid storyId, CancellationToken cancellationToken = default) =>
        Context.Opinions
            .AsNoTracking()
            .Include(x => x.Commentator)
            .Include(x => x.Video)
                .ThenInclude(x => x.Channel)
            .Where(x => x.StoryId == storyId && x.Status == OpinionStatus.Approved)
            .OrderByDescending(x => x.Video.PublishedAt)
            .ToListAsync(cancellationToken);

    public async Task<Paged<Opinion>> GetApprovedAsync(
        string? commentatorSlug,
        string? teamSlug,
        string? sportSlug,
        DateTimeOffset? since,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Opinions
            .AsNoTracking()
            .Include(x => x.Commentator)
            .Include(x => x.Team)
                .ThenInclude(x => x!.Sport)
            .Include(x => x.Sport)
            .Include(x => x.Video)
                .ThenInclude(x => x.Channel)
            .Include(x => x.Story)
            .Where(x => x.Status == OpinionStatus.Approved);

        if (!string.IsNullOrWhiteSpace(commentatorSlug))
            query = query.Where(x => x.Commentator != null && x.Commentator.Slug == commentatorSlug);

        if (!string.IsNullOrWhiteSpace(teamSlug))
            query = query.Where(x => x.Team != null && x.Team.Slug == teamSlug);

        if (!string.IsNullOrWhiteSpace(sportSlug))
            query = query.Where(x => x.Sport != null && x.Sport.Slug == sportSlug);

        if (since is not null)
            query = query.Where(x => x.Video.PublishedAt >= since);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Video.PublishedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Paged<Opinion>(items, total, page, pageSize);
    }

    public async Task<List<Opinion>> GetForFollowedAsync(
        List<Guid> commentatorIds,
        List<Guid> teamIds,
        List<Guid> sportIds,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (commentatorIds.Count == 0 && teamIds.Count == 0 && sportIds.Count == 0)
            return [];

        var query = Context.Opinions
            .AsNoTracking()
            .Include(x => x.Commentator)
            .Include(x => x.Team)
                .ThenInclude(x => x!.Sport)
            .Include(x => x.Sport)
            .Include(x => x.Video)
                .ThenInclude(x => x.Channel)
            .Include(x => x.Story)
            .Where(x => x.Status == OpinionStatus.Approved);

        // İki ayrı süzgeç ve ikisi de geçilmek zorunda: KİM konuşuyor, NE hakkında.
        // Seçim yapılmamış boyut süzmüyor — hiç yorumcu seçmeyen herkesi görüyor,
        // hiç branş/takım seçmeyen her konuyu görüyor. Önceden üçü VEYA ile bağlıydı;
        // Futbol'u takip etmek bütün futbol yorumcularını akışa dolduruyordu.
        if (commentatorIds.Count > 0)
            query = query.Where(x =>
                x.CommentatorId != null && commentatorIds.Contains(x.CommentatorId.Value));

        if (teamIds.Count > 0 || sportIds.Count > 0)
            query = query.Where(x =>
                (x.TeamId != null && teamIds.Contains(x.TeamId.Value))
                || (x.SportId != null && sportIds.Contains(x.SportId.Value)));

        return await query
            .OrderByDescending(x => x.Video.PublishedAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<OpinionSportFacet>> GetApprovedSportsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await Context.Opinions
            .AsNoTracking()
            .Where(x => x.Status == OpinionStatus.Approved && x.Sport != null)
            .GroupBy(x => new { x.Sport!.Name, x.Sport.Slug })
            .Select(g => new { g.Key.Name, g.Key.Slug, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new OpinionSportFacet(x.Name, x.Slug, x.Count))
            .ToList();
    }

    public async Task<List<OpinionTeamFacet>> GetApprovedTeamsAsync(CancellationToken cancellationToken = default)
    {
        // Gruplama veritabanında yapılıyor; takım adı ve branşı anahtarın parçası.
        var rows = await Context.Opinions
            .AsNoTracking()
            .Where(x => x.Status == OpinionStatus.Approved && x.Team != null)
            .GroupBy(x => new
            {
                x.Team!.Name,
                x.Team.Slug,
                SportSlug = (string?)x.Team.Sport.Slug,
                x.Team.LogoUrl
            })
            .Select(g => new
            {
                g.Key.Name,
                g.Key.Slug,
                g.Key.SportSlug,
                g.Key.LogoUrl,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new OpinionTeamFacet(x.Name, x.Slug, x.SportSlug, x.LogoUrl, x.Count))
            .ToList();
    }

    public async Task<Paged<Opinion>> GetPagedAsync(
        OpinionStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Opinions
            .AsNoTracking()
            .Include(x => x.Commentator)
            .Include(x => x.Video)
                .ThenInclude(x => x.Channel)
            .Include(x => x.Story)
            .AsQueryable();

        if (status is not null)
            query = query.Where(x => x.Status == status);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new Paged<Opinion>(items, total, page, pageSize);
    }

    public Task<Opinion?> GetWithContextAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Opinions
            .Include(x => x.Commentator)
            .Include(x => x.Video)
                .ThenInclude(x => x.Channel)
                    // Konuşmacıyı kanalın kadrosuna eklerken mevcut kadro gerekiyor.
                    .ThenInclude(x => x.RegularCommentators)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<Opinion>> GetByVideoAsync(Guid videoId, CancellationToken cancellationToken = default) =>
        Context.Opinions
            .Where(x => x.VideoId == videoId)
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> GetAwaitingQuoteCheckAsync(int take, CancellationToken cancellationToken = default) =>
        Context.Opinions
            .Where(x => x.Status == OpinionStatus.Pending
                        && x.QuoteCheck == QuoteCheckResult.NotChecked
                        && x.TimestampSeconds != null)
            // Eskiden yeniye: onay ekranında en çok bekleyen kayıt önce hazırlansın.
            .OrderBy(x => x.CreatedAt)
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task<int> CountByStatusAsync(OpinionStatus status, CancellationToken cancellationToken = default) =>
        Context.Opinions.CountAsync(x => x.Status == status, cancellationToken);
}
