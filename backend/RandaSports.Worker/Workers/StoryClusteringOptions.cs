namespace RandaSports.Worker.Workers;

public sealed class StoryClusteringOptions
{
    public const string SectionName = "StoryClustering";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 2;

    /// <summary>Bir turda en fazla kaç haber kümelenecek.</summary>
    public int BatchSize { get; set; } = 50;
}
