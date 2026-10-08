namespace RandaSports.Worker.Workers;

public sealed class StoryWriterOptions
{
    public const string SectionName = "StoryWriter";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 5;

    /// <summary>Bir turda en fazla kaç konu yazılacak.</summary>
    public int BatchSize { get; set; } = 5;

    /// <summary>İki konu arasındaki bekleme (saniye) — model kotasını aşmamak için.</summary>
    public int DelaySeconds { get; set; } = 4;

    /// <summary>Bir konu en fazla kaç kez denenecek.</summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>Bu kadar farklı kaynağa ulaşan konu hemen yazılır.</summary>
    public int MinSourceCount { get; set; } = 2;

    /// <summary>Tek kaynaklı konu, son haberden bu kadar dakika sonra yazılır.</summary>
    public int SingleSourceWaitMinutes { get; set; } = 20;
}
