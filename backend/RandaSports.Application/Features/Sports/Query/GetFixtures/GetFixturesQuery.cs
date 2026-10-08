using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sports.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sports.Query.GetFixtures;

public sealed record GetFixturesQuery(
    string Sport = "futbol",
    string? Team = null,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<Result<List<FixtureDto>>>;

public sealed class GetFixturesQueryHandler(
    ISportsReadRepository readRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GetFixturesQuery, Result<List<FixtureDto>>>
{
    private const int DefaultDaysBack = 3;
    private const int DefaultDaysAhead = 10;

    public async Task<Result<List<FixtureDto>>> Handle(
        GetFixturesQuery request,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        var from = request.From ?? today.AddDays(-DefaultDaysBack);
        var to = request.To ?? today.AddDays(DefaultDaysAhead);

        var fixtures = await readRepository.GetFixturesAsync(
            request.Sport,
            new DateTimeOffset(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            new DateTimeOffset(to.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero),
            request.Team,
            cancellationToken);

        return Result<List<FixtureDto>>.Ok(fixtures.Select(x => x.ToDto()).ToList());
    }
}
