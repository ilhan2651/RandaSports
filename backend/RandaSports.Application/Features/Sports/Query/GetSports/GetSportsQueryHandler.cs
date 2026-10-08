using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Features.Sports.Dtos;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sports.Query.GetSports;

public sealed class GetSportsQueryHandler(ISportsReadRepository sportsReadRepository)
    : IRequestHandler<GetSportsQuery, Result<List<SportDto>>>
{
    public async Task<Result<List<SportDto>>> Handle(
        GetSportsQuery request,
        CancellationToken cancellationToken)
    {
        var sports = await sportsReadRepository.GetSportsAsync(cancellationToken);

        var items = sports
            .Where(x => !request.OnlyWithStories || x.StoryCount > 0)
            .Select(x => new SportDto(x.Name, x.Slug, x.DisplayOrder, x.StoryCount, x.HasCompetitions))
            .ToList();

        return Result<List<SportDto>>.Ok(items);
    }
}
