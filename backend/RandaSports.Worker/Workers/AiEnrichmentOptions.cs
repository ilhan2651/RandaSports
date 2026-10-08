namespace RandaSports.Worker.Workers;

public sealed class AiEnrichmentOptions
{
    public const string SectionName = "AiEnrichment";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 5;

    /// <summary>Bir turda en fazla kaç haber işlenecek.</summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>İki haber arasındaki bekleme (saniye) — model kotasını aşmamak için.</summary>
    public int DelaySeconds { get; set; } = 4;

    /// <summary>Bir haber en fazla kaç kez denenecek.</summary>
    public int MaxAttempts { get; set; } = 3;
}
