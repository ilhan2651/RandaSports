using MediatR;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Auth;

public sealed class LogoutCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenService refreshTokenService,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<LogoutCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = refreshTokenService.Hash(request.RefreshToken);
        var stored = await userRepository.GetRefreshTokenAsync(hash, cancellationToken);

        // Jeton bulunamazsa da başarı dönüyoruz: çıkış yapmak isteyen birine
        // "böyle bir oturum yok" demenin bir faydası yok, zararı var.
        if (stored is not null && stored.RevokedAt is null)
        {
            stored.RevokedAt = timeProvider.GetUtcNow();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Ok(true, "Çıkış yapıldı.");
    }
}
