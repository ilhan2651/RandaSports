using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Domain.Enums;

namespace RandaSports.Application.Features.Commentary.Command.ReviewOpinion;

public sealed class ReviewOpinionCommandHandler(
    IOpinionRepository opinionRepository,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<ReviewOpinionCommandHandler> logger)
    : IRequestHandler<ReviewOpinionCommand, Result<bool>>
{
    private const int NoteMaxLength = 1000;
    private const int QuoteMaxLength = 2000;

    public async Task<Result<bool>> Handle(ReviewOpinionCommand request, CancellationToken cancellationToken)
    {
        var opinion = await opinionRepository.GetWithContextAsync(request.OpinionId, cancellationToken);

        if (opinion is null)
            return Result<bool>.Fail("Görüş bulunamadı.", HttpStatusCode.NotFound);

        // Düzeltme onayla birlikte geliyor: ön kontrol başka bir cümle duyduysa
        // yönetici duyulan hali alıntıya geçirip aynı hamlede onaylıyor.
        if (!string.IsNullOrWhiteSpace(request.Quote))
            opinion.Quote = Truncate(request.Quote, QuoteMaxLength)!;

        if (request.TimestampSeconds is >= 0)
            opinion.TimestampSeconds = request.TimestampSeconds;

        opinion.Status = request.Approve ? OpinionStatus.Approved : OpinionStatus.Rejected;
        opinion.ReviewedAt = timeProvider.GetUtcNow();
        opinion.ReviewNote = Truncate(request.Note, NoteMaxLength);

        // Alıntının doğruluğunu makine değil insan onaylıyor: videoyu açıp dinleyen kişi.
        opinion.IsQuoteVerified = request.Approve;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Görüş {Status}: {Speaker} — {Topic}",
            opinion.Status,
            opinion.Commentator?.FullName ?? opinion.SpeakerLabel ?? "bilinmiyor",
            opinion.Topic);

        return Result<bool>.Ok(true);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
