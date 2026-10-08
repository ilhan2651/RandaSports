using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;

namespace RandaSports.Application.Features.Commentary.Command.UpdateCommentator;

public sealed class UpdateCommentatorCommandHandler(
    ICommentatorRepository commentatorRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCommentatorCommandHandler> logger)
    : IRequestHandler<UpdateCommentatorCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateCommentatorCommand request, CancellationToken cancellationToken)
    {
        var commentator = await commentatorRepository.GetForReviewAsync(request.Id, cancellationToken);

        if (commentator is null)
            return Result<bool>.Fail("Yorumcu bulunamadı.", HttpStatusCode.NotFound);

        // Boş metin "sil" demek, null "dokunma" demek: formdan hep bir değer geliyor,
        // ikisini ayırmazsak temizleme düğmesi çalışmıyor.
        if (request.PhotoUrl is not null)
            commentator.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl)
                ? null
                : request.PhotoUrl.Trim();

        if (request.Bio is not null)
            commentator.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Yorumcu güncellendi: {Name}", commentator.FullName);
        return Result<bool>.Ok(true, $"{commentator.FullName} güncellendi.");
    }
}
