using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.AttachSpeaker;

/// <summary>
/// Modelin duyduğu ama sözlükte olmayan konuşmacıyı sözlüğe ekler ve görüşe bağlar.
/// Onay ekranındaki tek düğme; sözlük böylece kullandıkça büyüyor.
/// </summary>
/// <param name="FullName">Boş bırakılırsa görüşteki konuşmacı adı kullanılır.</param>
public sealed record AttachSpeakerCommand(
    Guid OpinionId,
    string? FullName = null) : IRequest<Result<bool>>;
