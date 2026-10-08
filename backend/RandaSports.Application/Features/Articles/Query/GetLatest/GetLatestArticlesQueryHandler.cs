using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Articles.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Articles.Query.GetLatest;

public sealed class GetLatestArticlesQueryHandler(IArticleRepository articleRepository)
    : IRequestHandler<GetLatestArticlesQuery, Result<Paged<ArticleDto>>>
{
    private const int MaxPageSize = 50;

    public async Task<Result<Paged<ArticleDto>>> Handle(
        GetLatestArticlesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, MaxPageSize);

        var articles = await articleRepository.GetPagedAsync(
            page, pageSize, request.SportId, request.Search, cancellationToken);

        var dto = new Paged<ArticleDto>(
            articles.Items.Select(x => x.ToDto()).ToList(),
            articles.Total,
            articles.Page,
            articles.PageSize);

        return Result<Paged<ArticleDto>>.Ok(dto);
    }
}
