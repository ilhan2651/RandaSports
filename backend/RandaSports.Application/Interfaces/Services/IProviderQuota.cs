namespace RandaSports.Application.Interfaces.Services;

/// <summary>
/// Günlük istek bütçesi. Ücretsiz planda 100 istek var; bunu aşan her çağrı
/// hata dönüyor, o yüzden çağrıdan ÖNCE izin alıyoruz.
/// </summary>
public interface IProviderQuota
{
    /// <returns>Bütçe yeterliyse true ve sayaç artar; yetmiyorsa false.</returns>
    Task<bool> TryConsumeAsync(string provider, int cost = 1, CancellationToken cancellationToken = default);

    Task<int> RemainingAsync(string provider, CancellationToken cancellationToken = default);
}
