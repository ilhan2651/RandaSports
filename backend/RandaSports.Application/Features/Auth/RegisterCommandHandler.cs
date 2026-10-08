using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Domain.Entities;

namespace RandaSports.Application.Features.Auth;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    AuthTokenIssuer tokenIssuer,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var normalized = email.ToUpperInvariant();

        if (await userRepository.EmailExistsAsync(normalized, cancellationToken))
            return Result<AuthResponse>.Fail(
                "Bu e-posta adresi zaten kayıtlı.",
                HttpStatusCode.Conflict);

        var role = await userRepository.GetRoleByCodeAsync(RoleCodes.Reader, cancellationToken);

        if (role is null)
            return Result<AuthResponse>.Fail(
                "Okuyucu rolü tanımlı değil, kayıt alınamıyor.",
                HttpStatusCode.ServiceUnavailable);

        var now = timeProvider.GetUtcNow();

        var user = new User
        {
            Email = email,
            NormalizedEmail = normalized,
            PasswordHash = passwordHasher.Hash(request.Password),
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        user.UserRoles.Add(new UserRole { User = user, Role = role, AssignedAt = now });

        await userRepository.AddAsync(user, cancellationToken);

        // Jeton kaydı kullanıcıyla aynı SaveChanges'te yazılıyor; EF ilişkiyi
        // çözdüğü için kullanıcı satırının önce yazılmış olması gerekmiyor.
        var response = await tokenIssuer.IssueAsync(user, request.IpAddress, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Yeni kullanıcı kaydı: {Email}", user.Email);
        return Result<AuthResponse>.Ok(response, "Hesabın oluşturuldu.");
    }
}
