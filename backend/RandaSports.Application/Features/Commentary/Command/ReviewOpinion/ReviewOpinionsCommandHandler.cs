using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.ReviewOpinion;

/// <summary>
/// Toplu onay/ret. Her kaydı tek tek işleyen komuta devrediyor — orada alıntı
/// doğrulama bayrağı, inceleme zamanı ve kayıt tutma zaten doğru yapılıyor,
/// burada ikinci bir yol açmak o kuralları çoğaltmak olurdu.
/// </summary>
public sealed class ReviewOpinionsCommandHandler(
    IMediator mediator,
    ILogger<ReviewOpinionsCommandHandler> logger)
    : IRequestHandler<ReviewOpinionsCommand, Result<int>>
{
    public async Task<Result<int>> Handle(ReviewOpinionsCommand request, CancellationToken cancellationToken)
    {
        var islenen = 0;

        foreach (var id in request.OpinionIds.Distinct())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await mediator.Send(
                new ReviewOpinionCommand(id, request.Approve, request.Note),
                cancellationToken);

            if (!result.IsFail)
                islenen++;
            else
                logger.LogWarning("Toplu işlemde atlandı ({OpinionId}): {Message}", id, result.Message);
        }

        var fiil = request.Approve ? "yayına alındı" : "reddedildi";

        return Result<int>.Ok(islenen, $"{islenen} görüş {fiil}.");
    }
}
