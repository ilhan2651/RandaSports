using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Stories.Dtos;

namespace RandaSports.Application.Features.Stories.Query.GetStories;

public sealed record GetStoriesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Sport = null,
    string? Search = null) : IRequest<Result<Paged<StoryListItemDto>>>;
