namespace RandaSports.Application.Interfaces.Services;

/// <summary>Haber metnindeki takımları sözlükten eşleştirir. AI tahminine değil, kayıtlı listeye bakar.</summary>
public interface ITeamMatcher
{
    Task<List<MatchedTeam>> MatchAsync(string? title, string? body, CancellationToken cancellationToken = default);
}

/// <param name="InTitle">Başlıkta geçiyorsa haberin öznesi sayılır; sadece gövdede geçiyorsa değinmedir.</param>
public sealed record MatchedTeam(
    Guid Id,
    string Name,
    string Slug,
    Guid SportId,
    string SportSlug,
    bool InTitle);
