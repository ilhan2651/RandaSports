namespace RandaSports.Worker.Workers;

public sealed class OpinionExtractionOptions
{
    public const string SectionName = "OpinionExtraction";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 20;

    /// <summary>
    /// Bir turda en fazla kaç video işlenecek. Video analizi modelin en pahalı
    /// işlemi; ücretsiz kotayı tüketmemek için küçük tutuluyor.
    /// </summary>
    public int BatchSize { get; set; } = 2;

    /// <summary>Bir video en fazla kaç kez denenecek.</summary>
    public int MaxAttempts { get; set; } = 2;

    /// <summary>İki video arasındaki bekleme (saniye).</summary>
    public int DelaySeconds { get; set; } = 20;

    /// <summary>Görüş, bu kadar günlük haberlerle eşleştirilmeye çalışılıyor.</summary>
    public int StoryMatchDays { get; set; } = 4;

    /// <summary>
    /// Kısa videolar önce işlensin mi. Shorts genelde tek yorumcunun tek görüşü,
    /// konuşmacının adı da başlıkta; aynı kotayla kat kat fazla görüş çıkıyor.
    /// Uzun programları öne almak istersen false yap.
    /// </summary>
    public bool PreferShortVideos { get; set; } = true;
}
