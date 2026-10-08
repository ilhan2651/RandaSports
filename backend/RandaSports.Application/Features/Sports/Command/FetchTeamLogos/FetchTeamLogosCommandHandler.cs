using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Sports.Command.FetchTeamLogos;

/// <summary>
/// Logosu olmayan takımlara logo arar. Her turda sınırlı sayıda deneniyor; seed'e
/// yeni takım eklendiğinde ek bir iş yapmaya gerek kalmadan bir sonraki turda
/// kendiliğinden sıraya giriyor.
///
/// Bulamadığına dokunmuyor. Her takımın logosu bulunamayacak — kulüp armaları
/// ticari marka olduğu için çoğu açık lisanslı arşivlerde yok. Kalanlar yönetim
/// ekranından elle giriliyor.
/// </summary>
public sealed class FetchTeamLogosCommandHandler(
    ISportsReadRepository sportsReadRepository,
    ILogoLookup logoLookup,
    IUnitOfWork unitOfWork,
    ILogger<FetchTeamLogosCommandHandler> logger)
    : IRequestHandler<FetchTeamLogosCommand, Result<int>>
{
    public async Task<Result<int>> Handle(FetchTeamLogosCommand request, CancellationToken cancellationToken)
    {
        var teams = await sportsReadRepository.GetTeamsWithoutLogoAsync(request.Take, cancellationToken);

        if (teams.Count == 0)
            return Result<int>.Ok(0, "Logosu aranacak takım yok.");

        var found = 0;

        foreach (var team in teams)
        {
            var url = await logoLookup.FindAsync(
                new LogoRequest(team.Name, team.IsNational, team.Country),
                cancellationToken);

            if (url is null)
                continue;

            team.LogoUrl = url;
            found++;

            logger.LogInformation("Logo bulundu: {Name}", team.Name);
        }

        if (found > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(found, $"{teams.Count} takımdan {found} tanesinin logosu bulundu.");
    }
}
