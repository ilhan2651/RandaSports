using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sports.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sports.Query.GetStandings;

public sealed record GetStandingsQuery(string Sport = "futbol", string? Competition = null)
    : IRequest<Result<StandingsDto>>;

public sealed class GetStandingsQueryHandler(ISportsReadRepository readRepository)
    : IRequestHandler<GetStandingsQuery, Result<StandingsDto>>
{
    public async Task<Result<StandingsDto>> Handle(
        GetStandingsQuery request,
        CancellationToken cancellationToken)
    {
        var competition = await readRepository.GetCompetitionAsync(
            request.Competition, request.Sport, cancellationToken);

        if (competition is null)
            return Result<StandingsDto>.Fail("Lig bulunamadı.", HttpStatusCode.NotFound);

        var season = await readRepository.GetCurrentSeasonAsync(competition.Id, cancellationToken);

        if (season is null)
            return Result<StandingsDto>.Fail("Sezon bulunamadı.", HttpStatusCode.NotFound);

        var rows = await readRepository.GetStandingsAsync(season.Id, cancellationToken);

        return Result<StandingsDto>.Ok(new StandingsDto(
            competition.Name,
            competition.Slug,
            season.Name,
            rows.Select(x => x.ToDto()).ToList()));
    }
}
