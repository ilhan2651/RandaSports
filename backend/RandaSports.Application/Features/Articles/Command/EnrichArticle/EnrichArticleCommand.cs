using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Articles.Command.EnrichArticle;

public sealed record EnrichArticleCommand(Guid ArticleId) : IRequest<Result<bool>>;
