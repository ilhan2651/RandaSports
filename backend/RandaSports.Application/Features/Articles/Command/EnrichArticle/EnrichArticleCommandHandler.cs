using System.Globalization;
using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Articles.Command.EnrichArticle;

public sealed class EnrichArticleCommandHandler(
    IArticleRepository articleRepository,
    IArticleContentExtractor contentExtractor,
    IGeminiClient geminiClient,
    ISportDictionary sportDictionary,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<EnrichArticleCommandHandler> logger)
    : IRequestHandler<EnrichArticleCommand, Result<bool>>
{
    private const int MinContentLength = 300;

    /// <summary>Article.ImageUrl kolonunun sınırı.</summary>
    private const int ImageUrlMaxLength = 1000;

    private static readonly CultureInfo Turkish = new("tr-TR");

    public async Task<Result<bool>> Handle(EnrichArticleCommand request, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetForEnrichAsync(request.ArticleId, cancellationToken);
        if (article is null)
            return Result<bool>.Fail("Haber bulunamadı.", HttpStatusCode.NotFound);

        article.AiAttempts++;

        var extracted = await contentExtractor.ExtractAsync(article.Url, cancellationToken);

        if (extracted is null || extracted.Text.Length < MinContentLength)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("İçerik çıkarılamadı, atlandı: {Url}", article.Url);
            return Result<bool>.Ok(false, "İçerik çıkarılamadı.");
        }

        var content = extracted.Text;

        // ESPN gibi beslemesinde görsel olmayan kaynaklarda sayfanın paylaşım görseli
        // kullanılıyor. Besleme görsel verdiyse ona dokunmuyoruz: oradaki görsel
        // habere özel, paylaşım görseli bazen genel bir kapak oluyor.
        if (string.IsNullOrWhiteSpace(article.ImageUrl) && !string.IsNullOrWhiteSpace(extracted.ImageUrl))
            article.ImageUrl = Truncate(extracted.ImageUrl, ImageUrlMaxLength);

        var sports = await sportDictionary.GetAllAsync(cancellationToken);

        var prompt = ArticlePromptBuilder.Build(
            article.Title,
            article.Excerpt,
            content,
            sports,
            article.Source?.Language);

        var json = await geminiClient.GenerateJsonAsync(prompt, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Fail("Model cevap vermedi.", HttpStatusCode.BadGateway);
        }

        AiArticleAnalysis? analysis;
        try
        {
            analysis = JsonSerializer.Deserialize<AiArticleAnalysis>(json);
        }
        catch (JsonException ex)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogWarning(ex, "Model cevabı okunamadı: {ArticleId}", article.Id);
            return Result<bool>.Fail("Model cevabı okunamadı.", HttpStatusCode.BadGateway);
        }

        if (analysis is null || string.IsNullOrWhiteSpace(analysis.Summary))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Fail("Model boş özet döndürdü.", HttpStatusCode.BadGateway);
        }

        var verified = analysis with
        {
            Quotes = VerifyQuotes(analysis.Quotes, content, article.Id),
            Sport = await VerifySportAsync(analysis.Sport, article.Id, cancellationToken)
        };

        article.AiHeadline = verified.Headline;
        article.AiSummary = verified.Summary;
        article.AiBody = verified.Body;
        article.AiIsLive = verified.IsLive;
        article.AiAnalysis = verified.Analysis;
        article.AiDataJson = JsonSerializer.Serialize(verified);
        article.AiProcessedAt = timeProvider.GetUtcNow();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Haber zenginleştirildi: {Title}", article.Title);
        return Result<bool>.Ok(true);
    }

    /// <summary>
    /// Model listede olmayan bir branş uydurursa (ör. "ufc", "nfl") alan boşaltılıyor:
    /// kümeleme adımı yanlış slug'la çalışmasın.
    /// </summary>
    private async Task<string?> VerifySportAsync(string? slug, Guid articleId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var sport = await sportDictionary.FindAsync(slug, cancellationToken);
        if (sport is not null)
            return sport.Slug;

        logger.LogWarning("Kayıtlı olmayan branş atıldı ({ArticleId}): {Sport}", articleId, slug);
        return null;
    }

    /// <summary>Modelin uydurmasını engeller: alıntı orijinal metinde birebir geçmiyorsa atılır.</summary>
    private List<AiQuote> VerifyQuotes(List<AiQuote> quotes, string content, Guid articleId)
    {
        if (quotes.Count == 0)
            return [];

        var normalizedContent = Normalize(content);
        var verified = new List<AiQuote>();

        foreach (var quote in quotes)
        {
            if (string.IsNullOrWhiteSpace(quote.Text))
                continue;

            if (normalizedContent.Contains(Normalize(quote.Text), StringComparison.Ordinal))
            {
                verified.Add(quote);
                continue;
            }

            logger.LogWarning("Doğrulanamayan alıntı atıldı ({ArticleId}): {Quote}", articleId, quote.Text);
        }

        return verified;
    }

    private static string Normalize(string value)
    {
        var normalized = value.ToLower(Turkish);
        return string.Concat(normalized.Where(char.IsLetterOrDigit));
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
