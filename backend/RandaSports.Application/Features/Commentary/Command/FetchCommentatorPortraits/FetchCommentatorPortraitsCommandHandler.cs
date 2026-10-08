using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Commentary.Command.FetchCommentatorPortraits;

/// <summary>
/// Doğrulanmış ama görseli olmayan yorumcular için açık lisanslı portre arar.
/// Bulamadığı kişiye dokunmuyor: arayüz baş harflere düşüyor, sen istersen
/// yönetim ekranından elle giriyorsun.
/// </summary>
public sealed class FetchCommentatorPortraitsCommandHandler(
    ICommentatorRepository commentatorRepository,
    IPortraitLookup portraitLookup,
    IUnitOfWork unitOfWork,
    ILogger<FetchCommentatorPortraitsCommandHandler> logger)
    : IRequestHandler<FetchCommentatorPortraitsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        FetchCommentatorPortraitsCommand request,
        CancellationToken cancellationToken)
    {
        var people = await commentatorRepository.GetVerifiedWithoutPhotoAsync(
            request.Take,
            cancellationToken);

        if (people.Count == 0)
            return Result<int>.Ok(0, "Görseli aranacak yorumcu yok.");

        var found = 0;

        foreach (var person in people)
        {
            var url = await portraitLookup.FindAsync(person.FullName, cancellationToken);

            if (url is null)
                continue;

            person.PhotoUrl = url;
            found++;

            logger.LogInformation("Portre bulundu: {Name}", person.FullName);
        }

        if (found > 0)
            await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Ok(found, $"{found} portre bulundu.");
    }
}
