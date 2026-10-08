namespace RandaSports.Application.Interfaces.Services;

public sealed record IssuedRefreshToken(string Token, string TokenHash, DateTimeOffset ExpiresAt);

/// <summary>
/// Yenileme jetonu üretir ve özetler. Veritabanına dokunmuyor: kaydı handler yazıyor,
/// böylece jeton üretimi ile saklama ayrı kalıyor.
/// </summary>
public interface IRefreshTokenService
{
    IssuedRefreshToken Issue();

    /// <summary>Elden gelen jetonu, tabloda aramak için aynı yöntemle özetler.</summary>
    string Hash(string token);
}
