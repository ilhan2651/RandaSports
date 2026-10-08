using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

/// <param name="StoryCount">Yayınlanmış haber sayısı — menüde boş branş göstermemek için.</param>
/// <param name="HasCompetitions">
/// Branşın aktif ligi var mı. MMA, boks, e-spor gibi branşlarda lig yok; fikstür ve
/// puan durumu sekmeleri o branşlarda hiç açılmıyor.
/// </param>
public sealed record SportSummary(
    string Name,
    string Slug,
    int DisplayOrder,
    int StoryCount,
    bool HasCompetitions);

/// <summary>Ekranların okuduğu sorgular; hepsi takip edilmeyen (AsNoTracking) okuma.</summary>
public interface ISportsReadRepository
{
    /// <summary>Menü ve filtreler için branş listesi, gösterim sırasına göre.</summary>
    Task<List<SportSummary>> GetSportsAsync(CancellationToken cancellationToken = default);

    /// <summary>Slug verilmezse branşın ilk aktif ligi döner.</summary>
    Task<Competition?> GetCompetitionAsync(
        string? competitionSlug,
        string sportSlug,
        CancellationToken cancellationToken = default);

    Task<Season?> GetCurrentSeasonAsync(Guid competitionId, CancellationToken cancellationToken = default);

    Task<List<Standing>> GetStandingsAsync(Guid seasonId, CancellationToken cancellationToken = default);

    Task<List<Event>> GetFixturesAsync(
        string sportSlug,
        DateTimeOffset from,
        DateTimeOffset to,
        string? teamSlug = null,
        CancellationToken cancellationToken = default);

    /// <summary>Takımın son oynadığı ya da yaklaşan maçları.</summary>
    Task<List<Event>> GetTeamFixturesAsync(
        Guid teamId,
        bool upcoming,
        int take,
        CancellationToken cancellationToken = default);

    Task<Team?> GetTeamBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Standing?> GetTeamStandingAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<List<Athlete>> GetSquadAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<Athlete?> GetAthleteBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Takip listesi kaydedilirken seçilen kimliklerin gerçekliğini doğruluyor.</summary>
    Task<List<Team>> GetTeamsByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);

    Task<List<Sport>> GetSportsByIdsAsync(List<Guid> ids, CancellationToken cancellationToken = default);

    Task<Sport?> GetSportBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Tüm branşlar, kimlikleriyle. Görüş çıkarımı modelin verdiği adresi buna göre doğruluyor.</summary>
    Task<List<Sport>> GetAllSportsAsync(CancellationToken cancellationToken = default);

    /// <summary>Logosu olmayan takımlar — arka plan işi bunları sırayla deniyor.</summary>
    Task<List<Team>> GetTeamsWithoutLogoAsync(int take, CancellationToken cancellationToken = default);

    /// <summary>Yönetim ekranı için tüm takımlar, logo durumuyla.</summary>
    Task<List<Team>> GetAllTeamsAsync(CancellationToken cancellationToken = default);
}
