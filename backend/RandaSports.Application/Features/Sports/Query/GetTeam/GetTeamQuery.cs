using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sports.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sports.Query.GetTeam;

public sealed record GetTeamQuery(string Slug) : IRequest<Result<TeamDetailDto>>;

public sealed class GetTeamQueryHandler(ISportsReadRepository readRepository)
    : IRequestHandler<GetTeamQuery, Result<TeamDetailDto>>
{
    private const int FixtureCount = 5;

    public async Task<Result<TeamDetailDto>> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        var team = await readRepository.GetTeamBySlugAsync(request.Slug, cancellationToken);

        if (team is null)
            return Result<TeamDetailDto>.Fail("Takım bulunamadı.", HttpStatusCode.NotFound);

        var standing = await readRepository.GetTeamStandingAsync(team.Id, cancellationToken);
        var squad = await readRepository.GetSquadAsync(team.Id, cancellationToken);
        var recent = await readRepository.GetTeamFixturesAsync(team.Id, false, FixtureCount, cancellationToken);
        var upcoming = await readRepository.GetTeamFixturesAsync(team.Id, true, FixtureCount, cancellationToken);

        return Result<TeamDetailDto>.Ok(new TeamDetailDto(
            team.Id,
            team.Name,
            team.Slug,
            team.LogoUrl,
            team.Country,
            standing?.ToDto(),
            squad.Select(x => x.ToListItem()).ToList(),
            recent.Select(x => x.ToDto()).ToList(),
            upcoming.Select(x => x.ToDto()).ToList()));
    }
}
