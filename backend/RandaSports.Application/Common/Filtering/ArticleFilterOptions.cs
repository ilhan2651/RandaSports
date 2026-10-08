namespace RandaSports.Application.Common.Filtering;

public sealed class ArticleFilterOptions
{
    public const string SectionName = "ArticleFilter";

    public bool Enabled { get; set; } = true;

    /// <summary>Başlıkta geçerse haber alınmaz (küçük harfe çevrilip aranır).</summary>
    public List<string> BlockedTitlePatterns { get; set; } =
    [
        "canlı izle",
        "canlı anlatım",
        "canlı yayın",
        "izleme ekranı",
        "nereden izlenir",
        "nasıl izlenir",
        "saat kaçta",
        "hangi kanalda",
        "şifreli mi",
        "şifresiz mi",
        "ne zaman oynanacak",
        "yayın akışı",
        "kimdir",
        "kaç yaşında",
        "nereli",
        "ne olur",
        "maçı kazanırsa",
        "berabere biterse",
        "muhtemel 11",
        "puan durumu nasıl",
        "biletleri ne zaman",
        "oynayacak mı",
        "oynuyor mu",
        "neden yok",
        "neden oynamıyor",
        "özeti izle",
        "golleri izle",
        "maç özeti"
    ];

    /// <summary>URL'de geçerse haber alınmaz: canlı anlatım blogları, foto galeriler, özet videoları.</summary>
    public List<string> BlockedUrlPatterns { get; set; } =
    [
        "canli-anlatim",
        "canli-izle",
        "canli-yayin",
        "/galeri-",
        "/foto/",
        "-izle-ekrani",
        "yayin-akisi",
        "mac-ozeti",
        "ozeti-izle",
        "golleri-izle"
    ];

    /// <summary>Başlıktaki büyük harf oranı bu değeri geçerse SEO içeriği sayılır.</summary>
    public double MaxUpperCaseRatio { get; set; } = 0.6;

    /// <summary>Büyük harf kontrolünün uygulanacağı en kısa başlık uzunluğu.</summary>
    public int UpperCaseCheckMinLength { get; set; } = 20;
}
