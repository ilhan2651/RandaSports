# Sunucuya kurulum

Sunucuda `~/infra` altında paylaşılan bir altyapı çalışıyor: tek bir Caddy
(80/443'ü o tutuyor) ve tek bir PostgreSQL, ikisi de `web` adlı dış Docker
ağında. RandaSports kendi Caddy'sini ve veritabanı sunucusunu getirmiyor —
bunlara katılıyor, paylaşılan Postgres içinde kendi veritabanını kullanıyor.

```
                     internet
                        │
                   infra-caddy-1          (80/443, web ağı)
                        │
              randasports-web:3000
                        │
              randasports-api:8080 ───┐
              randasports-worker  ────┤
                                      │
                          infra-postgres-1  (web ağı)
                           └─ randasports veritabanı
```

---

## 1. DNS

İsimtescil'de `randaprojects.tr` için A kaydı: ad `randasports`, değer sunucunun
IPv4 adresi. Caddy sertifikayı ancak alan adı sunucuya bakıyorsa alabiliyor, bu
yüzden ilk iş bu.

```bash
getent hosts randasports.randaprojects.tr
```

---

## 2. Veritabanını aç

Paylaşılan Postgres'te RandaSports'a ayrı bir veritabanı ve ayrı bir kullanıcı
açıyoruz. Diğer projelerin verisine erişemez.

Önce bir parola üret ve bir yere not et:

```bash
openssl rand -base64 24
```

Sonra (`BURAYA_PAROLA` yerine onu yaz):

```bash
docker exec -i infra-postgres-1 psql -U postgres <<'SQL'
CREATE USER randasports WITH PASSWORD 'BURAYA_PAROLA';
CREATE DATABASE randasports OWNER randasports;
SQL
```

Kontrol:

```bash
docker exec infra-postgres-1 psql -U postgres -c "\l" | grep randasports
```

---

## 3. Projeyi çek

```bash
mkdir -p ~/apps && cd ~/apps
git clone https://github.com/ilhan2651/RandaSports.git
cd RandaSports
```

Depo private ise parola yerine bir **personal access token** gerekiyor
(GitHub → Settings → Developer settings → Personal access tokens, `repo` yetkisi).

---

## 4. Ayarlar

```bash
cp .env.example .env
chmod 600 .env
nano .env
```

`.env` içinde **YEREL GELİŞTİRME** bloğunun dört satırını yorumla, **SUNUCU**
bloğunu aç ve doldur:

```
#COMPOSE_FILE=docker-compose.yml:docker-compose.local.yml   <- yorumla
#POSTGRES_USER=randasports                                   <- yorumla
#POSTGRES_PASSWORD=randasports_dev                           <- yorumla
#SITE_URL=http://localhost:3000                              <- yorumla

COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml
DB_HOST=infra-postgres-1
POSTGRES_USER=randasports
POSTGRES_PASSWORD=<2. adımda ürettiğin parola>
POSTGRES_DB=randasports
SITE_URL=https://randasports.randaprojects.tr
```

Ortak alanlar:

| Alan | Değer |
| --- | --- |
| `GEMINI_API_KEY` | Google AI Studio |
| `APISPORTS_API_KEY` | api-football.com |
| `JWT_KEY` | `openssl rand -base64 48` — yereldekinden farklı |
| `ADMIN_EMAIL` / `ADMIN_PASSWORD` / `ADMIN_FULLNAME` | ilk yönetici hesabı |

`COMPOSE_FILE` satırı olmazsa `docker compose` yalnızca ana dosyayı okur,
konteynerler `web` ağına bağlanmaz ve ne veritabanına ne Caddy'ye ulaşırlar.

---

## 5. Caddy'ye site bloğunu ekle

```bash
nano ~/infra/caddy/Caddyfile
```

`deploy/caddy-site-blogu.txt` içeriğini dosyanın sonuna ekle, sonra Caddy'yi
yeniden yükle (yeniden başlatmaya gerek yok, bağlantılar kopmaz):

```bash
cd ~/infra
docker compose -f docker-compose.caddy.yml exec caddy caddy reload --config /etc/caddy/Caddyfile
```

Caddy site bloğunu gördüğü anda sertifikayı almaya çalışır; bu yüzden bu adımı
uygulamayı ayağa kaldırdıktan **sonra** yapmak daha rahat. İkisinin sırası
şart değil, sadece önce blok eklenirse Caddy birkaç kez boşa deneyip susar.

---

## 6. Başlat

```bash
cd ~/apps/RandaSports
docker compose up -d --build
```

İlk derleme birkaç dakika sürüyor. Sonra:

```bash
docker compose ps
docker compose logs -f api
```

API migration'ları kendisi uyguluyor; ilk açılışta tabloları kurar ve tohum
verisini yazar. Ardından:

```bash
curl -s localhost:5122/api/sports | head -c 300     # API ayakta mı
curl -s -o /dev/null -w '%{http_code}\n' localhost:3000   # site ayakta mı
```

İkisi de cevap veriyorsa tarayıcıdan `https://randasports.randaprojects.tr`.

Takılırsan:

```bash
docker compose logs api    | tail -50
docker compose logs worker | tail -50
docker compose logs web    | tail -50
docker compose -f ~/infra/docker-compose.caddy.yml logs caddy | tail -30
```

---

## 7. İlk giriş sonrası

Yönetici hesabıyla giriş yap, sonra `.env` içindeki `ADMIN_EMAIL`,
`ADMIN_PASSWORD`, `ADMIN_FULLNAME` satırlarını boşalt:

```bash
nano .env
docker compose up -d
```

Hesap veritabanında duruyor; tohumlama yalnızca hesap yokken çalışıyor.
Parolanın sunucuda bir dosyada durmasına gerek yok.

---

## Güncelleme

```bash
cd ~/apps/RandaSports
git pull
docker compose up -d --build
```

Yalnızca değişen imajlar yeniden derleniyor. Disk dolarsa:

```bash
docker image prune -f
```

---

## Yedekleme

Veri, infra Postgres'inin `pgdata` biriminde. Günlük yedek:

```bash
mkdir -p ~/yedek
crontab -e
```

Sona ekle:

```cron
0 4 * * * docker exec infra-postgres-1 pg_dump -U randasports randasports | gzip > /home/ilhan/yedek/randasports-$(date +\%F).sql.gz && find /home/ilhan/yedek -name "randasports-*.sql.gz" -mtime +14 -delete
```

Geri yükleme:

```bash
gunzip -c ~/yedek/randasports-2026-10-09.sql.gz | docker exec -i infra-postgres-1 psql -U randasports randasports
```

---

## Bilinmesi gerekenler

**`DB_HOST` konteyner adına bağlı.** `infra-postgres-1`, compose'un `infra`
proje adından türetiyor. `~/infra` klasörünün adını değiştirirsen konteyner adı
da değişir ve bağlantı kopar.

**Servis adları ortak ağda görünür.** `api`, `web`, `worker` adları `web` ağında
da takma ad olarak duruyor. Aynı sunucuya ileride aynı adlarla başka bir proje
koyarsan isim çakışır; yeni projelerde servisleri `xyz-api` gibi adlandır ya da
her projeyi kendi iç ağında tut.

**Worker sürekli Gemini çağırıyor.** API ile aynı makinede; site yanıt süresi
etkilenirse `appsettings.json` içindeki aralıkları seyreltebilirsin.

**`next.config.ts` içindeki `remotePatterns` her adrese açık.** Haber görselleri
rastgele kaynaklardan geldiği için böyle, ama bu siteyi herkesin kullanabileceği
bir görsel vekiline dönüştürüyor. Trafik artarsa kaynak alan adlarını listele.
