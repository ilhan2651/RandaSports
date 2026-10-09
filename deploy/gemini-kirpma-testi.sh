#!/usr/bin/env bash
# Gemini'nin YouTube bağlantısını zaman aralığıyla kırparken SESİ de kırpıp
# kırpmadığını ölçer.
#
# Neden gerekli: Ağustos 2026'da bildirilen bir gerilemeye göre start_offset/end_offset
# yalnızca kareleri kırpıyor, ses tüm videodan geliyor. Bizim alıntılarımız konuşma
# olduğu için ses kırpılmıyorsa videoyu dilimlemek doğruluğu artırmaz, yalnızca
# maliyeti katlar. Bu betik bunu bir çağrıyla gösteriyor.
#
# Kullanım (sunucuda, proje kökünde):
#   bash deploy/gemini-kirpma-testi.sh https://www.youtube.com/watch?v=VIDEO_ID
#
# Uzun bir video seç (en az 30 dk) — fark ancak orada görünür.

set -euo pipefail

VIDEO="${1:-}"
if [ -z "$VIDEO" ]; then
  echo "Kullanım: bash $0 <youtube-url>" >&2
  exit 1
fi

# Anahtar .env'den okunuyor; betiğe yazmıyoruz.
if [ -f .env ]; then
  KEY=$(grep -E '^GEMINI_API_KEY=' .env | cut -d= -f2-)
else
  KEY="${GEMINI_API_KEY:-}"
fi

if [ -z "${KEY:-}" ]; then
  echo "GEMINI_API_KEY bulunamadı (.env ya da ortam değişkeni)." >&2
  exit 1
fi

MODEL="${GEMINI_MODEL:-gemini-3.5-flash-lite}"
URL="https://generativelanguage.googleapis.com/v1beta/models/$MODEL:generateContent"

# Tek iş: token sayımını görmek. Cevabın içeriği önemsiz, kısa tutuyoruz.
PROMPT='Bu videoda ne anlatiliyor? Tek cumleyle yaz.'

cagir() {
  local etiket="$1" govde="$2"
  local cevap
  cevap=$(curl -sS -X POST "$URL" \
    -H "x-goog-api-key: $KEY" \
    -H "Content-Type: application/json" \
    -d "$govde")

  echo "--- $etiket"
  echo "$cevap" | python3 -I -c '
import json,sys
d=json.load(sys.stdin)
u=d.get("usageMetadata")
if not u:
    print("  HATA:", json.dumps(d)[:300]); sys.exit()
print(f"  toplam token : {u.get(\"totalTokenCount\")}")
print(f"  girdi token  : {u.get(\"promptTokenCount\")}")
for ayrinti in u.get("promptTokensDetails", []):
    print(f"    {ayrinti.get(\"modality\"):<6} {ayrinti.get(\"tokenCount\")}")
'
}

echo "Video: $VIDEO"
echo "Model: $MODEL"
echo

cagir "TAMAMI (aralik yok)" "$(python3 -I -c "
import json,sys
print(json.dumps({'contents':[{'parts':[
  {'text':'''$PROMPT'''},
  {'file_data':{'file_uri':'$VIDEO'}}
]}]}))")"

cagir "ARALIK 600-900 sn (5 dk)" "$(python3 -I -c "
import json,sys
print(json.dumps({'contents':[{'parts':[
  {'text':'''$PROMPT'''},
  {'file_data':{'file_uri':'$VIDEO'},
   'video_metadata':{'start_offset':{'seconds':600},'end_offset':{'seconds':900}}}
]}]}))")"

cat <<'NOT'

NASIL OKUNUR
  Aralıklı çağrının AUDIO token sayısı tamamınkiyle aynıysa ses kırpılmıyor
  demektir — dilimleme işe yaramaz, maliyeti katlar. O zaman dilimlemeyi yapmıyoruz.

  Aralıklı çağrıda hem VIDEO hem AUDIO belirgin düşüyorsa kırpma düzelmiş,
  dilimlemeyi kurabiliriz.
NOT
