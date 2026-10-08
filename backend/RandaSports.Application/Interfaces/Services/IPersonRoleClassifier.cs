using RandaSports.Domain.Enums;

namespace RandaSports.Application.Interfaces.Services;

/// <param name="VideoTitles">Modelin bağlam kurması için; kişinin göründüğü videolar.</param>
/// <param name="Quotes">Ne söylediği, rolünü ele veren en güçlü ipucu.</param>
public sealed record PersonRoleCandidate(
    string FullName,
    List<string> VideoTitles,
    List<string> Quotes,
    List<string> Channels);

public sealed record PersonRoleVerdict(string FullName, PersonRole Role, double Confidence, string? Reason);

/// <summary>
/// Kişinin rolünü belirler. Yüz tanıma değil: yalnızca adı, göründüğü videoların
/// başlıkları ve söyledikleri üzerinden karar veriliyor.
/// </summary>
public interface IPersonRoleClassifier
{
    Task<List<PersonRoleVerdict>> ClassifyAsync(
        IReadOnlyList<PersonRoleCandidate> candidates,
        CancellationToken cancellationToken = default);
}
