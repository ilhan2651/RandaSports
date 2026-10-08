using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Commentary.Command.UpdateChannel;

/// <summary>Boş bırakılan alanlar değiştirilmiyor.</summary>
public sealed record UpdateChannelCommand(
    Guid ChannelId,
    string? Name = null,
    string? Reference = null,
    bool? IsActive = null,
    string[]? SportSlugs = null) : IRequest<Result<bool>>;
