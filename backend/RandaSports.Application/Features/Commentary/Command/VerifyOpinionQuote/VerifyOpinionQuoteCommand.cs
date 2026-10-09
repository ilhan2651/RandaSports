using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.VerifyOpinionQuote;

/// <param name="WindowSeconds">
/// Damganın iki yanında kaç saniyeye bakılacak. Damga zaten kayabildiği için
/// pencere sözün biraz öncesini ve sonrasını da kapsıyor.
/// </param>
public sealed record VerifyOpinionQuoteCommand(
    Guid OpinionId,
    int WindowSeconds = 20) : IRequest<Result<bool>>;
