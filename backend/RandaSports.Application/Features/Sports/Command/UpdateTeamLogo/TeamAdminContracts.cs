using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Sports.Command.UpdateTeamLogo;

public sealed record TeamLogoRow(
    Guid Id,
    string Name,
    string Slug,
    string? Country,
    bool IsNational,
    string? SportSlug,
    string? LogoUrl);

public sealed record GetTeamLogosQuery : IRequest<Result<List<TeamLogoRow>>>;

/// <param name="LogoUrl">Boş gönderilirse logo siliniyor ve arka plan işi yeniden deniyor.</param>
public sealed record UpdateTeamLogoCommand(Guid Id, string? LogoUrl) : IRequest<Result<bool>>;
