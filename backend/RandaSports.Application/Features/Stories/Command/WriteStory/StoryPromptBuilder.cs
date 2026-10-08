using System.Globalization;
using System.Text;

namespace RandaSports.Application.Features.Stories.Command.WriteStory;

internal static class StoryPromptBuilder
{
    public const int MaxSourceLength = 3000;

    private const string Template = """
        Sen bir spor gazetecisisin. Aşağıda AYNI OLAYI anlatan birden fazla kaynak var.
        Bunlardan TEK BİR haber yazacaksın ve JSON döndüreceksin.

        İÇERİK KURALLARI:
        - SADECE aşağıdaki kaynaklarda yazanları kullan. Kendi bilgini veya tahminini ekleme.
        - Kaynaklarda tekrar eden bilgiyi bir kez yaz. Her kaynağın getirdiği farklı detayı birleştir.
        - Kaynaklar çelişiyorsa (farklı skor, farklı rakam, farklı iddia) uydurup ortalama alma:
          metinde "X kaynağına göre ..., Y kaynağına göre ..." diye belirt ve "conflicts" alanına yaz.
        - Kaynak adı geçirmen gereken tek yer budur; onun dışında haber tek ağızdan yazılır.
        - "quotes" alanına SADECE kaynaklarda tırnak içinde geçen, kelimesi kelimesine alıntıları koy.

        DEVAM EDEN OLAYLAR (çok önemli):
        - Kaynaklardan biri bile olayın sürdüğünü söylüyorsa ("ilk yarı", "devam ediyor", "şu ana kadar")
          olayı bitmiş gibi ANLATMA ve "isLive" alanını true yap.
        - Devam eden maçta skoru kesin sonuç gibi yazma: "3-0 kazandı" değil, "ilk yarıyı 3-0 önde kapattı".
        - En güncel kaynak olayın bittiğini söylüyorsa "isLive" false olacak ve haber sonucu anlatacak.

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
        - Özel isimleri kaynaklarda nasıl yazıldıysa öyle yaz, harf değiştirme ("Uruguay" → "urugay" olmaz).

        ALAN ALAN NE YAZACAKSIN:
        - "headline": Olayı tek cümlede anlatan başlık, 60-90 karakter. Normal yazım kurallarıyla yaz.
          "saat kaçta / hangi kanalda / canlı izle" gibi arama motoru başlığı yazma.
        - "summary": Kart için 2-3 cümle. En önemli bilgi ilk cümlede olsun.
        - "body": Haber sayfası için 4-6 paragraf, toplam 250-400 kelime. Kaynakların tamamındaki somut
          detayları (dakika, skor, isim, rakam, tarih, alıntı) kullan: ne oldu, nasıl gelişti, kim ne dedi,
          sonrasında ne var. Alıntıları paragraf içinde tırnakla ver. Paragrafları boş satırla ayır.
          Kaynaklarda bu kadar bilgi yoksa daha kısa yaz, uydurma.
        - "analysis": Kaynaklardan çıkan somut bir sonuç varsa 1-2 cümle: sıralama değişikliği, seri, rekor,
          kadro etkisi. Somut bir şey yoksa BOŞ BIRAK. "Önemli bir gelişmedir" gibi içi boş cümle yazma.
        - "conflicts": Kaynaklar arasındaki çelişkiler; yoksa boş dizi.

        JSON ŞEMASI:
        {
          "headline": "Haberin başlığı",
          "summary": "2-3 cümlelik kart özeti",
          "body": "4-6 paragraflık haber metni",
          "analysis": "Somut sonuç varsa 1-2 cümle, yoksa boş",
          "isLive": false,
          "category": "transfer | mac-sonucu | sakatlik | aciklama | yonetim | diger",
          "conflicts": ["Kaynaklar arasındaki çelişki varsa"],
          "quotes": [{ "speaker": "Konuşan kişi", "text": "Birebir alıntı" }]
        }

        KAYNAKLAR:
        __SOURCES__
        """;

    public static string Build(IReadOnlyList<StorySourceInput> sources)
    {
        var builder = new StringBuilder();

        for (var i = 0; i < sources.Count; i++)
        {
            var source = sources[i];
            var text = string.IsNullOrWhiteSpace(source.Body) ? source.Summary : source.Body;
            text ??= string.Empty;

            if (text.Length > MaxSourceLength)
                text = text[..MaxSourceLength];

            builder.AppendLine(CultureInfo.InvariantCulture, $"--- KAYNAK {i + 1}: {source.SourceName} ---");
            builder.AppendLine(CultureInfo.InvariantCulture, $"Yayın zamanı: {source.PublishedAt:yyyy-MM-dd HH:mm}");
            builder.AppendLine(CultureInfo.InvariantCulture, $"Başlık: {source.Title}");

            if (source.IsLive)
                builder.AppendLine("Not: Bu kaynak olayın devam ettiğini söylüyor.");

            builder.AppendLine("Metin:");
            builder.AppendLine(text);
            builder.AppendLine();
        }

        return Template.Replace("__SOURCES__", builder.ToString());
    }
}
