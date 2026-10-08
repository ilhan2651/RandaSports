using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RandaSports.Application.Interfaces.Services;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Quotas;

public class ProviderQuotaService(
    RandaSportsDbContext context,
    TimeProvider timeProvider,
    ILogger<ProviderQuotaService> logger) : IProviderQuota
{
    /// <summary>Ayarlardan gelmezse ücretsiz planın günlük sınırı.</summary>
    public const int DefaultDailyLimit = 100;

    public async Task<bool> TryConsumeAsync(
        string provider,
        int cost = 1,
        CancellationToken cancellationToken = default)
    {
        var day = Today();

        // Tek SQL ile hem oluştur hem artır: iki worker aynı anda istese de sayaç şaşmaz.
        // Sınır aşılıyorsa WHERE tutmaz, hiçbir satır güncellenmez ve false döneriz.
        var affected = await context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO provider_quotas (id, provider, day, used, daily_limit, created_at, updated_at)
             VALUES ({Guid.CreateVersion7()}, {provider}, {day}, {cost}, {DefaultDailyLimit}, now(), now())
             ON CONFLICT (provider, day) DO UPDATE
             SET used = provider_quotas.used + {cost}, updated_at = now()
             WHERE provider_quotas.used + {cost} <= provider_quotas.daily_limit
             """,
            cancellationToken);

        if (affected > 0)
            return true;

        logger.LogWarning("{Provider} günlük istek kotası doldu, çağrı atlandı.", provider);
        return false;
    }

    public async Task<int> RemainingAsync(string provider, CancellationToken cancellationToken = default)
    {
        var day = Today();

        var quota = await context.ProviderQuotas
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Provider == provider && x.Day == day, cancellationToken);

        return quota is null ? DefaultDailyLimit : Math.Max(0, quota.DailyLimit - quota.Used);
    }

    /// <summary>Sağlayıcının kotası UTC gününe göre sıfırlanıyor.</summary>
    private DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
}
