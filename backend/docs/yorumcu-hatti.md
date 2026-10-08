# Yorumcu görüşleri hattı — ne yaptım, nasıl test edilir

## Hattın akışı

```
Kanal listesi  →  RSS beslemesi  →  Video kuyruğu  →  Gemini video analizi
                                                              ↓
Haber sayfası  ←  onaylı görüş   ←  insan onayı    ←  konuşmacı doğrulama
```

1. **Kanal yönetimi** — kanallar veritabanında tutuluyor. `@kullaniciadi`, tam kanal
   adresi veya `UC...` kimliği yapıştırılabiliyor; `UC...` kimliği ilk taramada
   kanal sayfasından kendiliğinden çözülüyor (YouTube API anahtarı gerekmiyor).
2. **Video keşfi** — `youtube.com/feeds/videos.xml?channel_id=...` beslemesi okunuyor.
   Besleme ücretsiz, anahtarsız ve kotasız; son 15 videoyu veriyor. Yeni videolar
   `Pending` durumunda kuyruğa giriyor.
3. **Görüş çıkarma** — video **indirilmiyor**: bağlantı doğrudan Gemini'ye veriliyor
   (`file_data.file_uri`). Model videoyu izleyip görüşleri JSON olarak döndürüyor.
4. **Konuşmacı doğrulama** — modelin söylediği isme güvenilmiyor. İsim, kayıtlı
   yorumcu sözlüğüyle eşleştiriliyor (ad + takma adlar, Türkçe sadeleştirme ile).
   Eşleşmezse görüş isme bağlanmıyor, "kaynak belirsiz" olarak onaya düşüyor.
   İki kişiye birden uyarsa da bağlanmıyor.
5. **Habere bağlama** — görüşteki takım, son 4 günün haberlerinin kümeleme
   anahtarıyla karşılaştırılıyor; aynı branş + aynı takım ise en güncel habere
   bağlanıyor.

**Her görüş `Pending` doğuyor.** Onaylanmadan sitede görünmüyor. Onay, alıntıyı
"doğrulanmış" sayıyor — yani `IsQuoteVerified` makinenin değil, videoyu açıp
dinleyen insanın kararı.

## Senin yapman gereken tek şey

Migration. PMC'de:

```
Add-Migration AddCommentaryPipeline -Project RandaSports.Persistence -StartupProject RandaSports.Api
Update-Database -Project RandaSports.Persistence -StartupProject RandaSports.Api
```

veya terminalde `backend` içinden:

```
.\mig.ps1 AddCommentaryPipeline
```

Şema değişiklikleri: `channels` tablosuna `handle` (benzersiz) ve `last_error`,
`you_tube_channel_id` artık boş olabiliyor, `opinions` tablosuna `story_id` + indeks.

## Test sırası

1. API ve Worker'ı başlat. Seed çalışınca `commentators` tablosunda 18 yorumcu,
   `channels` tablosunda 8 kanal olacak.
2. `http://localhost:3000/admin/kanallar` — kanal listesi. **Kimlik çözüldü** yazan
   kanallar hazır. "Kimlik bekliyor" + kırmızı hata yazanların kullanıcı adı yanlış
   demektir (aşağıya bak).
3. Bir kanalda **Şimdi tara**'ya bas. Worker'ın turunu beklemeden video çeker;
   Worker log'unda `N yeni video eklendi` görünür.
4. `OpinionExtraction` worker'ı 20 dakikada bir, turda 2 video işliyor. Beklemek
   istemezsen `POST /api/commentary/videos/{videoId}/extract` ile elle tetikle
   (video id'sini `videos` tablosundan al).
5. `http://localhost:3000/admin/yorumlar` — çıkan görüşler burada. Her kartta
   "videoda dinle" bağlantısı videonun tam o anına gider. Dinle, doğruysa **Onayla**.
6. Görüş bir habere bağlandıysa, o haberin sayfasının altında **"Yorumcular ne dedi"**
   bölümü çıkar. Karttaki "Dinle" düğmesi videoyu sayfa içinde o saniyeden başlatır.

## Kanal listesi hakkında — dikkat

Seed'e koyduğum 8 kanalın kullanıcı adlarını **doğrulayamadım**: senin makinen
kapalıydı, buradan YouTube'a çıkamadım. Yanlış olanlar `last_error` ile kendini
belli edecek, yönetim ekranından düzeltebilirsin:

- Yanlışsa: adresi tarayıcıda aç, `youtube.com/@xxx` kısmını kopyala, **Kanal ekle**
  formuna yapıştır (ya da mevcut kanalı sil/yeniden ekle).
- Yorumcu ↔ kanal eşlemesi **bilerek boş**. Hangi yorumcunun hangi kanalda
  konuştuğunu uydurmak istemedim. Eşleme boş olduğunda model aday listesi olarak
  **tüm aktif yorumcuları** alıyor; bu da çalışıyor, sadece biraz daha gevşek.
  İstersen sonra kanal bazında daraltırız, doğruluk artar.

## Kota

Video analizi modelin en pahalı işi. Varsayılanları ücretsiz katmanı yakmayacak
şekilde kıstım:

| Ayar | Değer | Niye |
|---|---|---|
| `VideoDiscovery.RecheckMinutes` | 120 | Aynı kanala 2 saatten önce dönmüyor |
| `VideoDiscovery.MaxAgeDays` | 3 | Eski videoları hiç almıyor |
| `VideoDiscovery.MaxPerChannel` | 4 | İlk taramada 15 video birden girmesin |
| `OpinionExtraction.BatchSize` | 2 | Turda 2 video |
| `OpinionExtraction.IntervalMinutes` | 20 | Saatte en çok 6 video |
| `OpinionExtraction.MaxAttempts` | 2 | Bozuk video sonsuz denenmiyor |
| `Gemini.TimeoutSeconds` | 300 | Video analizi metinden çok uzun sürüyor |

Hepsi `appsettings.Development.json`'da; `Enabled: false` ile iki worker da kapanır.

## Yayına çıkmadan önce kapatılacak iki açık

1. **İki API anahtarı dönmeli.** Gemini anahtarını sohbete yapıştırdın, API-Football
   anahtarını panelinden aldım — ikisi de `appsettings.Development.json` içinde
   (dosya `.gitignore`'da, repoya girmiyor). Sunucuya çıkarken ikisini de iptal
   edip yenisini üret.
2. **Yönetim uç noktalarında kimlik doğrulaması yok.** `/api/commentary/...`
   altındaki onay/ret ve kanal yazma uçları şu an herkese açık. Lokalde sorun değil,
   internete açık bir yere koymadan önce araya kimlik doğrulaması girmeli.

## Dosyalar

**Yeni — Application**
```
Common/Matching/SpeakerMatcher.cs
Common/Text/ChannelReference.cs
Interfaces/Repositories/{IChannelRepository,ICommentatorRepository,IVideoRepository,IOpinionRepository}.cs
Interfaces/Services/{IYouTubeChannelResolver,IYouTubeFeedReader}.cs
Features/Commentary/Command/DiscoverChannelVideos/{Command,Handler}.cs
Features/Commentary/Command/ExtractVideoOpinions/{Command,Handler,AiVideoAnalysis,OpinionPromptBuilder}.cs
Features/Commentary/Command/ReviewOpinion/{Command,Handler}.cs
Features/Commentary/Command/CreateChannel/{Command,Handler,Validator}.cs
Features/Commentary/Command/UpdateChannel/{Command,Handler}.cs
Features/Commentary/Query/{GetChannels,GetOpinions,GetStoryOpinions}/{Query,Handler}.cs
Features/Commentary/Dtos/CommentaryDtos.cs
```

**Yeni — Infrastructure / Persistence / Worker / Api**
```
Infrastructure/YouTube/{YouTubeChannelResolver,YouTubeFeedReader}.cs
Persistence/Repositories/{ChannelRepository,CommentatorRepository,VideoRepository,OpinionRepository}.cs
Persistence/Seeders/CommentatorSeedData.cs
Worker/Workers/{VideoDiscoveryWorker,VideoDiscoveryOptions,OpinionExtractionWorker,OpinionExtractionOptions}.cs
Api/Controllers/CommentaryController.cs
```

**Değişen**
```
Domain/Entities/Channel.cs            → Handle, LastError; YouTubeChannelId artık boş olabiliyor
Domain/Entities/Opinion.cs            → StoryId + Story
Persistence/Configurations/ChannelConfiguration.cs, OpinionConfiguration.cs
Persistence/Seeders/DataSeeder.cs     → yorumcu ve kanal seed'i
Persistence/Repositories/StoryRepository.cs + IStoryRepository → GetRecentClusterRefsAsync
Infrastructure/Ai/{GeminiClient,GeminiOptions}.cs → video desteği, uzun zaman aşımı
Application/Interfaces/Services/IGeminiClient.cs → GenerateJsonFromVideoAsync
Infrastructure/ServiceRegistration, Persistence/ServiceRegistration, Worker/Program.cs
appsettings.Development.json
```

**Frontend**
```
src/lib/commentary.ts                 (yeni)
src/components/opinion-list.tsx       (yeni)
src/app/admin/actions.ts              (yeni)
src/app/admin/yorumlar/page.tsx       (yeni)
src/app/admin/kanallar/page.tsx       (yeni)
src/app/haber/[slug]/page.tsx         (değişti — görüş bölümü eklendi)
src/app/robots.ts                     (değişti — /admin dışarıda)
```

## Modele koyduğum frenler

- "Videoyu izleyemediysen boş dizi döndür; boş dönmek uydurmaktan iyidir."
- "İsimden emin değilsen boş bırak; yanlış isim, isimsiz görüşten çok daha kötü."
- Alıntı 15 karakterden kısaysa atılıyor (modelin boş geçtiği alan oluyor).
- Video süresini aşan zaman damgası uydurma kabul ediliyor, başa alınıyor.
- Küfür/argo içeren görüş hiç alınmıyor.
- Video yeniden işlenirse yalnızca `Pending` görüşler siliniyor; senin onayladığın
  veya reddettiğin görüşlere dokunulmuyor.

## Spor verisi katmanı

Sildim demiyorum, **kapattım**: `SportsSync.Enabled = false`. Puan durumu / oyuncu
istatistiği ekranları ve kodu yerinde duruyor. Bir gün ücretli API'ye geçersen tek
ayarla geri açılıyor.
