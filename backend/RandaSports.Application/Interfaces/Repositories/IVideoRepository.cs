using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

/// <param name="OpinionCount">Bu videodan çıkan görüş sayısı; sıfırsa model bir şey bulamamış.</param>
public sealed record VideoStatusRow(
    Guid Id,
    string Title,
    string ChannelName,
    string YouTubeVideoId,
    DateTimeOffset PublishedAt,
    int? DurationSeconds,
    string Status,
    int Attempts,
    string? Error,
    DateTimeOffset? ProcessedAt,
    int OpinionCount);

public interface IVideoRepository : IGenericRepository<Video>
{
    /// <summary>Verilen YouTube kimliklerinden hangileri kayıtlı — aynı videoyu iki kez eklememek için.</summary>
    Task<HashSet<string>> GetExistingVideoIdsAsync(
        IReadOnlyCollection<string> youTubeVideoIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Görüşleri çıkarılmayı bekleyen videolar.
    /// </summary>
    /// <param name="preferShort">
    /// true: kısa videolar önce. Shorts genelde tek bir yorumcunun tek görüşü ve
    /// modele neredeyse bedava; aynı kotayla çok daha fazla görüş çıkıyor.
    /// false: en yeni video önce.
    /// </param>
    Task<List<Guid>> GetPendingExtractionIdsAsync(
        int take,
        int maxAttempts,
        bool preferShort,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Boru hattının neresinde takıldığını görmek için: videolar durumlarıyla,
    /// hatalarıyla ve ürettikleri görüş sayısıyla.
    /// </summary>
    Task<List<VideoStatusRow>> GetStatusRowsAsync(
        string? status,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>Durum başına video sayısı; tek bakışta tıkanıklığı gösteriyor.</summary>
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken = default);

    /// <summary>Videoyu kanalıyla ve kanalın yorumcularıyla getirir (konuşmacı doğrulaması için).</summary>
    Task<Video?> GetWithChannelAsync(Guid id, CancellationToken cancellationToken = default);
}
