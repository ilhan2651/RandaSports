using System.Net;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RandaSports.Application.Common.Wrappers;
using RandaSports.Application.Interfaces;
using RandaSports.Application.Interfaces.Repositories;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Application.Common.Options;

namespace RandaSports.Application.Features.Auth;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    AuthTokenIssuer tokenIssuer,
    IOptions<AuthPolicyOptions> options,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    /// <summary>
    /// Hesap yok, parola yanlış, hesap kapalı — üçünde de aynı cevap dönüyor.
    /// Farklı mesaj vermek, hangi adreslerin kayıtlı olduğunu dışarıya söylerdi.
    /// </summary>
    private const string FailureMessage = "E-posta ya da parola hatalı.";

    /// <summary>Gerçek bir özetle aynı maliyette, hiçbir parolaya uymayan sabit.</summary>
    private const string DummyHash =
        "pbkdf2.210000.AAAAAAAAAAAAAAAAAAAAAA==.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    private readonly AuthPolicyOptions _options = options.Value;

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await userRepository.GetByEmailAsync(normalized, cancellationToken);
        var now = timeProvider.GetUtcNow();

        if (user is null || !user.IsActive)
        {
            // Hesap yoksa da özet doğrulaması yapıyoruz: cevabın süresi, adresin
            // kayıtlı olup olmadığını ele vermesin.
            passwordHasher.Verify(request.Password, DummyHash);

            logger.LogWarning("Başarısız giriş denemesi: {Email}", request.Email);
            return Result<AuthResponse>.Fail(FailureMessage, HttpStatusCode.Unauthorized);
        }

        // Kilitli hesapta parolayı hiç kontrol etmiyoruz: deneme saldırısının
        // maliyeti ancak böyle artıyor.
        if (user.LockoutEnd is { } until && until > now)
        {
            var kalan = (int)Math.Ceiling((until - now).TotalMinutes);

            return Result<AuthResponse>.Fail(
                $"Çok fazla hatalı deneme. {kalan} dakika sonra tekrar deneyin.",
                HttpStatusCode.TooManyRequests);
        }

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= _options.MaxFailedAttempts)
            {
                user.LockoutEnd = now.AddMinutes(_options.LockoutMinutes);
                user.FailedLoginAttempts = 0;

                logger.LogWarning("Hesap geçici kilitlendi: {Email}", user.Email);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Başarısız giriş denemesi: {Email}", user.Email);
            return Result<AuthResponse>.Fail(FailureMessage, HttpStatusCode.Unauthorized);
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = now;

        var response = await tokenIssuer.IssueAsync(user, request.IpAddress, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Giriş yapıldı: {Email}", user.Email);
        return Result<AuthResponse>.Ok(response);
    }
}
