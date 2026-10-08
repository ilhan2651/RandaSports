namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Sağlayıcıdan gelen ham veriyi kendi dilimize çeviren ara modeller.
/// Sağlayıcı değişirse entity'ler değil, sadece istemci değişiyor.
/// </summary>
public sealed record ProviderTeam(
    string ExternalId,
    string Name,
    string? Country,
    string? LogoUrl,
    bool IsNational);

public sealed record ProviderPlayer(
    string ExternalId,
    string FullName,
    string? FirstName,
    string? LastName,
    string? Position,
    int? ShirtNumber,
    DateOnly? BirthDate,
    string? BirthPlace,
    string? Nationality,
    int? HeightCm,
    int? WeightKg,
    string? PhotoUrl);

/// <summary>
/// Oyuncunun tek bir turnuvadaki performansı: Süper Lig, Şampiyonlar Ligi ve
/// millî takım ayrı satırlar hâlinde geliyor, toplayıp kaybetmiyoruz.
/// </summary>
public sealed record ProviderCompetitionStat(
    string? LeagueExternalId,
    string? LeagueName,
    string? TeamExternalId,
    string? TeamName,
    bool IsNationalTeam,
    int Appearances,
    int Lineups,
    int Minutes,
    int Goals,
    int Assists,
    int ShotsTotal,
    int ShotsOn,
    int PassesTotal,
    int PassesKey,
    int? PassAccuracy,
    int DuelsTotal,
    int DuelsWon,
    int DribblesAttempts,
    int DribblesSuccess,
    int FoulsCommitted,
    int YellowCards,
    int RedCards,
    double? Rating);

public sealed record ProviderPlayerStats(
    string PlayerExternalId,
    int Season,
    IReadOnlyList<ProviderCompetitionStat> Competitions)
{
    public int Appearances => Competitions.Sum(x => x.Appearances);
    public int Minutes => Competitions.Sum(x => x.Minutes);
    public int Goals => Competitions.Sum(x => x.Goals);
    public int Assists => Competitions.Sum(x => x.Assists);

    /// <summary>Reyting toplanmaz; en çok oynanan turnuvanınki alınır.</summary>
    public double? Rating => Competitions
        .Where(x => x.Rating is not null)
        .OrderByDescending(x => x.Minutes)
        .Select(x => x.Rating)
        .FirstOrDefault();

    /// <summary>Kulüp formasıyla oynanan turnuvalar.</summary>
    public IEnumerable<ProviderCompetitionStat> Club => Competitions.Where(x => !x.IsNationalTeam);

    public IEnumerable<ProviderCompetitionStat> National => Competitions.Where(x => x.IsNationalTeam);
}

public sealed record ProviderFixture(
    string ExternalId,
    DateTimeOffset StartTime,
    string StatusCode,
    string? StatusDetail,
    int? Elapsed,
    string? Round,
    string? Venue,
    string HomeTeamExternalId,
    string HomeTeamName,
    int? HomeScore,
    string AwayTeamExternalId,
    string AwayTeamName,
    int? AwayScore);

public sealed record ProviderStanding(
    string TeamExternalId,
    string TeamName,
    int Rank,
    int Played,
    int Won,
    int Drawn,
    int Lost,
    int Points,
    int GoalsFor,
    int GoalsAgainst,
    string? Form,
    string? GroupName);

public sealed record ProviderLeague(
    string ExternalId,
    string Name,
    string? Country,
    string? LogoUrl,
    int CurrentSeason);
