namespace RandaSports.Infrastructure.Sports;

public sealed class ApiFootballOptions
{
    public const string SectionName = "ApiFootball";

    public string BaseUrl { get; set; } = "https://v3.football.api-sports.io/";

    /// <summary>Ücretsiz plan: spor başına günde 100 istek.</summary>
    public int DailyLimit { get; set; } = 100;

    /// <summary>Takip edilen lig (API-Football lig kimliği). Süper Lig varsayılan.</summary>
    public string LeagueId { get; set; } = "203";

    /// <summary>Sezon başlangıç yılı: 2026-27 sezonu için 2026.</summary>
    public int Season { get; set; } = 2026;
}
