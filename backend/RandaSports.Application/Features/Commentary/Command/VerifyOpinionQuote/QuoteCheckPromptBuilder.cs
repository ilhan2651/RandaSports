namespace RandaSports.Application.Features.Commentary.Command.VerifyOpinionQuote;

internal static class QuoteCheckPromptBuilder
{
    private const string Template = """
        Sana bir videonun KISA BİR BÖLÜMÜNÜ veriyorum. Elimde bu bölümden alındığı
        söylenen bir alıntı var. Tek işin, bu alıntının bu bölümde gerçekten böyle
        söylenip söylenmediğini kontrol etmek. Cevabı JSON olarak ver.

        İDDİA EDİLEN ALINTI:
        "{{QUOTE}}"

        İDDİA EDİLEN KONUŞMACI: {{SPEAKER}}

        NASIL KARAR VERECEKSİN — "sonuc" alanı:
        - "birebir": Bu cümle bu bölümde aynen böyle geçiyor. Noktalama, bağlaç ve
          "ya", "işte", "yani" gibi doldurma kelimelerindeki küçük farklar önemli
          değil, kelimeler tutuyorsa birebirdir.
        - "yakin": Aynı şey söylenmiş ama KELİMELER FARKLI. Anlamın tutması yetmez;
          biz alıntı yayınlıyoruz, "şöyle demek istedi" değil "şöyle dedi" diyoruz.
          Kelimeler değiştiyse bu "yakin"dir, "birebir" değildir.
        - "farkli": Bu bölümde söylenen şey bu değil.
        - "baskasi": Cümle doğru ama söyleyen, iddia edilen kişi değil.
        - "duyulmadi": Bu bölümde böyle bir söz geçmiyor, ses anlaşılmıyor ya da
          videoyu izleyemedin.

        EN ÖNEMLİ KURAL: Emin değilsen "duyulmadi" yaz. Burada senin işin
        DOĞRULAMAK, kurtarmak değil. Yanlış bir "birebir" cevabı, bu kontrolü
        tamamen değersiz kılar — çünkü biz buna güvenip alıntıyı yayına alıyoruz.
        Alıntıyı tutturmaya çalışma; ne duyduysan onu yaz.

        DİĞER ALANLAR:
        - "duyulan": Bu bölümde gerçekten duyduğun cümle, kelimesi kelimesine.
          "birebir" dahil her durumda doldur. Hiçbir şey duymadıysan boş bırak.
        - "konusan": Cümleyi söyleyen kişinin adı. SADECE videonun içinden belirle:
          alt bant, ekrandaki isim, birinin ona ismiyle hitap etmesi. Yukarıda
          yazan iddia edilen ismi doğru varsayma, ona bakarak cevap verme. Adı
          göremiyorsan boş bırak — tahmin etme.
        - "saniye": Cümlenin bu BÖLÜMÜN BAŞINDAN itibaren kaçıncı saniyede
          başladığı. Bölümün başı 0. Videonun tamamına göre değil, sana verilen
          bölüme göre say.

        Sadece şu JSON'u döndür:
        {
          "sonuc": "birebir",
          "duyulan": "...",
          "konusan": "...",
          "saniye": 12
        }
        """;

    public static string Build(string quote, string? speaker) =>
        Template
            .Replace("{{QUOTE}}", quote)
            .Replace("{{SPEAKER}}", string.IsNullOrWhiteSpace(speaker) ? "belirtilmemiş" : speaker);
}
