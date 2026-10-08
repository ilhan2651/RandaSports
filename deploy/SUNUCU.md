# Sunucuya kurulum

Ubuntu bir VPS'e sıfırdan kurulum. Komutlar sunucuda, SSH ile bağlıyken çalışır.

Sıra önemli: **DNS'i en başta ayarla**, çünkü Caddy sertifikayı ancak alan adı bu
sunucuya bakıyorsa alabiliyor.

---

## 1. DNS

İsimtescil panelinde `randaprojects.tr` için bir **A kaydı**:

| Tür | Ad | Değer |
| --- | --- | --- |
| A | `randasports` | sunucunun IPv4 adresi |

Yayılmasını bekle. Kendi makinenden kontrol:

```bash
nslookup randasports.randaprojects.tr
```

Sunucunun IP'sini döndürene kadar devam etme.

---

## 2. Sunucuyu hazırla

Root olarak bağlandıysan önce kendine bir kullanıcı aç — konteynerleri root
olarak yönetmenin gereği yok:

```bash
adduser ilhan
usermod -aG sudo ilhan
rsync --archive --chown=ilhan:ilhan ~/.ssh /home/ilhan
```

Çık, yeni kullanıcıyla bağlan. Sonra güvenlik duvarı:

```bash
sudo ufw allow OpenSSH
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

> Docker, yayınladığı portlar için ufw kurallarını atlıyor. `docker-compose.yml`
> içinde Postgres, API ve site `127.0.0.1:` önekiyle bağlandığı için dışarıya
> yalnızca Caddy'nin 80/443'ü açık. Bu önekleri kaldırma.

---

## 3. Docker

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
```

Grup değişikliğinin geçerli olması için oturumu kapatıp yeniden bağlan, sonra:

```bash
docker compose version
```

---

## 4. Projeyi çek

```bash
cd ~
git clone https://github.com/ilhan2651/RandaSports.git
cd RandaSports
```

Depo private ise bu komut parola soracak. GitHub parolası çalışmaz; ya
**Settings → Developer settings → Personal access tokens** altından `repo`
yetkili bir token üretip parola yerine onu gir, ya da depoya bir **deploy key**
ekleyip SSH adresiyle klonla.

---

## 5. Ayarlar

```bash
cp .env.example .env
chmod 600 .env
nano .env
```

Doldurulacaklar:

| Alan | Değer |
| --- | --- |
| `POSTGRES_PASSWORD` | yeni ve rastgele — `openssl rand -base64 24` |
| `GEMINI_API_KEY` | Google AI Studio |
| `APISPORTS_API_KEY` | api-football.com |
| `JWT_KEY` | yeni ve rastgele — `openssl rand -base64 48` |
| `SITE_URL` | `https://randasports.randaprojects.tr` |
| `ADMIN_EMAIL` / `ADMIN_PASSWORD` / `ADMIN_FULLNAME` | ilk yönetici hesabı |

Dosyanın sonundaki üç satırın başındaki `#` işaretini **kaldır**:

```
COMPOSE_FILE=docker-compose.yml:docker-compose.prod.yml
SITE_DOMAIN=randasports.randaprojects.tr
ACME_EMAIL=kendi@eposta.adresin
```

`COMPOSE_FILE` satırı, `docker compose` komutlarının Caddy'yi de kapsamasını
sağlıyor; olmazsa site ayağa kalkar ama dışarıdan erişilemez.

---

## 6. Başlat

```bash
docker compose up -d --build
```

İlk derleme birkaç dakika sürüyor. Bittiğinde:

```bash
docker compose ps
docker compose logs -f caddy
```

Caddy loglarında sertifika alındığına dair satırı gördükten sonra tarayıcıdan
`https://randasports.randaprojects.tr` adresini aç.

Takılırsan:

```bash
docker compose logs api | tail -50
docker compose logs worker | tail -50
docker compose logs web | tail -50
```

---

## 7. İlk giriş sonrası

Yönetici hesabıyla giriş yap, sonra `.env` içindeki `ADMIN_EMAIL`,
`ADMIN_PASSWORD` ve `ADMIN_FULLNAME` satırlarını boşalt ve yeniden uygula:

```bash
nano .env
docker compose up -d
```

Hesap zaten veritabanında; tohumlama yalnızca hesap yokken çalışıyor. Parolanın
sunucudaki bir dosyada durmasına gerek yok.

---

## Güncelleme

Her dağıtım:

```bash
cd ~/RandaSports
git pull
docker compose up -d --build
```

Yalnızca değişen imajlar yeniden derleniyor. Eski imajlar diski doldurursa:

```bash
docker image prune -f
```

---

## Yedekleme

Veritabanı `pgdata` adlı Docker biriminde. Günlük yedek için:

```bash
mkdir -p ~/yedek
crontab -e
```

Dosyanın sonuna:

```cron
0 4 * * * cd /home/ilhan/RandaSports && docker compose exec -T postgres pg_dump -U randasports randasports | gzip > /home/ilhan/yedek/randasports-$(date +\%F).sql.gz && find /home/ilhan/yedek -name "randasports-*.sql.gz" -mtime +14 -delete
```

Her gece 04:00'te yedek alır, on dört günden eskileri siler.

Geri yükleme:

```bash
gunzip -c ~/yedek/randasports-2026-10-09.sql.gz | docker compose exec -T postgres psql -U randasports randasports
```

---

## Bilinmesi gerekenler

**Migration'ları API uyguluyor.** Açılışta bekleyenleri kendisi çalıştırıyor,
worker şemayı hazır bulsun diye API'den sonra kalkıyor. Yeni bir migration
eklediğinde sunucuda ayrıca bir şey yapman gerekmiyor — ama migration dosyasını
commit etmeyi unutma, yoksa sunucu eski şemayla çalışır.

**Worker sürekli Gemini çağırıyor.** API ile aynı makinede duruyor; sitenin
yanıt süresi zaman zaman etkilenirse worker'ın aralıklarını `appsettings.json`
içinden seyreltebilir ya da worker'ı ayrı bir sunucuya alabilirsin.

**`next.config.ts` içindeki `remotePatterns` her adrese açık.** Haber görselleri
rastgele kaynaklardan geldiği için böyle; ama bu, siteyi herkesin
kullanabileceği bir görsel vekiline dönüştürüyor. Trafik artarsa kaynak alan
adlarını listelemek gerekir.
