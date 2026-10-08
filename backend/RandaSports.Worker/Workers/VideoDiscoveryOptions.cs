namespace RandaSports.Worker.Workers;

public sealed class VideoDiscoveryOptions
{
    public const string SectionName = "VideoDiscovery";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 30;

    /// <summary>Bir turda en fazla kaç kanal taranacak.</summary>
    public int BatchSize { get; set; } = 4;

    /// <summary>Bir kanala tekrar bakmak için geçmesi gereken süre (dakika).</summary>
    public int RecheckMinutes { get; set; } = 120;

    /// <summary>Bu günden eski videolar alınmıyor.</summary>
    public int MaxAgeDays { get; set; } = 3;

    /// <summary>Bir taramada bir kanaldan en fazla kaç video alınacak.</summary>
    public int MaxPerChannel { get; set; } = 4;

    /// <summary>İki kanal arasındaki bekleme (saniye).</summary>
    public int DelaySeconds { get; set; } = 3;

    /// <summary>Bundan kısa videolar alınmıyor — jenerik, fragman, duyuru.</summary>
    public int MinDurationSeconds { get; set; } = 20;

    /// <summary>
    /// Bundan uzun videolar alınmıyor. 60 dakikalık bir yayın modelin günlük
    /// ücretsiz video bütçesinin önemli bir kısmını tek başına yiyor.
    /// </summary>
    public int MaxDurationSeconds { get; set; } = 3600;

    /// <summary>Devam eden yayınları atla; model yarım yayını düzgün izleyemiyor.</summary>
    public bool SkipLive { get; set; } = true;
}
