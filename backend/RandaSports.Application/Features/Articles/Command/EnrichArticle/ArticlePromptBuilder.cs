using System.Globalization;
using System.Text;
using RandaSports.Application.Interfaces.Services;

namespace RandaSports.Application.Features.Articles.Command.EnrichArticle;

internal static class ArticlePromptBuilder
{
    public const int MaxContentLength = 12000;

    private const string Template = """
        Sen bir spor gazetecisisin. Aşağıdaki haber metnini okuyup JSON döndüreceksin.

        İÇERİK KURALLARI:
        - SADECE aşağıdaki metinde yazanları kullan. Kendi bilgini, tahminini veya metinde olmayan hiçbir detayı ekleme.
        - Emin olamadığın alanları boş bırak.
        - "quotes" alanına SADECE metinde tırnak içinde geçen, kelimesi kelimesine alıntıları koy.
        __LANGUAGE__
        DEVAM EDEN OLAYLAR (çok önemli):
        - Metin canlı anlatım ya da devam eden bir olaysa ("devam ediyor", "ilk yarı", "şu ana kadar" gibi ifadeler
          ya da dakika dakika anlatım varsa) bunu AÇIKÇA belirt ve "isLive" alanını true yap.
        - Devam eden bir maçta skoru kesin sonuç gibi YAZMA. "3-0 kazandı" değil, "ilk yarı 3-0 önde tamamlandı"
          ya da "45. dakikada 3-0 önde" gibi, metindeki ana kadar olan durumu yaz.
        - Olay bitmişse "isLive" false olacak.

        YAZIM KURALLARI:
        - Gazeteci gibi yaz: kısa, net cümleler. Resmî yazışma dili kullanma.
          "ağırlamaktadır" değil "ağırladı", "bulunmuştur" değil "açıkladı", "gelmiştir" değil "geldi".
        - "Millîler", "Ay-yıldızlılar", "sarı-kırmızılılar", "Bizim Çocuklar", "temsilcimiz" gibi klişeleri kullanma.
          Takımın adını yaz.
        - "kritik mücadele", "dev maç", "zorlu rakip", "adeta" gibi doldurma ifadeler kullanma.

        BÜYÜK-KÜÇÜK HARF (dikkat):
        - Normal Türkçe yazım kurallarını uygula. Cümleler büyük harfle başlar.
        - Kişi, takım, kulüp, lig, stat ve şehir adları büyük harfle başlar:
          "Galatasaray", "Mustafa Gürsel", "Trendyol Süper Lig", "Uruguay", "Bursa".
        - Metnin tamamını küçük harfle YAZMA. Metnin tamamını BÜYÜK harfle de yazma.
        - Özel isimleri kaynak metinde nasıl yazıldıysa öyle yaz, harf değiştirme ("Uruguay" → "urugay" olmaz).

        ALAN ALAN NE YAZACAKSIN:
        - "headline": Haberin kendi başlığı. Olayı tek cümlede anlatsın, 60-90 karakter, BÜYÜK HARF kullanma,
          "saat kaçta / hangi kanalda / canlı izle" gibi arama motoru başlığı yazma.
        - "summary": Kart için 2-3 cümle. En önemli bilgi ilk cümlede olsun.
        - "body": Haber sayfası için 3-4 paragraf, toplam 150-250 kelime. Zengin ve okunur olsun:
          ne oldu, nasıl gelişti, kim ne dedi, sonrasında ne var. Metindeki somut detayları (dakika, skor, isim,
          rakam, tarih) kullan. Alıntı varsa paragrafın içinde tırnakla ver. Paragrafları boş satırla ayır.
          Metinde bu kadar bilgi yoksa daha kısa yaz, uydurma.
        - "analysis": Metinden çıkan somut bir sonuç varsa 1-2 cümle: sıralama değişikliği, seri, rekor, kadro etkisi.
          Somut bir şey yoksa BOŞ BIRAK. "Önemli bir gelişmedir", "büyük önem taşıyor" gibi içi boş cümle yazma.

        "sport" (branş — haberler bu alana göre kategoriye giriyor):
        - Haberin hangi spor branşında olduğunu AŞAĞIDAKİ LİSTEDEN seç ve slug'ı birebir yaz.
        - Listede olmayan bir slug UYDURMA. Branşı anlamadıysan ya da haber birden fazla branşı
          kapsıyorsa BOŞ BIRAK — boş bırakmak yanlış branşa koymaktan iyidir.
        - Olimpiyat, üniversite sporu gibi çok branşlı haberlerde boş bırak.
        - Seçilebilecek branşlar:
        __SPORTS__

        "teams" VE "athletes" (çok dikkatli doldur — haberler bu alanlara göre gruplanıyor):
        - "teams": SADECE haberin KONUSU olan takım, kulüp ya da millî takımlar. En fazla 3 tane.
        - Metinde adı geçtiği için ekleme. Bir oyuncunun eski ya da yeni kulübü, örnek verilen başka maçlar,
          karşılaştırma için anılan takımlar bu listeye GİRMEZ.
        - Sponsor, marka, şirket, tesis, mekan, organizasyon ya da turnuva adı takım DEĞİLDİR. Bunları yazma.
        - Millî takımı ülke adıyla yaz: "Uruguay", "Güney Kore", "Türkiye".
        - Maç haberinde iki tarafı da yaz, başka takım ekleme.
        - "athletes": SADECE haberin konusu olan sporcu ya da teknik direktörler. En fazla 3 tane.
          Haberde adı geçen ama olayla ilgisi olmayan kişileri yazma.
        - Haberin öznesi bir takım ya da sporcu değilse (tesis, organizasyon, genel haber) iki listeyi de boş bırak.

        JSON ŞEMASI:
        {
          "headline": "Kendi başlığımız",
          "summary": "2-3 cümlelik kart özeti",
          "body": "3-4 paragraflık haber metni",
          "analysis": "Somut sonuç varsa 1-2 cümle, yoksa boş",
          "isLive": false,
          "sport": "Yukarıdaki listeden bir slug, emin değilsen boş",
          "category": "transfer | mac-sonucu | sakatlik | aciklama | yonetim | diger",
          "teams": ["Haberin konusu olan takımlar, en fazla 3"],
          "athletes": ["Haberin konusu olan sporcu/teknik direktörler, en fazla 3"],
          "facts": ["Somut bilgiler: skor, dakika, bonservis, tarih gibi"],
          "quotes": [{ "speaker": "Konuşan kişi", "text": "Birebir alıntı" }]
        }

        HABER BAŞLIĞI: __TITLE__

        HABER ÖZETİ: __EXCERPT__

        HABER METNİ:
        __CONTENT__
        """;

    /// <summary>
    /// Yabancı kaynaklarda alıntı sorunu var: modelin çevirdiği alıntı orijinal metinde
    /// birebir geçmediği için doğrulamadan geçemiyor ve atılıyor. Üstelik Türkçe sitede
    /// İngilizce alıntı bloğu bozuk görünüyor. Bu yüzden çeviride alıntıyı boş bırakıp
    /// sözü haber metninin içinde aktarmasını istiyoruz.
    /// </summary>
    private const string ForeignLanguageRules = """

        DİL (kaynak Türkçe değil):
        - Kaynak metin __LANGUAGE_NAME__; SEN TÜRKÇE YAZACAKSIN. Çeviri değil, Türkçe haber yaz.
        - Kişi, takım, lig, turnuva ve stat adlarını ÇEVİRME: "Jon Jones", "Kansas City Chiefs",
          "Premier League", "Madison Square Garden".
        - Yerleşik Türkçe karşılığı olan terimleri Türkçe yaz: "head coach" → "teknik direktör",
          "world championship" → "dünya şampiyonası", "free agent" → "serbest oyuncu".
        - Ölçü birimlerini çevirme, metindeki haliyle yaz (pound, yard, mil).
        - "quotes" alanını BOŞ BIRAK. Söylenen sözü "body" içinde Türkçe olarak aktar:
          tırnak kullanmak yerine "... olduğunu söyledi" biçiminde yaz.

        """;

    public static string Build(
        string title,
        string? excerpt,
        string content,
        IReadOnlyList<SportEntry> sports,
        string? language = null)
    {
        var trimmed = content.Length > MaxContentLength ? content[..MaxContentLength] : content;

        return Template
            .Replace("__LANGUAGE__", BuildLanguageBlock(language))
            .Replace("__SPORTS__", BuildSportList(sports))
            .Replace("__TITLE__", title)
            .Replace("__EXCERPT__", string.IsNullOrWhiteSpace(excerpt) ? "-" : excerpt)
            .Replace("__CONTENT__", trimmed);
    }

    private static string BuildLanguageBlock(string? language)
    {
        if (IsTurkish(language))
            return string.Empty;

        return ForeignLanguageRules.Replace("__LANGUAGE_NAME__", LanguageName(language));
    }

    private static bool IsTurkish(string? language) =>
        string.IsNullOrWhiteSpace(language)
        || language.StartsWith("tr", StringComparison.OrdinalIgnoreCase);

    private static string LanguageName(string? language) => language!.ToLowerInvariant() switch
    {
        var x when x.StartsWith("en", StringComparison.Ordinal) => "İngilizce",
        var x when x.StartsWith("de", StringComparison.Ordinal) => "Almanca",
        var x when x.StartsWith("fr", StringComparison.Ordinal) => "Fransızca",
        var x when x.StartsWith("es", StringComparison.Ordinal) => "İspanyolca",
        var x when x.StartsWith("it", StringComparison.Ordinal) => "İtalyanca",
        _ => "Türkçe değil"
    };

    /// <summary>Slug'ları listeden seçtirmek uydurma branş adlarını engelliyor.</summary>
    private static string BuildSportList(IReadOnlyList<SportEntry> sports)
    {
        if (sports.Count == 0)
            return "  (branş listesi boş — bu alanı boş bırak)";

        var builder = new StringBuilder();

        foreach (var sport in sports)
            builder.AppendLine(CultureInfo.InvariantCulture, $"  - {sport.Slug} ({sport.Name})");

        return builder.ToString().TrimEnd();
    }
}
