using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Stories.Command.AssignArticleToStory;

/// <summary>Bir haberi ait olduğu konuya bağlar; konu yoksa açar.</summary>
public sealed record AssignArticleToStoryCommand(Guid ArticleId) : IRequest<Result<bool>>;
