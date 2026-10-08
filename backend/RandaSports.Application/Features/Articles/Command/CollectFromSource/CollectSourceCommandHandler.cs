using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Filtering;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Articles.Command.CollectFromSource;

public sealed class CollectSourceCommandHandler(
    ISourceRepository sourceRepository,
    IArticleRepository articleRepository,
    IRssFeedReader feedReader,
    IArticleFilter articleFilter,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<CollectSourceCommandHandler> logger)
    : IRequestHandler<CollectSourceCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CollectSourceCommand request, CancellationToken cancellationToken)
    {
        var source = await sourceRepository.GetByIdAsync(request.SourceId, cancellationToken);
        if (source is null)
            return Result<int>.Fail("Kaynak bulunamadı.", HttpStatusCode.NotFound);

        var now = timeProvider.GetUtcNow();
        source.LastFetchedAt = now;

        IReadOnlyList<RssFeedItem> items;
        try
        {
            items = await feedReader.ReadAsync(source.Url, cancellationToken);
            source.LastError = null;
        }
        catch (Exception ex)
        {
            source.LastError = ex.Message.Length > 900 ? ex.Message[..900] : ex.Message;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning(ex, "Kaynak okunamadı: {SourceName} ({Url})", source.Name, source.Url);
            return Result<int>.Fail($"Kaynak okunamadı: {ex.Message}", HttpStatusCode.BadGateway);
        }

        var skipped = 0;
        var accepted = new List<RssFeedItem>();

        foreach (var item in items)
        {
            if (articleFilter.ShouldSkip(item.Title, item.Url, out var reason))
            {
                skipped++;
                logger.LogDebug("Haber elendi ({Reason}): {Title}", reason, item.Title);
                continue;
            }

            accepted.Add(item);
        }

        items = accepted;

        var urls = items.Select(x => x.Url).Distinct().ToList();
        if (urls.Count == 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<int>.Ok(0);
        }

        var existingUrls = await articleRepository.GetExistingUrlsAsync(urls, cancellationToken);

        var newArticles = items
            .DistinctBy(x => x.Url)
            .Where(x => !existingUrls.Contains(x.Url))
            .Select(x => new Article
            {
                SourceId = source.Id,
                SportId = source.SportId,
                Title = Truncate(x.Title, 500),
                Excerpt = x.Summary is null ? null : Truncate(x.Summary, 2000),
                Url = x.Url,
                ImageUrl = x.ImageUrl is null ? null : Truncate(x.ImageUrl, 1000),
                Author = x.Author is null ? null : Truncate(x.Author, 150),
                PublishedAt = x.PublishedAt.ToUniversalTime()
            })
            .ToList();

        foreach (var article in newArticles)
            await articleRepository.AddAsync(article, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (newArticles.Count > 0 || skipped > 0)
            logger.LogInformation(
                "{SourceName}: {Added} yeni haber eklendi, {Skipped} haber elendi.",
                source.Name, newArticles.Count, skipped);

        return Result<int>.Ok(newArticles.Count);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
