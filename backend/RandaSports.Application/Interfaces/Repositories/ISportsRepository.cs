using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

/// <summary>Spor verisi senkronu için takip edilen (tracked) okumalar.</summary>
public interface ISportsRepository
{
    Task<Guid?> GetSportIdBySlugAsync(string sportSlug, CancellationToken cancellationToken = default);

    Task<Competition?> GetCompetitionByExternalIdAsync(
        string externalId,
        CancellationToken cancellationToken = default);

    Task<Season?> GetSeasonAsync(
        Guid competitionId,
        string externalId,
        CancellationToken cancellationToken = default);

    Task<Season?> GetCurrentSeasonAsync(
        Guid competitionId,
        CancellationToken cancellationToken = default);

    /// <summary>Branştaki tüm takımlar; eşleme sözlüğü bundan kuruluyor.</summary>
    Task<List<Team>> GetTeamsBySportAsync(Guid sportId, CancellationToken cancellationToken = default);

    Task<Team?> GetTeamAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<List<Team>> GetTeamsByExternalIdsAsync(
        IReadOnlyCollection<string> externalIds,
        CancellationToken cancellationToken = default);

    /// <summary>Kadrosu en uzun süredir tazelenmemiş lig takımları.</summary>
    Task<List<Guid>> GetTeamIdsForSquadSyncAsync(
        string competitionExternalId,
        int take,
        DateTimeOffset staleBefore,
        CancellationToken cancellationToken = default);

    Task<Athlete?> GetAthleteAsync(Guid athleteId, CancellationToken cancellationToken = default);

    Task<List<Athlete>> GetAthletesByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<List<Athlete>> GetAthletesByExternalIdsAsync(
        IReadOnlyCollection<string> externalIds,
        CancellationToken cancellationToken = default);

    /// <summary>İstatistiği en eski olan oyuncular önce gelir; hiç çekilmemişler en başta.</summary>
    Task<List<Guid>> GetAthleteIdsForStatsSyncAsync(
        int take,
        DateTimeOffset staleBefore,
        CancellationToken cancellationToken = default);

    Task<HashSet<string>> GetAthleteSlugsAsync(Guid sportId, CancellationToken cancellationToken = default);

    /// <summary>Maçları katılımcılarıyla birlikte getirir (güncellenecekleri için takip edilir).</summary>
    Task<List<Event>> GetEventsByExternalIdsAsync(
        IReadOnlyCollection<string> externalIds,
        CancellationToken cancellationToken = default);

    Task<List<Standing>> GetStandingsBySeasonAsync(Guid seasonId, CancellationToken cancellationToken = default);

    Task AddCompetitionAsync(Competition competition, CancellationToken cancellationToken = default);
    Task AddSeasonAsync(Season season, CancellationToken cancellationToken = default);
    Task AddTeamAsync(Team team, CancellationToken cancellationToken = default);
    Task AddAthleteAsync(Athlete athlete, CancellationToken cancellationToken = default);
    Task AddEventAsync(Event sportsEvent, CancellationToken cancellationToken = default);
    Task AddStandingAsync(Standing standing, CancellationToken cancellationToken = default);
}
