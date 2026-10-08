using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetCommentators;

public sealed record GetCommentatorsQuery : IRequest<Result<List<CommentatorDto>>>;

/// <param name="Slug">Yorumcunun adresi.</param>
public sealed record GetCommentatorQuery(string Slug) : IRequest<Result<CommentatorDto>>;
