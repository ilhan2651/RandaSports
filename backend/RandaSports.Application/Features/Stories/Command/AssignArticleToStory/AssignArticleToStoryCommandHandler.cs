using System.Net;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Clustering;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Articles.Command.EnrichArticle;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Stories.Command.AssignArticleToStory;

public sealed class AssignArticleToStoryCommandHandler(
    IArticleRepository articleRepository,
    IStoryRepository storyRepository,
    ITeamMatcher teamMatcher,
    ISportsReadRepository sportsReadRepository,
    ISportDictionary sportDictionary,
    IUnitOfWork unitOfWork,
    ILogger<AssignArticleToStoryCommandHandler> logger)
    : IRequestHandler<AssignArticleToStoryCommand, Result<bool>>
{
    private const int CategoryMaxLength = 50;

    /// <summary>Başlıkta takım yoksa gövdedeki eşleşmeler ancak bu kadar azsa konu sayılır.</summary>
    private const int MaxBodyOnlyTeams = 2;

    public async Task<Result<bool>> Handle(AssignArticleToStoryCommand request, CancellationToken cancellationToken)
    {
        var article = await articleRepository.GetForClusteringAsync(request.ArticleId, cancellationToken);
        if (article is null)
            return Result<bool>.Fail("Haber bulunamadı.", HttpStatusCode.NotFound);

        if (article.StoryId.HasValue)
            return Result<bool>.Ok(false, "Haber zaten bir konuya bağlı.");

        var analysis = ReadAnalysis(article);

        var teams = await teamMatcher.MatchAsync(
            $"{article.Title} {article.AiHeadline}",
            article.AiBody ?? article.AiSummary,
            cancellationToken);

        var subjects = ResolveSubjects(teams);
        var entities = ResolveEntities(subjects, analysis);
        var sportSlug = await ResolveSportAsync(article, subjects, analysis, cancellationToken);

        var clusterKey = ClusterKeyBuilder.Build(
            sportSlug,
            analysis?.Category,
            entities,
            article.PublishedAt,
            article.Id);

        var story = await storyRepository.GetByClusterKeyAsync(clusterKey, cancellationToken);
        var isNew = story is null;

        if (story is null)
        {
            story = new Story
            {
                ClusterKey = clusterKey,
                SportId = article.SportId,
                Category = Truncate(analysis?.Category, CategoryMaxLength),
                FirstPublishedAt = article.PublishedAt,
                LastPublishedAt = article.PublishedAt,
                SourceCount = 1,
                NeedsRewrite = true
            };

            await storyRepository.AddAsync(story, cancellationToken);
        }
        else
        {
            var sourceIds = await storyRepository.GetSourceIdsAsync(story.Id, cancellationToken);
            if (!sourceIds.Contains(article.SourceId))
                story.SourceCount = sourceIds.Count + 1;

            story.SportId ??= article.SportId;
            story.Category ??= Truncate(analysis?.Category, CategoryMaxLength);

            if (article.PublishedAt < story.FirstPublishedAt)
                story.FirstPublishedAt = article.PublishedAt;

            if (article.PublishedAt > story.LastPublishedAt)
                story.LastPublishedAt = article.PublishedAt;
        }

        // Görsel: en yüksek öncelikli kaynağın görseli kazanır.
        if (!string.IsNullOrWhiteSpace(article.ImageUrl)
            && (story.ImageUrl is null || article.Source.Priority > story.ImagePriority))
        {
            story.ImageUrl = article.ImageUrl;
            story.ImagePriority = article.Source.Priority;
        }

        // Eşleşen takımları konuya bağlıyoruz: kişiye özel akış ve takım sayfaları
        // bunu okuyor. Zaten eşleştirdiğimiz veriyi atmamak için burada duruyor.
        await AttachTeamsAsync(story, subjects, cancellationToken);

        // Yeni kaynak geldi: metin baştan yazılacak.
        story.NeedsRewrite = true;
        story.AiAttempts = 0;

        article.Story = story;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "{Action} konu ({ClusterKey}) <- {Title}",
            isNew ? "Yeni" : "Mevcut",
            clusterKey,
            article.Title);

        return Result<bool>.Ok(true);
    }

    /// <summary>Başlıkta geçen takım haberin öznesidir; başlıkta yoksa gövdedeki birkaç eşleşme kabul edilir.</summary>
    private static List<MatchedTeam> ResolveSubjects(List<MatchedTeam> teams)
    {
        var inTitle = teams.Where(x => x.InTitle).ToList();
        if (inTitle.Count > 0)
            return inTitle;

        return teams.Count is > 0 and <= MaxBodyOnlyTeams ? teams : [];
    }

    /// <summary>
    /// Kümeleme sözlüğe güvenir. Sözlükte takım bulunamazsa modelin sporcu listesine düşeriz;
    /// modelin takım listesini kullanmıyoruz, çünkü sponsor ve mekan adlarını takım sanabiliyor.
    /// </summary>
    /// <summary>
    /// Konuya yeni takımları ekliyor, var olanlara dokunmuyor. Bir konu birkaç
    /// haberden besleniyor ve her haber farklı takım tanıyabiliyor; birikerek
    /// tam listeye ulaşıyoruz.
    /// </summary>
    private async Task AttachTeamsAsync(
        Story story,
        List<MatchedTeam> subjects,
        CancellationToken cancellationToken)
    {
        if (subjects.Count == 0)
            return;

        var mevcut = story.Teams.Select(x => x.Id).ToHashSet();
        var eksik = subjects.Select(x => x.Id).Where(x => !mevcut.Contains(x)).Distinct().ToList();

        if (eksik.Count == 0)
            return;

        foreach (var team in await sportsReadRepository.GetTeamsByIdsAsync(eksik, cancellationToken))
            story.Teams.Add(team);
    }

    private static List<string> ResolveEntities(List<MatchedTeam> subjects, AiArticleAnalysis? analysis)
    {
        if (subjects.Count > 0)
            return subjects.Select(x => x.Slug).ToList();

        return analysis?.Athletes ?? [];
    }

    /// <summary>
    /// Branş üç kademede belirleniyor: kaynağın kendi branşı, takım sözlüğü, en son modelin
    /// tahmini. Model tahmini en zayıf delil olduğu için en sonda: takım sözlüğü MMA, NFL ya
    /// da tenis bilmiyor, o branşlarda tek dayanağımız bu. Slug'ın kayıtlı olduğu
    /// zenginleştirme adımında doğrulanmış durumda.
    /// </summary>
    private async Task<string?> ResolveSportAsync(
        Article article,
        List<MatchedTeam> subjects,
        AiArticleAnalysis? analysis,
        CancellationToken cancellationToken)
    {
        if (article.Sport is not null)
            return article.Sport.Slug;

        var sportIds = subjects.Select(x => x.SportId).Distinct().ToList();

        if (sportIds.Count == 1)
        {
            // Haberin branşı sözlükten anlaşıldı: bir daha tahmin etmeyelim diye kaydediyoruz.
            article.SportId = sportIds[0];
            return subjects[0].SportSlug;
        }

        // Birden fazla branştan takım eşleşmişse (aynı adlı kulüpler) tahmine girmiyoruz.
        if (sportIds.Count > 1)
            return null;

        var guessed = await sportDictionary.FindAsync(analysis?.Sport, cancellationToken);
        if (guessed is null)
            return null;

        article.SportId = guessed.Id;

        logger.LogInformation(
            "Branş modelden belirlendi ({ArticleId}): {Sport} <- {Title}",
            article.Id,
            guessed.Slug,
            article.Title);

        return guessed.Slug;
    }

    private AiArticleAnalysis? ReadAnalysis(Article article)
    {
        if (string.IsNullOrWhiteSpace(article.AiDataJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<AiArticleAnalysis>(article.AiDataJson);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "AI verisi okunamadı ({ArticleId}).", article.Id);
            return null;
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
