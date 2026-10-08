using RandaSports.Application.Common.Wrappers;
using RandaSports.Domain.Entities;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Interfaces.Repositories;

/// <param name="OpinionCount">Yalnızca onaylanmış görüşler sayılıyor.</param>
public sealed record OpinionSportFacet(
    string Name,
    string Slug,
    int OpinionCount);

public sealed record OpinionTeamFacet(
    string Name,
    string Slug,
    string? SportSlug,
    string? LogoUrl,
    int OpinionCount);

public interface IOpinionRepository : IGenericRepository<Opinion>
{
    /// <summary>Haber sayfasında gösterilecek onaylı görüşler.</summary>
    Task<List<Opinion>> GetApprovedByStoryAsync(Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Siteye açık görüş akışı: yalnızca onaylananlar, en yeniden eskiye.
    /// </summary>
    /// <param name="commentatorSlug">Tek bir yorumcuya daraltmak için.</param>
    /// <param name="teamSlug">Görüşün konusu olan takıma daraltmak için.</param>
    /// <param name="sportSlug">Branşa daraltmak için; görüşün branşı takımından geliyor.</param>
    /// <param name="since">Bu tarihten yeni görüşler; boşsa sınır yok.</param>
    Task<Paged<Opinion>> GetApprovedAsync(
        string? commentatorSlug,
        string? teamSlug,
        string? sportSlug,
        DateTimeOffset? since,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kişiye özel görüş akışı. İki süzgeç birden geçilmek zorunda: görüş hem takip
    /// edilen bir yorumcuya ait olacak, hem de takip edilen bir takım ya da branşla
    /// ilgili olacak. Seçim yapılmamış boyut süzmüyor — hiç yorumcu seçmemiş kullanıcı
    /// branşındaki herkesi görüyor.
    /// </summary>
    Task<List<Opinion>> GetForFollowedAsync(
        List<Guid> commentatorIds,
        List<Guid> teamIds,
        List<Guid> sportIds,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filtre çubuğu için: hakkında yayınlanmış görüş bulunan takımlar, görüş sayılarıyla.
    /// </summary>
    Task<List<OpinionTeamFacet>> GetApprovedTeamsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Filtre çubuğu için branş listesi. Görüşün branşı konu edilen takımdan geliyor;
    /// takımı olmayan görüş hiçbir branşa sayılmıyor.
    /// </summary>
    Task<List<OpinionSportFacet>> GetApprovedSportsAsync(CancellationToken cancellationToken = default);

    /// <summary>Yönetim ekranı: duruma göre sayfalı liste.</summary>
    Task<Paged<Opinion>> GetPagedAsync(
        OpinionStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Onay/ret için görüşü video ve yorumcusuyla getirir.</summary>
    Task<Opinion?> GetWithContextAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Aynı videodan daha önce çıkarılmış görüşler (tekrar çalıştırmada temizlenir).</summary>
    Task<List<Opinion>> GetByVideoAsync(Guid videoId, CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(OpinionStatus status, CancellationToken cancellationToken = default);
}
