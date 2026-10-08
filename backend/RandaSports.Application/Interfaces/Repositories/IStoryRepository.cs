using RandaSports.Application.Common.Wrappers;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

/// <summary>Görüşü habere bağlarken kullanılan hafif konu kaydı.</summary>
public sealed record StoryClusterRef(Guid Id, string ClusterKey, DateTimeOffset LastPublishedAt);

public interface IStoryRepository : IGenericRepository<Story>
{
    /// <summary>Kümeleme anahtarıyla açık konuyu bulur (takip edilen hâlde döner).</summary>
    Task<Story?> GetByClusterKeyAsync(string clusterKey, CancellationToken cancellationToken = default);

    /// <summary>Konuya bağlı haberlerin farklı kaynakları.</summary>
    Task<List<Guid>> GetSourceIdsAsync(Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>Konuyu haberleri ve kaynaklarıyla getirir (AI'a verilecek malzeme).</summary>
    Task<Story?> GetWithArticlesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Yazılmayı bekleyen konular: ya yeterli kaynağa ulaşmış ya da beklemesi dolmuş.</summary>
    Task<List<Guid>> GetPendingWriteIdsAsync(
        int take,
        int maxAttempts,
        int minSourceCount,
        DateTimeOffset singleSourceCutoff,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, Guid excludeStoryId, CancellationToken cancellationToken = default);

    /// <summary>Yayına hazır konular: metni yazılmış olanlar, en yeniden eskiye.</summary>
    Task<Paged<Story>> GetPagedAsync(
        int page,
        int pageSize,
        string? sportSlug = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>Haber sayfası için konuyu kaynaklarıyla getirir.</summary>
    Task<Story?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Kişiye özel akış: takip edilen takımların geçtiği ya da takip edilen
    /// branşlardaki haberler. İki ölçütten biri yetiyor — takımı takip eden
    /// kullanıcı o takımın haberini branşı seçmemiş olsa da görüyor.
    /// </summary>
    Task<List<Story>> GetForFollowedAsync(
        List<Guid> teamIds,
        List<Guid> sportIds,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Yorumcu görüşünü habere bağlamak için son günlerin yazılmış konularının
    /// kümeleme anahtarlarını verir; eşleştirme bellekte yapılıyor.
    /// </summary>
    Task<List<StoryClusterRef>> GetRecentClusterRefsAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default);
}
