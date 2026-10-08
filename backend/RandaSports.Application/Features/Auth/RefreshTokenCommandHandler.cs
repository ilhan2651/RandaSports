using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Auth;

/// <summary>
/// Yenileme jetonu tek kullanımlık: her yenilemede eskisi iptal edilip yenisi
/// veriliyor. Çalınan bir jeton böyle en fazla bir kez işe yarıyor.
/// </summary>
public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenService refreshTokenService,
    AuthTokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<RefreshTokenCommandHandler> logger)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private const string FailureMessage = "Oturum geçersiz, tekrar giriş yap.";

    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var hash = refreshTokenService.Hash(request.RefreshToken);
        var stored = await userRepository.GetRefreshTokenAsync(hash, cancellationToken);
        var now = timeProvider.GetUtcNow();

        if (stored is null || !stored.IsActive(now) || !stored.User.IsActive)
            return Result<AuthResponse>.Fail(FailureMessage, HttpStatusCode.Unauthorized);

        stored.RevokedAt = now;

        var response = await tokenIssuer.IssueAsync(stored.User, request.IpAddress, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Oturum yenilendi: {Email}", stored.User.Email);
        return Result<AuthResponse>.Ok(response);
    }
}
