using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Command.ReviewCommentator;

public sealed class ReviewCommentatorCommandHandler(
    ICommentatorRepository commentatorRepository,
    IUnitOfWork unitOfWork,
    ILogger<ReviewCommentatorCommandHandler> logger)
    : IRequestHandler<ReviewCommentatorCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ReviewCommentatorCommand request, CancellationToken cancellationToken)
    {
        var commentator = await commentatorRepository.GetForReviewAsync(request.Id, cancellationToken);

        if (commentator is null)
            return Result<bool>.Fail("Yorumcu bulunamadı.", HttpStatusCode.NotFound);

        if (request.Approve)
        {
            commentator.IsVerified = true;
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Yorumcu doğrulandı: {Name}", commentator.FullName);
            return Result<bool>.Ok(true, $"{commentator.FullName} doğrulandı.");
        }

        // Yanlış eklenmiş kişi: ona atfedilen görüşler de yanlış atfedilmiş demektir.
        // Otomatik yayına girenleri geri çekip onay ekranına düşürüyoruz; isim alanında
        // modelin duyduğu metin kalıyor ki ne olduğunu görebilesin.
        var geriCekilen = 0;

        foreach (var opinion in commentator.Opinions)
        {
            opinion.CommentatorId = null;
            opinion.AttributionConfidence = null;

            if (opinion.Status != OpinionStatus.Approved)
                continue;

            opinion.Status = OpinionStatus.Pending;
            opinion.ReviewedAt = null;
            opinion.ReviewNote = null;
            geriCekilen++;
        }

        // Kadrodan da çıkıyor: yoksa aynı kanalın sonraki videolarında aday olarak sunulur.
        commentator.Channels.Clear();

        commentatorRepository.Delete(commentator);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Yorumcu reddedildi ve silindi: {Name} ({Count} görüş onaya geri döndü).",
            commentator.FullName,
            geriCekilen);

        return Result<bool>.Ok(
            true,
            geriCekilen > 0
                ? $"{commentator.FullName} silindi, {geriCekilen} görüş onaya geri döndü."
                : $"{commentator.FullName} silindi.");
    }
}
