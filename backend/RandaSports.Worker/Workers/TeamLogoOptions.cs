namespace RandaSports.Worker.Workers;

public sealed class TeamLogoOptions
{
    public const string SectionName = "TeamLogo";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// İki tur arasındaki bekleme. Takım kümesi kapalı ve yavaş değişiyor, o yüzden
    /// sık çalışmasına gerek yok; bir kez doldurduktan sonra turlar boş geçiyor.
    /// </summary>
    public int IntervalMinutes { get; set; } = 180;

    /// <summary>Bir turda kaç takım denenecek. Her takım iki Wikidata isteği demek.</summary>
    public int BatchSize { get; set; } = 10;
}
