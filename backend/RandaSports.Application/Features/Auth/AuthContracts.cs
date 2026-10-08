using MediatR;
using RandaSports.Application.Common.Wrappers;

namespace RandaSports.Application.Features.Auth;

/// <param name="Roles">Arayüz neyi göstereceğine buna bakarak karar veriyor.</param>
public sealed record AuthResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshExpiresAt,
    string Email,
    string? FullName,
    List<string> Roles);

/// <param name="IpAddress">Şüpheli oturumu ayırt etmek için; istemci değil sunucu dolduruyor.</param>
public sealed record LoginCommand(string Email, string Password, string? IpAddress = null)
    : IRequest<Result<AuthResponse>>;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string? FullName,
    string? IpAddress = null) : IRequest<Result<AuthResponse>>;

public sealed record RefreshTokenCommand(string RefreshToken, string? IpAddress = null)
    : IRequest<Result<AuthResponse>>;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result<bool>>;
