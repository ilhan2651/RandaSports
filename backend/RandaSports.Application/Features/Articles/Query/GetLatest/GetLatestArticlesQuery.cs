using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Articles.Dtos;

namespace RandaSports.Application.Features.Articles.Query.GetLatest;

public sealed record GetLatestArticlesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? SportId = null,
    string? Search = null) : IRequest<Result<Paged<ArticleDto>>>;
