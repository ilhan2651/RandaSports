using Microsoft.Extensions.Logging;

namespace RandaSports.Infrastructure.Ai;

/// <summary>
/// Kota (429) görüldüğünde Gemini çağrılarını topluca durduran anahtar.
///
/// Günlük kota saatlerce geri gelmiyor; bu sürede her worker turunda yeniden
/// denemek yalnızca log üretiyor ve her haberin deneme hakkını yakıyordu.
/// Devre açıkken çağrılar HTTP'ye hiç çıkmadan "geçici hata" olarak dönüyor,
/// böylece çağıran taraf işi bir sonraki tura bırakıyor.
///
/// Beş worker aynı anahtarı paylaşıyor: biri kotaya takıldığında hepsi duruyor.
/// </summary>
public sealed class GeminiCircuitBreaker(TimeProvider timeProvider, ILogger<GeminiCircuitBreaker> logger)
{
    private long _blockedUntilUtcTicks;

    /// <summary>Devre açıksa true; <paramref name="remaining"/> kalan süreyi verir.</summary>
    public bool IsOpen(out TimeSpan remaining)
    {
        var until = Interlocked.Read(ref _blockedUntilUtcTicks);
        var now = timeProvider.GetUtcNow().UtcTicks;

        if (until > now)
        {
            remaining = TimeSpan.FromTicks(until - now);
            return true;
        }

        remaining = TimeSpan.Zero;
        return false;
    }

    /// <summary>Devreyi belirtilen süre boyunca açar.</summary>
    public void Trip(TimeSpan duration)
    {
        var now = timeProvider.GetUtcNow();
        var until = now.Add(duration).UtcTicks;
        var previous = Interlocked.Exchange(ref _blockedUntilUtcTicks, until);

        // Yalnızca kapalıyken açılırken logluyoruz: kota dolu geçen saatlerde
        // her çağrı için bir satır yazmanın anlamı yok.
        if (previous <= now.UtcTicks)
            logger.LogWarning(
                "Gemini kotası doldu. Devre {Minutes:F0} dakika açık kalacak.",
                duration.TotalMinutes);
    }

    /// <summary>Başarılı bir çağrıdan sonra devreyi kapatır.</summary>
    public void Close()
    {
        if (Volatile.Read(ref _blockedUntilUtcTicks) == 0)
            return;

        if (Interlocked.Exchange(ref _blockedUntilUtcTicks, 0) != 0)
            logger.LogInformation("Gemini yeniden cevap veriyor, devre kapandı.");
    }
}
