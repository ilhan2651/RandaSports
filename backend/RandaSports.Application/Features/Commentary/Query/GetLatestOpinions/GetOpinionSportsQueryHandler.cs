using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetLatestOpinions;

public sealed class GetOpinionSportsQueryHandler(IOpinionRepository opinionRepository)
    : IRequestHandler<GetOpinionSportsQuery, Result<List<SportFacetDto>>>
{
    public async Task<Result<List<SportFacetDto>>> Handle(
        GetOpinionSportsQuery request,
        CancellationToken cancellationToken)
    {
        var sports = await opinionRepository.GetApprovedSportsAsync(cancellationToken);

        return Result<List<SportFacetDto>>.Ok([.. sports.Select(x => x.ToDto())]);
    }
}
