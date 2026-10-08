using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.UpdateCommentator;

/// <param name="PhotoUrl">
/// Boş gönderilirse görsel siliniyor; arayüzde baş harflere düşsün diye.
/// Görseli yalnızca insan giriyor: videodan türetmek yanlış portre üretiyordu.
/// </param>
public sealed record UpdateCommentatorCommand(Guid Id, string? PhotoUrl, string? Bio)
    : IRequest<Result<bool>>;
