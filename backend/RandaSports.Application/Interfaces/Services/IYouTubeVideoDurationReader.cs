namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Besleme video süresini vermiyor; süreyi ayrıca okuyoruz. Süre hem canlı yayını
/// ayıklamak hem de kısa videoları öne almak için gerekiyor.
/// </summary>
public interface IYouTubeVideoDurationReader
{
    /// <summary>
    /// Video kimliği → saniye. Canlı yayın ve süresi okunamayan videolar için 0 döner.
    /// Hiç okunamayan video sözlükte yer almaz.
    /// </summary>
    Task<IReadOnlyDictionary<string, int>> GetDurationsAsync(
        IReadOnlyCollection<string> youTubeVideoIds,
        CancellationToken cancellationToken = default);
}
