using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Sports.Command.UpdateTeamLogo;

public sealed class GetTeamLogosQueryHandler(ISportsReadRepository sportsReadRepository)
    : IRequestHandler<GetTeamLogosQuery, Result<List<TeamLogoRow>>>
{
    public async Task<Result<List<TeamLogoRow>>> Handle(
        GetTeamLogosQuery request,
        CancellationToken cancellationToken)
    {
        var teams = await sportsReadRepository.GetAllTeamsAsync(cancellationToken);

        return Result<List<TeamLogoRow>>.Ok(
        [
            .. teams.Select(x => new TeamLogoRow(
                x.Id,
                x.Name,
                x.Slug,
                x.Country,
                x.IsNational,
                x.Sport?.Slug,
                x.LogoUrl))
        ]);
    }
}

public sealed class UpdateTeamLogoCommandHandler(
    ISportsRepository sportsRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateTeamLogoCommandHandler> logger)
    : IRequestHandler<UpdateTeamLogoCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateTeamLogoCommand request, CancellationToken cancellationToken)
    {
        var team = await sportsRepository.GetTeamAsync(request.Id, cancellationToken);

        if (team is null)
            return Result<bool>.Fail("Takım bulunamadı.", HttpStatusCode.NotFound);

        team.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Takım logosu güncellendi: {Name}", team.Name);
        return Result<bool>.Ok(true, $"{team.Name} güncellendi.");
    }
}
