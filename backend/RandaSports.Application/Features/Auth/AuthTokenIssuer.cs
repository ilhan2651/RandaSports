using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Auth;

/// <summary>
/// Giriş, kayıt ve yenileme aynı cevabı üretiyor: erişim jetonu + yeni yenileme
/// jetonu. Üçünde de aynı kodu yazmamak için tek yerde duruyor.
/// </summary>
public sealed class AuthTokenIssuer(
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenService refreshTokenService,
    IUserRepository userRepository,
    TimeProvider timeProvider)
{
    public async Task<AuthResponse> IssueAsync(
        User user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = user.UserRoles
            .Where(x => x.Role is not null)
            .Select(x => x.Role.Code)
            .ToList();

        var access = tokenGenerator.Generate(user, roles);
        var refresh = refreshTokenService.Issue();
        var now = timeProvider.GetUtcNow();

        await userRepository.AddRefreshTokenAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refresh.TokenHash,
                ExpiresAt = refresh.ExpiresAt,
                CreatedIp = ipAddress,
                CreatedAt = now,
                UpdatedAt = now
            },
            cancellationToken);

        return new AuthResponse(
            access.Token,
            access.ExpiresAt,
            refresh.Token,
            refresh.ExpiresAt,
            user.Email,
            user.FullName,
            roles);
    }
}
