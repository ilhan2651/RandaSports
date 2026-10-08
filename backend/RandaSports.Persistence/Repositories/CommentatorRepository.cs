using Microsoft.EntityFrameworkCore;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Repositories;

public class CommentatorRepository(RandaSportsDbContext context)
    : GenericRepository<Commentator>(context), ICommentatorRepository
{
    /// <summary>Onay ekranında kişi başına gösterilecek örnek görüş sayısı.</summary>
    private const int SampleSize = 2;

    public Task<List<Commentator>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        Context.Commentators
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

    public Task<Commentator?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        Context.Commentators
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);

    public Task<List<Commentator>> GetByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default) =>
        ids.Count == 0
            ? Task.FromResult(new List<Commentator>())
            : Context.Commentators.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);

    public async Task<List<RoleCandidate>> GetRoleCandidatesAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var rows = await Context.Commentators
            .AsNoTracking()
            .Where(x => x.PersonRole == PersonRole.Unknown && x.Opinions.Any())
            .OrderByDescending(x => x.Opinions.Count)
            .Take(take)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                VideoTitles = c.Opinions
                    .OrderByDescending(o => o.Video.PublishedAt)
                    .Select(o => o.Video.Title)
                    .Take(3)
                    .ToList(),
                Quotes = c.Opinions
                    .OrderByDescending(o => o.Video.PublishedAt)
                    .Select(o => o.Quote)
                    .Take(2)
                    .ToList(),
                Channels = c.Channels.Select(ch => ch.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new RoleCandidate(x.Id, x.FullName, x.VideoTitles, x.Quotes, x.Channels))
            .ToList();
    }

    public async Task<List<CommentatorEvidence>> GetVerificationEvidenceAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await Context.Commentators
            .AsNoTracking()
            .Where(x => !x.IsVerified)
            .Select(c => new
            {
                c.Id,
                c.FullName,
                DistinctVideos = c.Opinions.Select(o => o.VideoId).Distinct().Count(),
                DistinctChannels = c.Opinions.Select(o => o.Video.ChannelId).Distinct().Count(),
                AltbantVideos = c.Opinions
                    .Where(o => o.SpeakerSource == "altbant")
                    .Select(o => o.VideoId)
                    .Distinct()
                    .Count()
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new CommentatorEvidence(
                x.Id,
                x.FullName,
                x.DistinctVideos,
                x.DistinctChannels,
                x.AltbantVideos))
            .ToList();
    }

    public Task<List<Commentator>> GetVerifiedWithoutPhotoAsync(
        int take,
        CancellationToken cancellationToken = default) =>
        Context.Commentators
            .Where(x => x.IsVerified && x.PhotoUrl == null)
            .OrderByDescending(x => x.Opinions.Count)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<List<UnverifiedCommentator>> GetUnverifiedAsync(
        CancellationToken cancellationToken = default)
    {
        // Entity taşıyan kayda doğrudan projeksiyon EF'te çevrilemiyor; anonim tiple
        // çekip kayda bellekte dönüştürüyoruz (profil sorgusuyla aynı sebep).
        var rows = await Context.Commentators
            .AsNoTracking()
            .Where(x => !x.IsVerified)
            .Select(c => new
            {
                Commentator = c,
                OpinionCount = c.Opinions.Count,
                ApprovedCount = c.Opinions.Count(o => o.Status == OpinionStatus.Approved),
                LastOpinionAt = c.Opinions.Max(o => (DateTimeOffset?)o.Video.PublishedAt),
                ChannelNames = c.Channels.Select(ch => ch.Name).ToList(),
                Sample = c.Opinions
                    .OrderByDescending(o => o.Video.PublishedAt)
                    .Take(SampleSize)
                    .Select(o => new
                    {
                        o.Topic,
                        o.Quote,
                        VideoTitle = o.Video.Title,
                        o.Video.YouTubeVideoId,
                        o.TimestampSeconds
                    })
                    .ToList()
            })
            .OrderByDescending(x => x.LastOpinionAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new UnverifiedCommentator(
                x.Commentator,
                x.OpinionCount,
                x.ApprovedCount,
                x.LastOpinionAt,
                x.ChannelNames,
                [.. x.Sample.Select(o => new OpinionSample(
                    o.Topic,
                    o.Quote,
                    o.VideoTitle,
                    o.YouTubeVideoId,
                    o.TimestampSeconds))]))
            .ToList();
    }

    public Task<Commentator?> GetForReviewAsync(Guid id, CancellationToken cancellationToken = default) =>
        Context.Commentators
            .Include(x => x.Opinions)
            .Include(x => x.Channels)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<List<CommentatorProfile>> GetProfilesAsync(CancellationToken cancellationToken = default)
    {
        var rows = await BuildRows(Context.Commentators.AsNoTracking())
            .Where(x => x.OpinionCount > 0)
            .OrderByDescending(x => x.LastOpinionAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new CommentatorProfile(x.Commentator, x.OpinionCount, x.LastOpinionAt, x.SportSlugs))
            .ToList();
    }

    public async Task<CommentatorProfile?> GetProfileBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var row = await BuildRows(Context.Commentators.AsNoTracking().Where(c => c.Slug == slug))
            .FirstOrDefaultAsync(cancellationToken);

        return row is null
            ? null
            : new CommentatorProfile(row.Commentator, row.OpinionCount, row.LastOpinionAt, row.SportSlugs);
    }

    /// <summary>
    /// Sayım yayına çıkmış görüşler üzerinden; onay bekleyenler sayılmıyor.
    /// Projeksiyon anonim tipe yapılıyor: EF, içinde entity taşıyan özel bir kayda
    /// dönüştürüp sonra onun alanlarına göre filtrelemeyi SQL'e çeviremiyor.
    /// Kayda dönüşüm veritabanından döndükten sonra, bellekte yapılıyor.
    /// </summary>
    private static IQueryable<ProfileRow> BuildRows(IQueryable<Commentator> source) =>
        source.Select(c => new ProfileRow
        {
            Commentator = c,
            OpinionCount = c.Opinions.Count(o => o.Status == OpinionStatus.Approved),
            LastOpinionAt = c.Opinions
                .Where(o => o.Status == OpinionStatus.Approved)
                .Max(o => (DateTimeOffset?)o.Video.PublishedAt),

            // Branş görüşlerden türetiliyor: kayıttaki statik alan yalnızca tohum
            // listesindekilerde dolu, otomatik eklenenlerde hiç yok.
            SportSlugs = c.Opinions
                .Where(o => o.Status == OpinionStatus.Approved && o.Sport != null)
                .Select(o => o.Sport!.Slug)
                .Distinct()
                .ToList()
        });

    private sealed class ProfileRow
    {
        public required Commentator Commentator { get; init; }
        public required int OpinionCount { get; init; }
        public required DateTimeOffset? LastOpinionAt { get; init; }
        public required List<string> SportSlugs { get; init; }
    }
}
