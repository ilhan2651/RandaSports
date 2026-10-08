namespace RandaSports.Worker.Workers;

public sealed class CommentatorMaintenanceOptions
{
    public const string SectionName = "CommentatorMaintenance";

    public bool Enabled { get; set; } = true;

    /// <summary>İki tur arasındaki bekleme (dakika).</summary>
    public int IntervalMinutes { get; set; } = 60;

    /// <summary>Bir turda kaç kişinin rolü sorulacak. Tek modele tek istek gidiyor.</summary>
    public int RoleBatchSize { get; set; } = 10;

    /// <summary>Bir turda en fazla kaç kişi için portre aranacak.</summary>
    public int PortraitBatchSize { get; set; } = 5;

    /// <summary>Portre araması açık mı.</summary>
    public bool FetchPortraits { get; set; } = true;
}
