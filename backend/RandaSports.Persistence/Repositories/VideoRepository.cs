using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class VideoRepository(RandaSportsDbContext context)
    : GenericRepository<Video>(context), IVideoRepository
{
    public async Task<HashSet<string>> GetExistingVideoIdsAsync(
        IReadOnlyCollection<string> youTubeVideoIds,
        CancellationToken cancellationToken = default)
    {
        if (youTubeVideoIds.Count == 0)
            return [];

        var found = await Context.Videos
            .AsNoTracking()
            .Where(x => youTubeVideoIds.Contains(x.YouTubeVideoId))
            .Select(x => x.YouTubeVideoId)
            .ToListAsync(cancellationToken);

        return found.ToHashSet(StringComparer.Ordinal);
    }

    public Task<List<Guid>> GetPendingExtractionIdsAsync(
        int take,
        int maxAttempts,
        bool preferShort,
        CancellationToken cancellationToken = default)
    {
        var pending = Context.Videos
            .AsNoTracking()
            .Where(x => x.ProcessingStatus == VideoProcessingStatus.Pending
                        && x.ProcessingAttempts < maxAttempts);

        // Süresi bilinmeyen video kısa sayılmıyor; sıranın sonuna değil, ortasına
        // düşsün diye uzun bir varsayılanla sıralanıyor.
        var ordered = preferShort
            ? pending
                .OrderBy(x => x.DurationSeconds ?? int.MaxValue)
                .ThenByDescending(x => x.PublishedAt)
            : pending
                .OrderByDescending(x => x.PublishedAt)
                .ThenBy(x => x.DurationSeconds ?? int.MaxValue);

        return ordered
            .Take(take)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VideoStatusRow>> GetStatusRowsAsync(
        string? status,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = Context.Videos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<VideoProcessingStatus>(status, ignoreCase: true, out var parsed))
            query = query.Where(x => x.ProcessingStatus == parsed);

        // Entity taşıyan kayda doğrudan projeksiyon EF'te çevrilemiyor; anonim tiple
        // çekip kayda bellekte dönüştürüyoruz.
        var rows = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(take)
            .Select(x => new
            {
                x.Id,
                x.Title,
                ChannelName = x.Channel.Name,
                x.YouTubeVideoId,
                x.PublishedAt,
                x.DurationSeconds,
                x.ProcessingStatus,
                x.ProcessingAttempts,
                x.ProcessingError,
                x.ProcessedAt,
                OpinionCount = x.Opinions.Count
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new VideoStatusRow(
                x.Id,
                x.Title,
                x.ChannelName,
                x.YouTubeVideoId,
                x.PublishedAt,
                x.DurationSeconds,
                x.ProcessingStatus.ToString(),
                x.ProcessingAttempts,
                x.ProcessingError,
                x.ProcessedAt,
                x.OpinionCount))
            .ToList();
    }

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await Context.Videos
            .AsNoTracking()
            .GroupBy(x => x.ProcessingStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(x => x.Status.ToString(), x => x.Count);
    }

    public Task<Video?> GetWithChannelAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Videos
            .Include(x => x.Channel)
                .ThenInclude(x => x.RegularCommentators)
            .Include(x => x.Channel)
                .ThenInclude(x => x.Sports)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
}
