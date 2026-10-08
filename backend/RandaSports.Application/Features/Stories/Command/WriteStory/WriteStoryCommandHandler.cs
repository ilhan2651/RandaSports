using System.Net;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Text;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Stories.Command.WriteStory;

public sealed class WriteStoryCommandHandler(
    IStoryRepository storyRepository,
    IGeminiClient geminiClient,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<WriteStoryCommandHandler> logger)
    : IRequestHandler<WriteStoryCommand, Result<bool>>
{
    private const int MaxSources = 6;
    private const int HeadlineMaxLength = 300;
    private const int SummaryMaxLength = 2000;
    private const int SlugMaxLength = 120;
    private const int CategoryMaxLength = 50;

    public async Task<Result<bool>> Handle(WriteStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await storyRepository.GetWithArticlesAsync(request.StoryId, cancellationToken);
        if (story is null)
            return Result<bool>.Fail("Konu bulunamadı.", HttpStatusCode.NotFound);

        story.AiAttempts++;

        var articles = story.Articles
            .Where(x => !string.IsNullOrWhiteSpace(x.AiSummary))
            .OrderBy(x => x.PublishedAt)
            .Take(MaxSources)
            .ToList();

        if (articles.Count == 0)
        {
            story.NeedsRewrite = false;
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Ok(false, "Konuda yazılacak kaynak yok.");
        }

        var sources = articles
            .Select(x => new StorySourceInput(
                x.Source.Name,
                x.AiHeadline ?? x.Title,
                x.PublishedAt,
                x.AiSummary,
                x.AiBody,
                x.AiIsLive))
            .ToList();

        var prompt = StoryPromptBuilder.Build(sources);
        var json = await geminiClient.GenerateJsonAsync(prompt, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Fail("Model cevap vermedi.", HttpStatusCode.BadGateway);
        }

        AiStoryContent? content;
        try
        {
            content = JsonSerializer.Deserialize<AiStoryContent>(json);
        }
        catch (JsonException ex)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogWarning(ex, "Model cevabı okunamadı ({StoryId}).", story.Id);
            return Result<bool>.Fail("Model cevabı okunamadı.", HttpStatusCode.BadGateway);
        }

        if (content is null || string.IsNullOrWhiteSpace(content.Summary) || string.IsNullOrWhiteSpace(content.Body))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<bool>.Fail("Model eksik içerik döndürdü.", HttpStatusCode.BadGateway);
        }

        var verified = content with { Quotes = VerifyQuotes(content.Quotes, articles, story.Id) };

        story.Headline = Truncate(verified.Headline, HeadlineMaxLength);
        story.Summary = Truncate(verified.Summary, SummaryMaxLength);
        story.Body = verified.Body;
        story.Analysis = string.IsNullOrWhiteSpace(verified.Analysis) ? null : verified.Analysis;
        story.IsLive = verified.IsLive;
        story.Category ??= Truncate(verified.Category, CategoryMaxLength);
        story.AiDataJson = JsonSerializer.Serialize(verified);
        story.AiProcessedAt = timeProvider.GetUtcNow();
        story.NeedsRewrite = false;

        // Slug bir kez üretilir: haber yeniden yazılsa da adres değişmez.
        story.Slug ??= await BuildUniqueSlugAsync(story, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Konu yazıldı ({SourceCount} kaynak): {Headline}",
            articles.Count,
            story.Headline);

        return Result<bool>.Ok(true);
    }

    private async Task<string> BuildUniqueSlugAsync(Story story, CancellationToken cancellationToken)
    {
        var baseSlug = TextNormalizer.Slugify(story.Headline, SlugMaxLength);

        if (string.IsNullOrEmpty(baseSlug))
            baseSlug = "haber";

        if (!await storyRepository.SlugExistsAsync(baseSlug, story.Id, cancellationToken))
            return baseSlug;

        // Aynı başlık daha önce çıktıysa konunun kimliğinden kısa bir ek koyuyoruz.
        var suffix = story.Id.ToString("N")[..6];
        var trimmed = baseSlug.Length > SlugMaxLength - 7 ? baseSlug[..(SlugMaxLength - 7)] : baseSlug;

        return $"{trimmed}-{suffix}";
    }

    /// <summary>Modelin uydurmasını engeller: alıntı kaynak metinlerde birebir geçmiyorsa atılır.</summary>
    private List<AiStoryQuote> VerifyQuotes(List<AiStoryQuote> quotes, List<Article> articles, Guid storyId)
    {
        if (quotes.Count == 0)
            return [];

        var corpus = new StringBuilder();
        foreach (var article in articles)
        {
            corpus.Append(TextNormalizer.Compact(article.AiSummary));
            corpus.Append(TextNormalizer.Compact(article.AiBody));
            corpus.Append(TextNormalizer.Compact(article.AiDataJson));
        }

        var haystack = corpus.ToString();
        var verified = new List<AiStoryQuote>();

        foreach (var quote in quotes)
        {
            if (string.IsNullOrWhiteSpace(quote.Text))
                continue;

            if (haystack.Contains(TextNormalizer.Compact(quote.Text), StringComparison.Ordinal))
            {
                verified.Add(quote);
                continue;
            }

            logger.LogWarning("Doğrulanamayan alıntı atıldı ({StoryId}): {Quote}", storyId, quote.Text);
        }

        return verified;
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
