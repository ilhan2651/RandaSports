using System.Net;
using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sports.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sports.Query.GetAthlete;

public sealed record GetAthleteQuery(string Slug) : IRequest<Result<AthleteDetailDto>>;

public sealed class GetAthleteQueryHandler(ISportsReadRepository readRepository)
    : IRequestHandler<GetAthleteQuery, Result<AthleteDetailDto>>
{
    public async Task<Result<AthleteDetailDto>> Handle(
        GetAthleteQuery request,
        CancellationToken cancellationToken)
    {
        var athlete = await readRepository.GetAthleteBySlugAsync(request.Slug, cancellationToken);

        return athlete is null
            ? Result<AthleteDetailDto>.Fail("Oyuncu bulunamadı.", HttpStatusCode.NotFound)
            : Result<AthleteDetailDto>.Ok(athlete.ToDetail());
    }
}
