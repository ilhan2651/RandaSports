using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Infrastructure.Security;

/// <summary>
/// Jeton 256 bit rastgele; tahmin edilemez olması tek güvenlik şartı. Tabloda
/// yalnızca SHA-256 özeti duruyor. Parolada PBKDF2 kullanmamızın sebebi parolaların
/// tahmin edilebilir olması; burada öyle bir sorun yok, tek geçişli özet yeterli
/// ve her istekte çalıştığı için hızlı olması gerekiyor.
/// </summary>
public sealed class RefreshTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
    : IRefreshTokenService
{
    private const int TokenBytes = 32;
    private readonly JwtOptions _options = options.Value;

    public IssuedRefreshToken Issue()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes));

        return new IssuedRefreshToken(
            token,
            Hash(token),
            timeProvider.GetUtcNow().AddDays(_options.RefreshLifetimeDays));
    }

    public string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
