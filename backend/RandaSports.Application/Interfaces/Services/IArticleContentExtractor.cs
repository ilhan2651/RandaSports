namespace RandaSports.Application.Interfaces.Services;

/// <param name="Text">Haberin gövde metni.</param>
/// <param name="ImageUrl">
/// Sayfanın paylaşım görseli (og:image). Beslemesi görsel vermeyen kaynaklarda —
/// ESPN gibi — haberin görseli buradan geliyor; besleme görsel veriyorsa o kalıyor.
/// </param>
public sealed record ExtractedArticle(string Text, string? ImageUrl);

public interface IArticleContentExtractor
{
    /// <summary>Haber sayfasının gövde metnini ve paylaşım görselini çıkarır. Çıkaramazsa null döner.</summary>
    Task<ExtractedArticle?> ExtractAsync(string url, CancellationToken cancellationToken = default);
}
