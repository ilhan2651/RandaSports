namespace RandaSports.Persistence.Seeders;

/// <param name="SportSlug">Haberler bu branşa yazılır; kaynak tek branşa yayın yapmıyorsa seed'e eklenmez.</param>
/// <param name="Language">Metnin dili. İngilizce kaynakta model yazıyı Türkçeye çeviriyor.</param>
/// <param name="Priority">Görsel yarışında ağırlık. Yabancı kaynaklar 0'da kalıyor ki
/// elle eklenen Türkçe kaynakların görselini geçmesinler.</param>
public sealed record SourceSeed(
    string Name,
    string Url,
    string SportSlug,
    string Language = "en",
    int FetchIntervalMinutes = 60,
    int Priority = 0);

/// <summary>
/// Türk medyasının düzenli takip etmediği branşlar için beslemeler.
///
/// Genel Türkçe spor kaynakları listede YOK: onlar yönetim ucundan elle ekleniyor ve
/// tek branşa bağlanamıyor. Voleybol, güreş, atletizm ve e-spor haberleri Türk
/// medyasında zaten var; onları kategoriye oturtan şey yeni kaynak değil, haberin
/// branşını metinden bulan AI adımı.
///
/// Adreslerin tamamı tarayıcıdan tek tek denendi (4 Ekim 2026): hepsi 200 dönüyor ve
/// gerçek haber içeriyor. Tahmin edilen hiçbir adres kalmadı.
/// </summary>
public static class SourceSeedData
{
    public static readonly SourceSeed[] Sources =
    [
        // Formula 1 ve MotoGP — Motorsport.com'un Türkçe sürümü. Çeviriye hiç girmiyoruz,
        // isimler ve terimler zaten Türkçe geliyor; model kotası da boşa gitmiyor.
        new("Motorsport.com Türkiye F1", "https://tr.motorsport.com/rss/f1/news/", "formula-1",
            Language: "tr", FetchIntervalMinutes: 30, Priority: 5),
        new("Motorsport.com Türkiye MotoGP", "https://tr.motorsport.com/rss/motogp/news/", "motogp",
            Language: "tr", FetchIntervalMinutes: 30, Priority: 5),

        // MMA — UFC takvimini Türk medyası ancak büyük maçlarda haber yapıyor.
        new("MMA Fighting", "https://www.mmafighting.com/rss/index.xml", "mma", FetchIntervalMinutes: 30),
        new("ESPN MMA", "https://www.espn.com/espn/rss/mma/news", "mma", FetchIntervalMinutes: 30),

        new("Bad Left Hook", "https://www.badlefthook.com/rss/index.xml", "boks"),
        new("ESPN Boxing", "https://www.espn.com/espn/rss/boxing/news", "boks"),

        // Amerikan futbolu — NFL için Türkçe düzenli yayın yok.
        new("ESPN NFL", "https://www.espn.com/espn/rss/nfl/news", "amerikan-futbolu", FetchIntervalMinutes: 30),

        new("ESPN NHL", "https://www.espn.com/espn/rss/nhl/news", "buz-hokeyi"),
        new("ESPN MLB", "https://www.espn.com/espn/rss/mlb/news", "beyzbol"),
        new("ESPN Tennis", "https://www.espn.com/espn/rss/tennis/news", "tenis")
    ];
}
