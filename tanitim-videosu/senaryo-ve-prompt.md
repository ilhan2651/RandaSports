# RandaSports — Tanıtım Videosu

**Format:** 40–45 sn · dikey 4:5 (1080×1350) veya 9:16 · LinkedIn
**Ton:** samimi, enerjik, "sen" dili. Teknolojiden bahsedilmez, sadece ürün anlatılır.
**Marka:** koyu zemin (#0A0A0C), turuncu vurgu (#FF7A1A), kalın ve dar spor başlık fontu (Barlow Condensed / Bebas tarzı)
**Site:** randasports.randaprojects.tr

## Ana fikir
Maçtan çok maç hakkında söylenenler konuşuluyor, ama bu sözler saatlerce süren yayınların içinde kayboluyor ya da çarpıtılıyor.
RandaSports haberleri tek yerde topluyor, yorumcuların gerçekte ne dediğini de birebir, kaynağıyla ve videonun saniyesiyle gösteriyor.

## Sahneler

| # | Süre | Görsel(ler) | Seslendirme | Ekran yazısı |
|---|---|---|---|---|
| 1 | 0–4 sn | (görsel yok: siyah ekran + hızlı metin animasyonu) | "Bir yorumcu bir şey söylüyor, ertesi gün on sitede on farklı şekilde okuyorsun." | Kim ne dedi, gerçekten? |
| 2 | 4–9 sn | 01-anasayfa | "İşte bu yüzden RandaSports'u yaptım." | RandaSports — Spor haberleri, tek yerde |
| 3 | 9–14 sn | 02-gundem-kartlari, 03-haber-akisi | "Futboldan F1'e, MMA'dan basketbola, 11 branşın gündemi tek yerde." | 11 branş · tek akış |
| 4 | 14–21 sn | 04-haber-detay → 05b-kaynaklar-yakin | "Aynı haberi farklı sitelerden tek tek okumana gerek yok. Hepsini tek metinde topluyor, kaynakları da hemen yanına koyuyor." | 3 kaynak → 1 haber |
| 5 | 21–27 sn | 06b-yorumcular-baslik → 06-yorumcular-ne-dedi | "Ama asıl olay burada: Yorumcular ne dedi?" | Yorumcular ne dedi? |
| 6 | 27–35 sn | 08b-yorumcu-alinti-karti, 07-yorumcu-alintilari | "Yorumcunun söylediği cümle birebir, kimin söylediği doğrulanmış halde. Tıklayınca video tam o saniyeden başlıyor." | Birebir alıntı · Doğrulanmış konuşmacı · Tam o saniye |
| 7 | 35–40 sn | 09-formula1, 10-mma (hızlı geçiş), 11-giris | "Hesabını aç, takımını ve yorumcunu seç, akışın sana göre şekillensin." | Sana özel akış |
| 8 | 40–45 sn | (logo + link kartı) | "Bir göz at, ne düşündüğünü çok merak ediyorum!" | randasports.randaprojects.tr |

## AI video üreticiye verilecek prompt

```
Create a 45-second vertical (4:5) promo video for LinkedIn for "RandaSports", a Turkish sports news website.
Tone: friendly, energetic, personal — a developer proudly showing his own product. Do NOT mention any technology.
Style: dark background (#0A0A0C), orange accent (#FF7A1A), bold condensed sports typography, fast cuts,
smooth zoom/pan (Ken Burns) on the uploaded screenshots, upbeat royalty-free sports music.
Use ONLY the uploaded screenshots as visuals, in the order and timing below. Turkish on-screen captions,
Turkish voiceover (warm, natural male voice).

Scene 1 (0-4s): black screen, kinetic text "Kim ne dedi, gerçekten?"
  VO: "Bir yorumcu bir şey söylüyor, ertesi gün on sitede on farklı şekilde okuyorsun."
Scene 2 (4-9s): 01-anasayfa. Text: "RandaSports — Spor haberleri, tek yerde"
  VO: "İşte bu yüzden RandaSports'u yaptım."
Scene 3 (9-14s): 02-gundem-kartlari, 03-haber-akisi. Text: "11 branş · tek akış"
  VO: "Futboldan F1'e, MMA'dan basketbola, 11 branşın gündemi tek yerde."
Scene 4 (14-21s): 04-haber-detay, then zoom into 05b-kaynaklar-yakin. Text: "3 kaynak → 1 haber"
  VO: "Aynı haberi farklı sitelerden tek tek okumana gerek yok. Hepsini tek metinde topluyor, kaynakları da hemen yanına koyuyor."
Scene 5 (21-27s): 06b-yorumcular-baslik, 06-yorumcular-ne-dedi. Text: "Yorumcular ne dedi?"
  VO: "Ama asıl olay burada: Yorumcular ne dedi?"
Scene 6 (27-35s): 08b-yorumcu-alinti-karti (slow zoom on the quote), 07-yorumcu-alintilari.
  Text: "Birebir alıntı · Doğrulanmış konuşmacı · Tam o saniye"
  VO: "Yorumcunun söylediği cümle birebir, kimin söylediği doğrulanmış halde. Tıklayınca video tam o saniyeden başlıyor."
Scene 7 (35-40s): 09-formula1, 10-mma quick cuts, then 11-giris. Text: "Sana özel akış"
  VO: "Hesabını aç, takımını ve yorumcunu seç, akışın sana göre şekillensin."
Scene 8 (40-45s): end card, "RANDA SPORTS" logo (white + orange) and "randasports.randaprojects.tr"
  VO: "Bir göz at, ne düşündüğünü çok merak ediyorum!"
```

## LinkedIn yazısı (teknoloji burada anlatılıyor)

```
Spor gündeminde en çok konuşulan şey çoğu zaman maçın kendisi değil, maç hakkında söylenenler. 🎙️
Ama o sözler saatlerce süren yayınların içinde kayboluyor ya da alıntılanırken bağlamından kopuyor.

Bu yüzden RandaSports'u geliştirdim:
⚽ Aynı olayı anlatan haberleri tek metinde birleştiriyor, kaynaklar altında
🤖 Yorumcuların YouTube videolarını Gemini ile analiz edip birebir alıntıyı, zaman damgasını ve tutumu çıkarıyor
✅ Sözün gerçekten o kişiye ait olup olmadığını puanlıyor
🎯 Takımına, branşına ve yorumcuna göre kişisel akış sunuyor

Teknik tarafta:
• .NET — Clean Architecture, CQRS (MediatR), EF Core, ayrı Worker servisi
• Next.js 15 (App Router)
• PostgreSQL
• Gemini API ile video analizi
• Docker Compose ile kendi VPS'imde yayında

Denemek isteyenler için link ilk yorumda 👇 Geri bildirimlerinizi çok merak ediyorum!

#dotnet #nextjs #yapayzeka #yazilim #spor
```
