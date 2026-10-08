using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Commentary.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Query.GetLatestOpinions;

public sealed class GetLatestOpinionsQueryHandler(
    IOpinionRepository opinionRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetLatestOpinionsQuery, Result<Paged<OpinionDto>>>
{
    private const int MaxPageSize = 50;

    public async Task<Result<Paged<OpinionDto>>> Handle(
        GetLatestOpinionsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 12 : Math.Min(request.PageSize, MaxPageSize);

        DateTimeOffset? since = request.Days > 0
            ? timeProvider.GetUtcNow().AddDays(-request.Days)
            : null;

        var opinions = await opinionRepository.GetApprovedAsync(
            request.Commentator,
            request.Team,
            request.Sport,
            since,
            page,
            pageSize,
            cancellationToken);

        var dto = new Paged<OpinionDto>(
            opinions.Items.Select(x => x.ToDto()).ToList(),
            opinions.Total,
            opinions.Page,
            opinions.PageSize);

        return Result<Paged<OpinionDto>>.Ok(dto);
    }
}

public sealed class GetOpinionTeamsQueryHandler(IOpinionRepository opinionRepository)
    : IRequestHandler<GetOpinionTeamsQuery, Result<List<TeamFacetDto>>>
{
    public async Task<Result<List<TeamFacetDto>>> Handle(
        GetOpinionTeamsQuery request,
        CancellationToken cancellationToken)
    {
        var teams = await opinionRepository.GetApprovedTeamsAsync(cancellationToken);

        return Result<List<TeamFacetDto>>.Ok(teams.Select(x => x.ToDto()).ToList());
    }
}
