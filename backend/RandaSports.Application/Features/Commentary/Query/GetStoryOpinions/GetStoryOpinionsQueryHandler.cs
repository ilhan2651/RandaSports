using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetStoryOpinions;

public sealed class GetStoryOpinionsQueryHandler(IOpinionRepository opinionRepository)
    : IRequestHandler<GetStoryOpinionsQuery, Result<List<OpinionDto>>>
{
    public async Task<Result<List<OpinionDto>>> Handle(
        GetStoryOpinionsQuery request,
        CancellationToken cancellationToken)
    {
        var opinions = await opinionRepository.GetApprovedByStoryAsync(request.StoryId, cancellationToken);

        return Result<List<OpinionDto>>.Ok(opinions.Select(x => x.ToDto()).ToList());
    }
}
