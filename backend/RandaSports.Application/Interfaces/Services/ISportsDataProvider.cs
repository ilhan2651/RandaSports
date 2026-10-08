namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Spor verisi sağlayıcısı. Şu an API-Football, ileride başkası olabilir:
/// uygulamanın geri kalanı bu arayüzden ötesini bilmiyor.
/// </summary>
public interface ISportsDataProvider
{
    string Name { get; }

    Task<ProviderLeague?> GetLeagueAsync(
        string leagueExternalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderTeam>> GetTeamsAsync(
        string leagueExternalId,
        int season,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderPlayer>> GetSquadAsync(
        string teamExternalId,
        CancellationToken cancellationToken = default);

    /// <summary>Oyuncunun künyesi ve sezon istatistikleri tek çağrıda gelir.</summary>
    Task<(ProviderPlayer Player, ProviderPlayerStats? Stats)?> GetPlayerAsync(
        string playerExternalId,
        int season,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderFixture>> GetFixturesAsync(
        string leagueExternalId,
        int season,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProviderStanding>> GetStandingsAsync(
        string leagueExternalId,
        int season,
        CancellationToken cancellationToken = default);

    /// <summary>Tek çağrıda o an sahadaki tüm maçlar. Kota açısından en verimli uç.</summary>
    Task<IReadOnlyList<ProviderFixture>> GetLiveFixturesAsync(
        string? leagueExternalId = null,
        CancellationToken cancellationToken = default);
}
