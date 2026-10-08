# RandaSports

Türkçe, yalnızca spora odaklanan bir haber platformu. Haberleri farklı kaynaklardan toplayıp aynı olayı anlatanları tek bir konuda birleştiriyor — ve asıl farkı burada: **yorumcuların YouTube videolarından ne söyledikleri çıkarılıyor, kime ait olduğu doğrulanıyor ve ilgili haberin altına bağlanıyor.**

Yani "Ahmet Çakar şöyle dedi" diye bir başlık okumak yerine, cümlenin kendisini, videonun hangi saniyesinde söylendiğini ve o cümlenin hangi habere ait olduğunu bir arada görüyorsun.

> Kişisel bir proje. Aktif geliştiriliyor, henüz yayında değil.

---

## Neden

Spor gündeminde en çok tartışılan şey maçın kendisi değil, maç hakkında söylenenler. Ama o söylenenler saatlerce süren yayınların içinde kaybolmuş durumda ve haber siteleri bunları alıntılarken sürekli çarpıtıyor: cümle kısaltılıyor, bağlamından koparılıyor, bazen hiç o kişinin olmayan bir söz ona mal ediliyor.

RandaSports bu işi tersten yapıyor. Videoyu modele veriyor, model konuşan kişiyi ve cümleyi çıkarıyor, sistem de **bu atfın ne kadar güvenilir olduğunu puanlıyor**. Ekranda alt bantta adı yazan bir konuşmacı ile yalnızca video başlığından tahmin edilen bir konuşmacı aynı muameleyi görmüyor.

---

## Neler yapıyor

**Haber tarafı**
- RSS kaynaklarından sürekli haber topluyor
- Aynı olayı anlatan haberleri kümeliyor (branş + kategori + geçen takımlar + gün)
- Küme yeterince beslendiğinde tek bir metin olarak yeniden yazılıyor, kaynakların orijinal bağlantıları altında duruyor
- Fikstür, puan durumu, kadro ve oyuncu istatistikleri API-Football'dan senkronlanıyor

**Yorum tarafı**
- Takip edilen YouTube kanalları taranıyor, yeni videolar kuyruğa alınıyor
- Video doğrudan modele veriliyor; konu, özet, birebir alıntı, zaman damgası ve tutum (olumlu / olumsuz / nötr) çıkarılıyor
- Konuşmacı sözlükle eşleştiriliyor, atıf güveni puanlanıyor
- Yorumcu mu, teknik direktör mü, futbolcu mu — rolü ayrıca modele sorduruluyor; yalnızca gerçekten yorumcu olanlar sözlüğe giriyor
- Görüş, konusu olan habere bağlanıyor

**Kişiselleştirme**
- Kullanıcı branş, yorumcu ve takım seçiyor
- Akış iki süzgeçten geçiyor: *kim konuşuyor* ve *ne hakkında*
- Seçim yapılmamış boyut süzmüyor — hiç yorumcu seçmeyen branşındaki herkesi görüyor

---

## Mimari

Clean Architecture, katmanlar arası bağımlılık tek yönlü (`Domain` hiçbir şey bilmiyor, `Api` ve `Worker` en dışta).

```
backend/
  RandaSports.Domain          Entity'ler ve enum'lar. Hiçbir pakete bağlı değil.
  RandaSports.Application     CQRS komut/sorguları, arayüzler, Result sarmalayıcısı,
                              FluentValidation davranışı. Dış dünyayı yalnızca
                              arayüzlerden tanıyor.
  RandaSports.Persistence     EF Core, IEntityTypeConfiguration'lar, repository'ler,
                              UnitOfWork, migration'lar, tohumlama.
  RandaSports.Infrastructure  Gemini istemcisi, YouTube besleme okuyucu, API-Football
                              istemcisi, Wikidata istemcisi, JWT üretimi.
  RandaSports.Api             İnce controller'lar; iş yok, yalnızca MediatR'a iletim.
  RandaSports.Worker          Arka plan hizmetleri. Her biri kendi ayar bloğundan
                              açılıp kapanabiliyor.

frontend/                     Next.js 15 App Router, sunucu bileşeni ağırlıklı.
                              Oturum jetonu httpOnly çerezde; tarayıcı görmüyor.
```

**Arka plan hizmetleri:** RSS toplayıcı · konu kümeleme · AI zenginleştirme · haber yazarı · video keşfi · görüş çıkarımı · yorumcu bakımı (rol tespiti + portre) · takım logosu · spor verisi senkronu.

Hepsi bağımsız ve ayar dosyasından tek tek kapatılabiliyor; biri patladığında diğerleri çalışmaya devam ediyor.

---

## Teknoloji

| Katman | Kullanılan |
| --- | --- |
| Backend | .NET 10, C# 13 |
| Veri | PostgreSQL 17, EF Core 10, Npgsql, snake_case adlandırma |
| Desen | MediatR (CQRS), FluentValidation, repository + UnitOfWork |
| Kimlik | JWT (`JsonWebTokenHandler`), PBKDF2-SHA256 210k tur, dönen yenileme jetonu |
| Yapay zekâ | Google Gemini — video anlama ve metin üretimi |
| Dış veri | API-Football, YouTube RSS, Wikidata / Wikimedia Commons |
| Frontend | Next.js 15 (App Router), React 19, TypeScript, Tailwind, Framer Motion |

---

## Çalıştırma

Gerekenler: .NET 10 SDK, Node 20+, Docker (ya da yerelde çalışan bir PostgreSQL 17).

**1. Veritabanı**

```bash
cp .env.example .env     # Windows cmd: copy .env.example .env
docker compose up -d postgres
```

`.env` compose'un okuduğu tek sır kaynağı. İçindeki `COMPOSE_FILE` satırı hangi
compose dosyalarının okunacağını belirliyor: yerelde projenin kendi Postgres'i
kalkıyor, sunucuda paylaşılan altyapıya bağlanılıyor.

**2. Ayar dosyaları**

Örnekleri kopyala ve kendi değerlerini yaz. Bu dosyalar `.gitignore`'da; içinde anahtar var, depoya girmiyorlar.

```bash
cp backend/RandaSports.Api/appsettings.Development.example.json \
   backend/RandaSports.Api/appsettings.Development.json

cp backend/RandaSports.Worker/appsettings.Development.example.json \
   backend/RandaSports.Worker/appsettings.Development.json

cp frontend/.env.example frontend/.env.local
```

Doldurman gerekenler:

| Alan | Nereden |
| --- | --- |
| `ConnectionStrings:Default` | `docker-compose.yml`'deki kullanıcı/parola |
| `Gemini:ApiKey` | [Google AI Studio](https://aistudio.google.com/apikey) |
| `ApiSports:ApiKey` | [api-football.com](https://www.api-football.com/) — ücretsiz katman günde 100 istek |
| `Jwt:Key` | Kendin üret, en az 32 karakter: `openssl rand -base64 48` |
| `AdminSeed` | İlk yönetici hesabı. **Giriş yaptıktan sonra bu bloğu sil.** |

**3. Backend**

```bash
cd backend
dotnet run --project RandaSports.Api      # migration'ları da uygular
dotnet run --project RandaSports.Worker   # ayrı terminalde
```

API ayağa kalkarken bekleyen migration'ları kendisi uyguluyor. Entity'lerde migration'a yansımamış bir değişiklik varsa açılışta uyarıyor.

**4. Frontend**

```bash
cd frontend
npm install
npm run dev
```

`http://localhost:3000`

### Migration üretmek

```bash
cd backend
dotnet ef migrations add Adi -p RandaSports.Persistence -s RandaSports.Api
```

ya da kısayol: `.\mig.ps1 Adi` (üretir ve uygular).

> Migration üretmeden önce projenin derlendiğinden emin ol. `dotnet ef`, derlenmiş derlemeyi okur; kaynaktaki yeni bir entity değişikliği henüz derlenmemişse **boş bir migration** üretir ve bunu sana söylemez.

---

## Gizli bilgiler

Depoda hiçbir anahtar, parola ya da jeton yok. Hepsi `appsettings.Development.json` dosyalarında ve bu dosyalar `.gitignore`'da.

Yayına almadan önce:

- `docker-compose.yml`'deki geliştirme parolası değiştirilmeli — olduğu gibi sunucuya taşınmamalı
- `Jwt:Key` üretim için ayrı ve rastgele olmalı
- `AdminSeed` bloğu ilk girişten sonra silinmeli
- Kullanıcı hesapları kişisel veri: KVKK aydınlatma metni, gizlilik politikası ve açık rıza yayına çıkmadan hazır olmalı

---

## Dağıtım

Üç compose dosyası var: `docker-compose.yml` uygulamanın kendisi (API, worker,
site), `docker-compose.local.yml` yerel geliştirme için Postgres ekliyor,
`docker-compose.prod.yml` sunucudaki paylaşılan Caddy ve Postgres'e bağlıyor.
Hangisinin okunacağını `.env` içindeki `COMPOSE_FILE` satırı söylüyor, bu yüzden
çalıştırma komutu iki ortamda da aynı.

```bash
# sunucuda, ilk kurulumdan sonra her dağıtım:
git pull
docker compose up -d --build
```

Sıfırdan kurulum adım adım: [`deploy/SUNUCU.md`](deploy/SUNUCU.md)

Veritabanı, API ve site portları yalnızca `127.0.0.1`'e bağlı; dışarıya açık
olan tek şey Caddy'nin 80 ve 443'ü. Docker yayınladığı portlar için güvenlik
duvarı kurallarını atladığından bu önek önemli, kaldırılmamalı.

## Kararlar ve sınırlar

Bazı şeyler kasıtlı olarak yapılmadı:

**Skor ve istatistik siteleri kazınmıyor.** Mackolik, Flashscore, Sofascore ve TFF'nin kullanım koşulları otomatik veri çekmeyi yasaklıyor. Maç verisi lisanslı bir API'den geliyor.

**Yüz tanıma yok.** Yorumcu fotoğraflarını eşlemek için yüz tanıma kullanmak cazipti ama KVKK'da biyometrik veriyi *kimlik tespiti amacıyla* işlemek özel nitelikli kişisel veri; ilgili kişinin kendi açık rızasını gerektiriyor. Fotoğraflar açık lisanslı kaynaklardan (Wikimedia Commons) ya da elle giriliyor.

**Alıntı çevrilmiyor.** İleride yabancı kaynaklı yorumlar eklenecek; alıntı kaynak dilinde kalacak, yalnızca konu ve özet Türkçeleştirilecek. Çevrilmiş bir alıntıyı birine atfetmek uydurmadır.

**Uzun videolar daha güvenilir.** Sezgiye ters ama ölçtük: uzun yayınlarda konuşmacının adı alt bantta yazıyor, bu yazılı kanıt. Kısa kliplerde atıf çoğu zaman video başlığından çıkarılıyor ve gerçek yanlış atıfların neredeyse tamamı oradan geliyor.

---

## Yol haritası

- [ ] Günlük kişisel bülten (akış hazır, teslimat yöntemi henüz seçilmedi)
- [ ] Diğer branşlar için İngilizce yorum kaynakları
- [ ] Takım logolarının tamamlanması — şu an basketbolda büyük kısmı eksik
- [ ] KVKK metinleri

---

## Lisans

Henüz bir lisans belirlenmedi; tüm hakları saklıdır.
