using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Sports;

public sealed class ApiFootballClient(
    HttpClient httpClient,
    IProviderQuota quota,
    IOptions<ApiSportsOptions> accountOptions,
    ILogger<ApiFootballClient> logger) : ISportsDataProvider
{
    public const string ProviderName = "api-football";

    private readonly ApiSportsOptions _account = accountOptions.Value;

    public string Name => ProviderName;

    public async Task<ProviderLeague?> GetLeagueAsync(
        string leagueExternalId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"leagues?id={leagueExternalId}", cancellationToken);
        if (response is null || response.Value.GetArrayLength() == 0)
            return null;

        var item = response.Value[0];
        var league = item.GetProperty("league");
        var country = item.TryGetProperty("country", out var countryNode)
            ? Text(countryNode, "name")
            : null;

        var currentSeason = 0;
        if (item.TryGetProperty("seasons", out var seasons))
        {
            foreach (var season in seasons.EnumerateArray())
            {
                if (season.TryGetProperty("current", out var current) && current.ValueKind == JsonValueKind.True)
                    currentSeason = Number(season, "year") ?? 0;
            }
        }

        return new ProviderLeague(
            (Number(league, "id") ?? 0).ToString(CultureInfo.InvariantCulture),
            Text(league, "name") ?? "",
            country,
            Text(league, "logo"),
            currentSeason);
    }

    public async Task<IReadOnlyList<ProviderTeam>> GetTeamsAsync(
        string leagueExternalId,
        int season,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"teams?league={leagueExternalId}&season={season}", cancellationToken);
        if (response is null)
            return [];

        var teams = new List<ProviderTeam>();

        foreach (var item in response.Value.EnumerateArray())
        {
            if (!item.TryGetProperty("team", out var team))
                continue;

            var id = Number(team, "id");
            var name = Text(team, "name");
            if (id is null || name is null)
                continue;

            teams.Add(new ProviderTeam(
                id.Value.ToString(CultureInfo.InvariantCulture),
                name,
                Text(team, "country"),
                Text(team, "logo"),
                team.TryGetProperty("national", out var national) && national.ValueKind == JsonValueKind.True));
        }

        return teams;
    }

    public async Task<IReadOnlyList<ProviderPlayer>> GetSquadAsync(
        string teamExternalId,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"players/squads?team={teamExternalId}", cancellationToken);
        if (response is null || response.Value.GetArrayLength() == 0)
            return [];

        if (!response.Value[0].TryGetProperty("players", out var players))
            return [];

        var squad = new List<ProviderPlayer>();

        foreach (var player in players.EnumerateArray())
        {
            var id = Number(player, "id");
            var name = Text(player, "name");
            if (id is null || name is null)
                continue;

            squad.Add(new ProviderPlayer(
                id.Value.ToString(CultureInfo.InvariantCulture),
                name,
                null,
                null,
                Text(player, "position"),
                Number(player, "number"),
                null,
                null,
                null,
                null,
                null,
                Text(player, "photo")));
        }

        return squad;
    }

    public async Task<(ProviderPlayer Player, ProviderPlayerStats? Stats)?> GetPlayerAsync(
        string playerExternalId,
        int season,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"players?id={playerExternalId}&season={season}", cancellationToken);
        if (response is null || response.Value.GetArrayLength() == 0)
            return null;

        var item = response.Value[0];
        if (!item.TryGetProperty("player", out var player))
            return null;

        var id = Number(player, "id");
        var name = Text(player, "name");
        if (id is null || name is null)
            return null;

        var birth = player.TryGetProperty("birth", out var birthNode) ? birthNode : default;

        var profile = new ProviderPlayer(
            id.Value.ToString(CultureInfo.InvariantCulture),
            name,
            Text(player, "firstname"),
            Text(player, "lastname"),
            null,
            null,
            ParseDate(birth.ValueKind == JsonValueKind.Object ? Text(birth, "date") : null),
            birth.ValueKind == JsonValueKind.Object ? Text(birth, "place") : null,
            Text(player, "nationality"),
            ParseMeasure(Text(player, "height")),
            ParseMeasure(Text(player, "weight")),
            Text(player, "photo"));

        var stats = BuildStats(item, profile.ExternalId, season);

        return (profile, stats);
    }

    public async Task<IReadOnlyList<ProviderFixture>> GetFixturesAsync(
        string leagueExternalId,
        int season,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var path = $"fixtures?league={leagueExternalId}&season={season}" +
                   $"&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        var response = await GetAsync(path, cancellationToken);
        return response is null ? [] : ReadFixtures(response.Value);
    }

    public async Task<IReadOnlyList<ProviderStanding>> GetStandingsAsync(
        string leagueExternalId,
        int season,
        CancellationToken cancellationToken = default)
    {
        var response = await GetAsync($"standings?league={leagueExternalId}&season={season}", cancellationToken);
        if (response is null || response.Value.GetArrayLength() == 0)
            return [];

        if (!response.Value[0].TryGetProperty("league", out var league)
            || !league.TryGetProperty("standings", out var groups))
            return [];

        var rows = new List<ProviderStanding>();

        foreach (var group in groups.EnumerateArray())
        {
            foreach (var row in group.EnumerateArray())
            {
                if (!row.TryGetProperty("team", out var team))
                    continue;

                var teamId = Number(team, "id");
                if (teamId is null)
                    continue;

                var all = row.TryGetProperty("all", out var allNode) ? allNode : default;
                var goals = all.ValueKind == JsonValueKind.Object && all.TryGetProperty("goals", out var goalsNode)
                    ? goalsNode
                    : default;

                rows.Add(new ProviderStanding(
                    teamId.Value.ToString(CultureInfo.InvariantCulture),
                    Text(team, "name") ?? "",
                    Number(row, "rank") ?? 0,
                    all.ValueKind == JsonValueKind.Object ? Number(all, "played") ?? 0 : 0,
                    all.ValueKind == JsonValueKind.Object ? Number(all, "win") ?? 0 : 0,
                    all.ValueKind == JsonValueKind.Object ? Number(all, "draw") ?? 0 : 0,
                    all.ValueKind == JsonValueKind.Object ? Number(all, "lose") ?? 0 : 0,
                    Number(row, "points") ?? 0,
                    goals.ValueKind == JsonValueKind.Object ? Number(goals, "for") ?? 0 : 0,
                    goals.ValueKind == JsonValueKind.Object ? Number(goals, "against") ?? 0 : 0,
                    Text(row, "form"),
                    Text(row, "group")));
            }
        }

        return rows;
    }

    public async Task<IReadOnlyList<ProviderFixture>> GetLiveFixturesAsync(
        string? leagueExternalId = null,
        CancellationToken cancellationToken = default)
    {
        var path = leagueExternalId is null ? "fixtures?live=all" : $"fixtures?live={leagueExternalId}";

        var response = await GetAsync(path, cancellationToken);
        return response is null ? [] : ReadFixtures(response.Value);
    }

    private static List<ProviderFixture> ReadFixtures(JsonElement response)
    {
        var fixtures = new List<ProviderFixture>();

        foreach (var item in response.EnumerateArray())
        {
            if (!item.TryGetProperty("fixture", out var fixture)
                || !item.TryGetProperty("teams", out var teams))
                continue;

            var id = Number(fixture, "id");
            var home = teams.TryGetProperty("home", out var homeNode) ? homeNode : default;
            var away = teams.TryGetProperty("away", out var awayNode) ? awayNode : default;

            if (id is null || home.ValueKind != JsonValueKind.Object || away.ValueKind != JsonValueKind.Object)
                continue;

            var status = fixture.TryGetProperty("status", out var statusNode) ? statusNode : default;
            var venue = fixture.TryGetProperty("venue", out var venueNode) ? venueNode : default;
            var goals = item.TryGetProperty("goals", out var goalsNode) ? goalsNode : default;
            var league = item.TryGetProperty("league", out var leagueNode) ? leagueNode : default;

            var startText = Text(fixture, "date");
            var startTime = DateTimeOffset.TryParse(startText, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed)
                ? parsed.ToUniversalTime()
                : DateTimeOffset.UtcNow;

            fixtures.Add(new ProviderFixture(
                id.Value.ToString(CultureInfo.InvariantCulture),
                startTime,
                (status.ValueKind == JsonValueKind.Object ? Text(status, "short") : null) ?? "NS",
                status.ValueKind == JsonValueKind.Object ? Text(status, "long") : null,
                status.ValueKind == JsonValueKind.Object ? Number(status, "elapsed") : null,
                league.ValueKind == JsonValueKind.Object ? Text(league, "round") : null,
                venue.ValueKind == JsonValueKind.Object ? Text(venue, "name") : null,
                (Number(home, "id") ?? 0).ToString(CultureInfo.InvariantCulture),
                Text(home, "name") ?? "",
                goals.ValueKind == JsonValueKind.Object ? Number(goals, "home") : null,
                (Number(away, "id") ?? 0).ToString(CultureInfo.InvariantCulture),
                Text(away, "name") ?? "",
                goals.ValueKind == JsonValueKind.Object ? Number(goals, "away") : null));
        }

        return fixtures;
    }

    /// <summary>
    /// Sağlayıcı her takım + turnuva için ayrı bir satır gönderiyor:
    /// kulüp ligi, Avrupa kupası ve millî takım ayrı ayrı korunuyor.
    /// </summary>
    private static ProviderPlayerStats? BuildStats(JsonElement item, string playerExternalId, int season)
    {
        if (!item.TryGetProperty("statistics", out var statistics) || statistics.GetArrayLength() == 0)
            return null;

        var competitions = new List<ProviderCompetitionStat>();

        foreach (var entry in statistics.EnumerateArray())
        {
            var team = entry.TryGetProperty("team", out var teamNode) ? teamNode : default;
            var league = entry.TryGetProperty("league", out var leagueNode) ? leagueNode : default;
            var games = entry.TryGetProperty("games", out var gamesNode) ? gamesNode : default;
            var goals = entry.TryGetProperty("goals", out var goalsNode) ? goalsNode : default;
            var shots = entry.TryGetProperty("shots", out var shotsNode) ? shotsNode : default;
            var passes = entry.TryGetProperty("passes", out var passesNode) ? passesNode : default;
            var duels = entry.TryGetProperty("duels", out var duelsNode) ? duelsNode : default;
            var dribbles = entry.TryGetProperty("dribbles", out var dribblesNode) ? dribblesNode : default;
            var fouls = entry.TryGetProperty("fouls", out var foulsNode) ? foulsNode : default;
            var cards = entry.TryGetProperty("cards", out var cardsNode) ? cardsNode : default;

            var leagueName = Text(league, "name");
            var leagueCountry = Text(league, "country");

            double? rating = null;
            if (double.TryParse(Text(games, "rating"), NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var parsedRating))
                rating = parsedRating;

            competitions.Add(new ProviderCompetitionStat(
                Number(league, "id")?.ToString(CultureInfo.InvariantCulture),
                leagueName,
                Number(team, "id")?.ToString(CultureInfo.InvariantCulture),
                Text(team, "name"),
                IsNationalCompetition(leagueName, leagueCountry),
                Number(games, "appearences") ?? Number(games, "appearances") ?? 0,
                Number(games, "lineups") ?? 0,
                Number(games, "minutes") ?? 0,
                Number(goals, "total") ?? 0,
                Number(goals, "assists") ?? 0,
                Number(shots, "total") ?? 0,
                Number(shots, "on") ?? 0,
                Number(passes, "total") ?? 0,
                Number(passes, "key") ?? 0,
                Number(passes, "accuracy"),
                Number(duels, "total") ?? 0,
                Number(duels, "won") ?? 0,
                Number(dribbles, "attempts") ?? 0,
                Number(dribbles, "success") ?? 0,
                Number(fouls, "committed") ?? 0,
                Number(cards, "yellow") ?? 0,
                Number(cards, "red") ?? 0,
                rating));
        }

        return competitions.Count == 0
            ? null
            : new ProviderPlayerStats(playerExternalId, season, competitions);
    }

    /// <summary>
    /// Millî takım turnuvalarının ülkesi "World" geliyor; ad kontrolü de yedek.
    /// Kesin ayrım senkron katmanında takımın IsNational alanıyla düzeltilebilir.
    /// </summary>
    private static bool IsNationalCompetition(string? leagueName, string? country)
    {
        if (string.Equals(country, "World", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.IsNullOrWhiteSpace(leagueName))
            return false;

        string[] markers = ["Nations League", "World Cup", "Euro", "Friendlies", "Qualification"];
        return markers.Any(marker => leagueName.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Her çağrı önce kotadan izin alıyor; izin yoksa istek hiç gitmiyor.</summary>
    private async Task<JsonElement?> GetAsync(string path, CancellationToken cancellationToken)
    {
        if (!_account.HasKey)
        {
            logger.LogWarning("ApiSports:ApiKey boş, çağrı yapılmadı.");
            return null;
        }

        if (!await quota.TryConsumeAsync(ProviderName, 1, cancellationToken))
            return null;

        using var response = await httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("API-Football {Status} döndü: {Path}", (int)response.StatusCode, path);
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = document.RootElement;

        if (root.TryGetProperty("errors", out var errors) && HasError(errors))
        {
            logger.LogWarning("API-Football hata döndü ({Path}): {Errors}", path, errors.GetRawText());
            return null;
        }

        if (!root.TryGetProperty("response", out var payload) || payload.ValueKind != JsonValueKind.Array)
            return null;

        // JsonDocument dispose olduğunda element geçersizleşiyor; kopyasını döndürüyoruz.
        return payload.Clone();
    }

    private static bool HasError(JsonElement errors) =>
        errors.ValueKind switch
        {
            JsonValueKind.Array => errors.GetArrayLength() > 0,
            JsonValueKind.Object => errors.EnumerateObject().Any(),
            _ => false
        };

    private static string? Text(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int? Number(JsonElement element, string property)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(property, out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetInt32(out var number) ? number : null,
            JsonValueKind.String => int.TryParse(value.GetString(), out var parsed) ? parsed : null,
            _ => null
        };
    }

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;

    /// <summary>"183 cm" ve "75 kg" gibi değerlerden sayıyı alır.</summary>
    private static int? ParseMeasure(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var digits = new string(value.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var parsed) ? parsed : null;
    }
}
