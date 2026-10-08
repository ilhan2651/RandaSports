using RandaSports.Application.Common.Wrappers;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

public interface IArticleRepository : IGenericRepository<Article>
{
    Task<Paged<Article>> GetPagedAsync(
        int page,
        int pageSize,
        Guid? sportId = null,
        string? search = null,
        CancellationToken cancellationToken = default);

    Task<List<Guid>> GetPendingAiArticleIdsAsync(
        int take,
        int maxAttempts,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetExistingUrlsAsync(
        IReadOnlyCollection<string> urls,
        CancellationToken cancellationToken = default);

    /// <summary>AI'dan geçmiş ama henüz bir konuya bağlanmamış haberler (eskiden yeniye).</summary>
    Task<List<Guid>> GetUnclusteredArticleIdsAsync(
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>Kümeleme için haberi kaynağı ve branşıyla getirir (takip edilen hâlde).</summary>
    Task<Article?> GetForClusteringAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Zenginleştirme için haberi kaynağıyla getirir: kaynağın dili prompt'u belirliyor,
    /// yabancı beslemede model metni Türkçeye çevirerek yazıyor.
    /// </summary>
    Task<Article?> GetForEnrichAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
