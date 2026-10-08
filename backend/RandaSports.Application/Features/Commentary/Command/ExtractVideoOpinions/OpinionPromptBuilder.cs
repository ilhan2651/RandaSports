using System.Text;

namespace RandaSports.Application.Features.Commentary.Command.ExtractVideoOpinions;

internal static class OpinionPromptBuilder
{
    public const int MaxOpinions = 8;

    private const string Template = """
        Sana bir spor yorum videosu veriyorum. Videoyu izle ve konuşmacıların
        NET GÖRÜŞLERİNİ çıkar. Cevabı JSON olarak ver.

        EN ÖNEMLİ KURAL:
        - Sadece videoda GERÇEKTEN söylenenleri yaz. Söylenmeyen hiçbir şeyi ekleme,
          tamamlama, yumuşatma veya kendi yorumunu katma.
        - Videoyu izleyemediysen veya ses anlaşılmıyorsa "opinions" alanını boş dizi
          olarak döndür. Boş dönmek, uydurmaktan iyidir.

        NEYİ ALACAKSIN — GÖRÜŞ İLE HABERİ AYIRMAK EN KRİTİK İŞİN:
        - Bir takım, oyuncu, teknik direktör, hakem kararı veya maç hakkında
          konuşmacının KENDİ DEĞERLENDİRMESİNİ ya da TAHMİNİNİ al. Bunlar "gorus".
        - Olay aktarımı GÖRÜŞ DEĞİLDİR, "haber"dir. Muhabirin sahadan/adliyeden
          bildirdiği, "kim gözaltına alındı", "kim nereye getirilecek", "hangi karar
          açıklandı", "maç kaç kaç bitti", "şu oyuncu şu kulübe transfer oldu" gibi
          OLAY BİLDİREN konuşmalar haberdir. Konuşan kişi ünlü bir yorumcu olsa bile
          o cümlede değerlendirme yoksa "haber" yaz.
        - Sunucunun sorusu "soru"dur. Skor okuma, reklam, jenerik, araya giren kısa
          onay sözleri ("evet", "aynen") hiçbiri alınmaz.
        - Basit ayırt etme ölçütü: cümle "oldu/olacak/açıklandı" diyorsa haberdir;
          "bence/iyi/kötü/yanlış/yapmalı/eder/edemez" diyorsa görüştür.
        - "kind" alanına bunu yaz: "gorus" | "haber" | "soru". Biz sadece "gorus"
          olanları yayınlıyoruz, ama emin değilsen sil deme — doğru etiketi yaz, gerisini biz hallederiz.
        - Aynı kişinin aynı konudaki birden fazla cümlesini TEK görüşte topla.
        - En fazla {{MAX}} görüş döndür. Videoda bu kadar yoksa daha az döndür.

        KONUŞMACI (en sık hata burada yapılıyor, dikkatle oku):
        - Konuşmacının adını SADECE videonun içinden belirle: alt bant, ekrandaki isim,
          birinin ona ismiyle hitap etmesi, kendini tanıtması.
        - AŞAĞIDAKİ AÇIKLAMA METNİ GENELLİKLE PROGRAMIN TÜM KONUKLARINI SAYAR.
          Oradaki isim listesi kimin konuştuğunu GÖSTERMEZ. Listeden isim SEÇME.
          Üç isim yazıyorsa, konuşan o üçünden biri olmayabilir bile.
        - Başlıktaki isme DİKKAT: başlıkta geçen isim çoğu zaman videonun KONUSUDUR,
          konuşan kişi değildir. "Beşiktaş'ın yeni transferi | Clarke-Harris" başlığında
          Clarke-Harris konuşmuyor, hakkında konuşuluyor. Bir ismi başlıktan alıp
          konuşmacı yazacaksan, o kişinin videoda GERÇEKTEN konuştuğunu görmüş olmalısın.
          Oyuncu, teknik direktör ve hakem adlarını konuşmacı sanma — onlar genelde konudur.
        - EMİN DEĞİLSEN "speaker" alanını boş bırak ve "speakerConfidence" değerini düşük ver.
          İsim TAHMİN ETME; yanlış isim, isimsiz görüşten çok daha kötü.
        - "speakerConfidence": 0 ile 1 arasında. 1 = alt bantta/başlıkta adını gördüm,
          0.5 = sesinden/hitaptan çıkardım, 0.2 = tahmin. Açıklamadaki listeden
          seçtiysen bu bir tahmindir, 0.2 ver.

        - "speakerSource": İsmi NEREDEN bulduğunu yaz. Bu alan güven alanından daha
          önemli; dürüst ol, çünkü buna göre ismi yayına alıp almayacağımıza karar veriyoruz.
            "altbant"  → ekranda/alt bantta yazan ismi okudum
            "baslik"   → videonun başlığında yazıyordu
            "hitap"    → biri ona ismiyle seslendi ya da kendini tanıttı
            "aciklama" → video açıklamasındaki isim listesinden seçtim
            "tahmin"   → yüzünden/sesinden tanıdığımı sanıyorum, yazılı bir kanıt yok
          Yüze bakıp tanıdığını düşünmek "tahmin"dir, "altbant" değildir.
          İsim bulamadıysan "speaker" boş, "speakerSource" da "tahmin" olsun.

        ALANLAR:
        - "topic": Görüşün konusu, kısa bir cümle. Örnek: "Galatasaray'ın savunma dizilişi".
        - "summary": Konuşmacının ne dediği VE neden dediği, 2-4 cümle. Gerekçesini de yaz:
          "eleştirdi" demekle yetinme, neyi niçin eleştirdiğini anlat. Kendi sözlerine yakın dur.
        - "quote": Konuşmacının o konudaki sözlerinin BİREBİR dökümü. Tek cümle DEĞİL:
          görüşünü anlattığı kesitin tamamı, genelde 2-5 cümle. Okuyan kişi videoyu
          açmadan ne demek istediğini anlamalı.
          - Kelime ekleme, düzeltme, özetleme yapma; araya kendi cümleni koyma.
          - Konuşmacı o konuyu bitirip başka konuya geçtiğinde alıntıyı orada bitir.
          - Araya başka biri girip konuşuyorsa onun sözlerini alıntıya KATMA.
          - Duymadığın ya da emin olmadığın kısmı yazma: emin olduğun yere kadar yaz,
            eksik bırakmak uydurmaktan iyidir.
          - "..." ile kırpma. Alıntı tam bir cümleyle başlasın, tam bir cümleyle bitsin.
          - Küfür/argo varsa olduğu gibi yazma, o görüşü hiç almaz.
        - "timestamp": Görüşün videoda başladığı an, SANİYE cinsinden tam sayı. Örnek: "738".
        - "stance": Konu hakkındaki tutum — "olumlu", "olumsuz" veya "notr".
        - "prediction": Tahmin varsa kısa cümle ("Beşiktaş ilk 3'e girer"); yoksa boş bırak.
        - "subjects": Görüşün konusu olan takım ve kişi adları, listede.
        - "sport": Görüşün HANGİ BRANŞA ait olduğu. Aşağıdaki listeden tam olarak bir
          adres yaz. Listede olmayan bir şey yazma. Emin değilsen boş bırak —
          yanlış branş, branşsız görüşten kötüdür.

        BRANŞ LİSTESİ (yalnızca bunlardan birini yaz):
        {{SPORTS}}

        {{CHANNEL_SPORTS}}

        {{SPEAKER_HEADING}}
        {{SPEAKERS}}
        {{SPEAKER_NOTE}}
        Listede olmayan biri de konuşabilir; o zaman duyduğun adı yaz.
        Listedeki adı sırf listede olduğu için SEÇME.

        VİDEO BİLGİSİ:
        {{VIDEO}}

        JSON ŞEMASI:
        {
          "videoSummary": "Videonun 1-2 cümlelik özeti",
          "opinions": [
            {
              "kind": "gorus",
              "speaker": "Konuşmacının adı veya boş",
              "speakerConfidence": 0.9,
              "speakerSource": "altbant",
              "topic": "Görüşün konusu",
              "summary": "Ne dediği ve neden dediği, 2-4 cümle",
              "quote": "Konuşmacının o konudaki sözlerinin tamamı, birebir, 2-5 cümle",
              "timestamp": "738",
              "stance": "olumsuz",
              "prediction": "",
              "sport": "futbol",
              "subjects": ["Galatasaray"]
            }
          ]
        }
        """;

    /// <param name="isChannelRoster">
    /// true: liste bu kanalda konuştuğu insan tarafından doğrulanmış kişilerden oluşuyor.
    /// false: liste sistemdeki tüm yorumcular, kanalla bağı yok.
    /// </param>
    /// <param name="sportSlugs">Sistemdeki tüm branş adresleri; model bunlardan birini seçiyor.</param>
    /// <param name="channelSportSlugs">
    /// Kanalın kapsadığı branşlar. Kısıt değil, ipucu: kanalın alışılmış konuları
    /// belliyse model doğru branşı daha kolay buluyor. Yine de kanalın listesinde
    /// olmayan bir branş çıkabilir — tek konuluk bir video olabilir.
    /// </param>
    public static string Build(
        string videoTitle,
        string? videoDescription,
        IReadOnlyList<string> speakerNames,
        bool isChannelRoster,
        IReadOnlyList<string> sportSlugs,
        IReadOnlyList<string> channelSportSlugs)
    {
        var speakers = speakerNames.Count == 0
            ? "(liste boş — duyduğun adı yaz)"
            : string.Join(", ", speakerNames);

        var heading = isChannelRoster
            ? "BU KANALDA DAHA ÖNCE KONUŞTUĞU DOĞRULANMIŞ KİŞİLER:"
            : "SİSTEMDE KAYITLI YORUMCULAR (bu kanalla bağları doğrulanmadı):";

        var note = isChannelRoster
            ? "Bu liste güvenilir: aşağıdaki isimler bu kanalın yayınlarında bir insan tarafından teyit edildi."
            : "Bu liste ZAYIF bir ipucudur: bu kişilerin bu kanalda konuştuğu doğrulanmadı. Buradan isim seçmen tek başına kanıt sayılmaz, 'speakerSource' alanına 'tahmin' yaz.";

        var video = new StringBuilder();
        video.Append("Başlık: ").AppendLine(videoTitle);

        if (!string.IsNullOrWhiteSpace(videoDescription))
        {
            var description = videoDescription.Length > 600
                ? videoDescription[..600]
                : videoDescription;

            video.Append("Açıklama: ").AppendLine(description);
        }

        var channelSports = channelSportSlugs.Count == 0
            ? "Bu kanal farklı branşlarda yayın yapıyor; branşı videonun içeriğinden belirle."
            : $"BU KANAL GENELLİKLE ŞU BRANŞLARI İŞLİYOR: {string.Join(", ", channelSportSlugs)}. "
              + "Bu bir ipucu, kural değil — video başka bir branştan olabilir, o zaman doğrusunu yaz.";


        return Template
            .Replace("{{MAX}}", MaxOpinions.ToString())
            .Replace("{{SPEAKER_HEADING}}", heading)
            .Replace("{{SPEAKERS}}", speakers)
            .Replace("{{SPEAKER_NOTE}}", note)
            .Replace("{{VIDEO}}", video.ToString().TrimEnd())
            .Replace("{{SPORTS}}", string.Join(", ", sportSlugs))
            .Replace("{{CHANNEL_SPORTS}}", channelSports);
    }
}
