namespace RandaSports.Worker.Workers;

public sealed class QuoteCheckOptions
{
    public const string SectionName = "QuoteCheck";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 10;

    /// <summary>
    /// Bir turda en fazla kaç görüş kontrol edilecek. Her kontrol kısa bir pencere
    /// olduğu için video çıkarmaya göre ucuz; yine de kotayı çıkarma turuyla
    /// paylaşıyor, o yüzden ölçülü.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>İki kontrol arasındaki bekleme (saniye).</summary>
    public int DelaySeconds { get; set; } = 5;

    /// <summary>
    /// Damganın iki yanında kaç saniyeye bakılacak. Damga zaten kayabildiği için
    /// pencere sözün öncesini ve sonrasını da kapsıyor. 20 saniye, 40 dakikalık bir
    /// videonun yaklaşık yüzde biri.
    /// </summary>
    public int WindowSeconds { get; set; } = 20;
}
