using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Stories.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Stories.Query.GetStories;

public sealed class GetStoriesQueryHandler(IStoryRepository storyRepository)
    : IRequestHandler<GetStoriesQuery, Result<Paged<StoryListItemDto>>>
{
    private const int MaxPageSize = 50;

    public async Task<Result<Paged<StoryListItemDto>>> Handle(
        GetStoriesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, MaxPageSize);

        var stories = await storyRepository.GetPagedAsync(
            page, pageSize, request.Sport, request.Search, cancellationToken);

        var dto = new Paged<StoryListItemDto>(
            stories.Items.Select(x => x.ToListItem()).ToList(),
            stories.Total,
            stories.Page,
            stories.PageSize);

        return Result<Paged<StoryListItemDto>>.Ok(dto);
    }
}
