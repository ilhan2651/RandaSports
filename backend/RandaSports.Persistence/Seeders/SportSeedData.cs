namespace RandaSports.Persistence.Seeders;

/// <param name="Slug">Adresin parçası: /futbol, /formula-1. Bir kez yayınlandıktan sonra DEĞİŞTİRİLMEZ.</param>
public sealed record SportSeed(string Name, string Slug, int DisplayOrder);

/// <summary>
/// Branş listesi. Sıra menüdeki sırayı belirliyor: önce Türkiye'de en çok okunanlar.
/// Yeni branş eklemek buraya bir satır yazmak; seed yalnızca eksikleri ekliyor,
/// mevcutlara dokunmuyor.
/// </summary>
public static class SportSeedData
{
    public static readonly SportSeed[] Sports =
    [
        new("Futbol", "futbol", 1),
        new("Basketbol", "basketbol", 2),
        new("Voleybol", "voleybol", 3),
        new("MMA", "mma", 4),
        new("Amerikan Futbolu", "amerikan-futbolu", 5),
        new("Formula 1", "formula-1", 6),
        new("MotoGP", "motogp", 7),
        new("Tenis", "tenis", 8),
        new("Boks", "boks", 9),
        new("Güreş", "gures", 10),
        new("Atletizm", "atletizm", 11),
        new("Buz Hokeyi", "buz-hokeyi", 12),
        new("Beyzbol", "beyzbol", 13),
        new("E-spor", "espor", 14)
    ];
}
