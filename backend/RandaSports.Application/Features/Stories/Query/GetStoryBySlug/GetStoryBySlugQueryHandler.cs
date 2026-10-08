using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Stories.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Stories.Query.GetStoryBySlug;

public sealed class GetStoryBySlugQueryHandler(IStoryRepository storyRepository)
    : IRequestHandler<GetStoryBySlugQuery, Result<StoryDetailDto>>
{
    public async Task<Result<StoryDetailDto>> Handle(
        GetStoryBySlugQuery request,
        CancellationToken cancellationToken)
    {
        var story = await storyRepository.GetBySlugAsync(request.Slug, cancellationToken);

        if (story is null || story.AiProcessedAt is null)
            return Result<StoryDetailDto>.Fail("Haber bulunamadı.", HttpStatusCode.NotFound);

        return Result<StoryDetailDto>.Ok(story.ToDetail());
    }
}
