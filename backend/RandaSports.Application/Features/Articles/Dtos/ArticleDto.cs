using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Articles.Dtos;

/// <param name="AiAttempts">
/// Kaç kez AI'a verilmeye çalışıldı. Sayı artmış ama AiProcessedAt boşsa haber
/// sırasını bekliyor değil, deneniyor ve başarısız oluyor demektir (çoğunlukla
/// sayfadan metin çıkarılamıyor).
/// </param>
public sealed record ArticleDto(
    Guid Id,
    string Title,
    string? AiHeadline,
    string? Excerpt,
    string? AiSummary,
    string? AiBody,
    string? AiAnalysis,
    bool AiIsLive,
    string Url,
    string? ImageUrl,
    string? Author,
    DateTimeOffset PublishedAt,
    string SourceName,
    Guid? SportId,
    string? SportName,
    int AiAttempts,
    DateTimeOffset? AiProcessedAt);

public static class ArticleMappings
{
    public static ArticleDto ToDto(this Article article) => new(
        article.Id,
        article.Title,
        article.AiHeadline,
        article.Excerpt,
        article.AiSummary,
        article.AiBody,
        article.AiAnalysis,
        article.AiIsLive,
        article.Url,
        article.ImageUrl,
        article.Author,
        article.PublishedAt,
        article.Source.Name,
        article.SportId,
        article.Sport?.Name,
        article.AiAttempts,
        article.AiProcessedAt);
}
