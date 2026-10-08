using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;

namespace RandaSports.Application.Features.Commentary.Query.GetStoryOpinions;

public sealed record GetStoryOpinionsQuery(Guid StoryId) : IRequest<Result<List<OpinionDto>>>;
