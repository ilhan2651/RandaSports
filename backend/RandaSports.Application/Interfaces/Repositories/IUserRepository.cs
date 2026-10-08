using RandaSports.Domain.Entities;

namespace RandaSports.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Rolleriyle birlikte, takip edilen hâlde döner: girişte son giriş zamanı ve
    /// hatalı deneme sayacı güncelleniyor, jetona da roller yazılıyor.
    /// </summary>
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Takip listeleriyle birlikte; tercih ekranı bunu okuyup yazıyor.</summary>
    Task<User?> GetWithPreferencesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<Role?> GetRoleByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>Yenileme jetonunu özetinden bulur; kullanıcısı ve rolleri yüklü gelir.</summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
