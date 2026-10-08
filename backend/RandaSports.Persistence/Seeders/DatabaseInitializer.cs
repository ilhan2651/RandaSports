using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RandaSports.Persistence.Contexts;

namespace RandaSports.Persistence.Seeders;

/// <summary>
/// Açılışta veritabanını hizaya getirir: bekleyen migration'ları uygular ve
/// entity'lerde migration'a yansımamış değişiklik varsa uyarır.
/// </summary>
public class DatabaseInitializer(RandaSportsDbContext context, ILogger<DatabaseInitializer> logger)
{
    private const int WaitSeconds = 60;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);

    public async Task RunAsync(bool applyMigrations = true, CancellationToken cancellationToken = default)
    {
        if (applyMigrations)
            await ApplyAsync(cancellationToken);
        else
            await WaitForMigrationsAsync(cancellationToken);

        WarnIfModelChanged();
    }

    private async Task ApplyAsync(CancellationToken cancellationToken)
    {
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
            return;

        logger.LogInformation(
            "{Count} migration uygulanıyor: {Migrations}",
            pending.Count,
            string.Join(", ", pending));

        await context.Database.MigrateAsync(cancellationToken);

        logger.LogInformation("Migrationlar uygulandı.");
    }

    /// <summary>
    /// Migration'ı API uyguluyor. Worker aynı anda ayağa kalkarsa eski şemayla çalışmasın diye
    /// kısa süre bekliyor; bu süre dolarsa uyarıp devam ediyor.
    /// </summary>
    private async Task WaitForMigrationsAsync(CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(WaitSeconds);
        var warned = false;

        while (true)
        {
            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

            if (pending.Count == 0)
            {
                if (warned)
                    logger.LogInformation("Veritabanı hazır, devam ediliyor.");

                return;
            }

            if (DateTimeOffset.UtcNow >= deadline)
            {
                logger.LogWarning(
                    "Veritabanı hâlâ geride: {Count} migration uygulanmamış. API'yi çalıştır ya da 'dotnet ef database update' yap.",
                    pending.Count);

                return;
            }

            if (!warned)
            {
                logger.LogWarning(
                    "{Count} migration uygulanmamış, API'nin uygulaması bekleniyor...",
                    pending.Count);

                warned = true;
            }

            await Task.Delay(PollInterval, cancellationToken);
        }
    }

    /// <summary>
    /// Migration üretmek tasarım zamanı işi; uygulama bunu kendi yapamaz.
    /// Yapabileceği şey, unuttuğunu açılışta yüzüne söylemek.
    /// </summary>
    private void WarnIfModelChanged()
    {
        try
        {
            if (!context.Database.HasPendingModelChanges())
                return;

            logger.LogWarning(
                "DİKKAT: Entity'lerde migration'a yansımamış değişiklik var. Şunu çalıştır: "
                + "dotnet ef migrations add <ad> -p RandaSports.Persistence -s RandaSports.Api");
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Model değişiklik kontrolü yapılamadı.");
        }
    }
}
