using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Stories.Dtos;

namespace RandaSports.Application.Features.Stories.Query.GetStoryBySlug;

public sealed record GetStoryBySlugQuery(string Slug) : IRequest<Result<StoryDetailDto>>;
