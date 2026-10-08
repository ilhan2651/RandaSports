using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetUnverifiedCommentators;

/// <summary>Videodan otomatik eklenmiş, insan onayı bekleyen konuşmacılar.</summary>
public sealed record GetUnverifiedCommentatorsQuery : IRequest<Result<List<UnverifiedCommentatorDto>>>;
