using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.CreateChannel;

/// <param name="Reference">"@sporx", kanal adresi veya UC... kimliği.</param>
/// <param name="SportSlugs">
/// Kanalın kapsadığı branşlar; genel spor kanallarında boş bırakılıyor.
/// </param>
public sealed record CreateChannelCommand(
    string Name,
    string Reference,
    string[]? SportSlugs = null) : IRequest<Result<Guid>>;
