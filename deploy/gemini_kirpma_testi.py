#!/usr/bin/env python3
"""
Gemini, YouTube bağlantısını zaman aralığıyla kırparken SESİ de kırpıyor mu?

Neden önemli: Ağustos 2026'da bildirilen bir gerilemeye göre start_offset/end_offset
yalnızca kareleri kırpıyor, ses tüm videodan geliyor. Bizim alıntılarımız konuşma
olduğundan, ses kırpılmıyorsa videoyu dilimlemek doğruluğu artırmaz; yalnızca
maliyeti katlar ve model bütün yayını duymaya devam eder.

Bu betik aynı videoyu iki kez soruyor (tamamı / 5 dakikalık aralık) ve dönen token
sayımını modaliteye göre yazdırıyor.

Kullanım (proje kökünde):
    python3 deploy/gemini_kirpma_testi.py "https://www.youtube.com/watch?v=GERCEK_ID"

En az 30 dakikalık bir video seç — fark ancak orada görünür.
"""

import json
import os
import re
import sys
import urllib.request

ARALIK_BASLANGIC = 600   # saniye
ARALIK_BITIS = 900       # saniye


def anahtari_bul() -> str:
    if os.environ.get("GEMINI_API_KEY"):
        return os.environ["GEMINI_API_KEY"]

    try:
        with open(".env", encoding="utf-8") as dosya:
            for satir in dosya:
                eslesme = re.match(r"^GEMINI_API_KEY=(.*)$", satir.strip())
                if eslesme:
                    return eslesme.group(1).strip()
    except FileNotFoundError:
        pass

    sys.exit("GEMINI_API_KEY bulunamadı (.env ya da ortam değişkeni).")


def cagir(model: str, anahtar: str, govde: dict) -> dict:
    istek = urllib.request.Request(
        f"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent",
        data=json.dumps(govde).encode("utf-8"),
        headers={"Content-Type": "application/json", "x-goog-api-key": anahtar},
        method="POST",
    )

    try:
        with urllib.request.urlopen(istek, timeout=300) as cevap:
            return json.load(cevap)
    except urllib.error.HTTPError as hata:
        return {"hata": hata.read().decode("utf-8", "replace")[:500]}


def yazdir(etiket: str, cevap: dict) -> dict:
    print(f"--- {etiket}")

    if "hata" in cevap:
        print(f"    HATA: {cevap['hata']}")
        return {}

    kullanim = cevap.get("usageMetadata")
    if not kullanim:
        print(f"    usageMetadata yok: {json.dumps(cevap)[:300]}")
        return {}

    print(f"    toplam token : {kullanim.get('totalTokenCount')}")
    print(f"    girdi token  : {kullanim.get('promptTokenCount')}")

    modaliteler = {}
    for ayrinti in kullanim.get("promptTokensDetails", []):
        ad = ayrinti.get("modality", "?")
        sayi = ayrinti.get("tokenCount", 0)
        modaliteler[ad] = sayi
        print(f"      {ad:<8} {sayi}")

    return modaliteler


def main() -> None:
    if len(sys.argv) < 2:
        sys.exit(f"Kullanım: python3 {sys.argv[0]} <youtube-url>")

    video = sys.argv[1]
    if "VIDEO_ID" in video:
        sys.exit("Örnek adresi gerçek bir video bağlantısıyla değiştir.")

    anahtar = anahtari_bul()
    model = os.environ.get("GEMINI_MODEL", "gemini-3.5-flash-lite")
    istem = "Bu videoda ne anlatiliyor? Tek cumleyle yaz."

    print(f"Video: {video}\nModel: {model}\n")

    tam = yazdir("TAMAMI (aralık yok)", cagir(model, anahtar, {
        "contents": [{"parts": [
            {"text": istem},
            {"file_data": {"file_uri": video}},
        ]}]
    }))

    print()

    aralik = yazdir(
        f"ARALIK {ARALIK_BASLANGIC}-{ARALIK_BITIS} sn",
        cagir(model, anahtar, {
            "contents": [{"parts": [
                {"text": istem},
                {
                    "file_data": {"file_uri": video},
                    "video_metadata": {
                        "start_offset": {"seconds": ARALIK_BASLANGIC},
                        "end_offset": {"seconds": ARALIK_BITIS},
                    },
                },
            ]}]
        }),
    )

    print("\n=== SONUÇ")

    ses_tam = tam.get("AUDIO")
    ses_aralik = aralik.get("AUDIO")

    if ses_tam is None or ses_aralik is None:
        print("  Ses token sayısı okunamadı; yukarıdaki ham çıktıya bak.")
        return

    oran = ses_aralik / ses_tam if ses_tam else 0
    print(f"  Ses tokenı: tam {ses_tam} → aralıklı {ses_aralik}  (oran {oran:.2f})")

    if oran > 0.9:
        print("  SES KIRPILMIYOR. Dilimleme işe yaramaz, maliyeti katlar. Yapmıyoruz.")
    elif oran < 0.5:
        print("  Ses kırpılıyor. Dilimleme kurulabilir.")
    else:
        print("  Belirsiz. Farklı bir video ve aralıkla tekrarla.")


if __name__ == "__main__":
    main()
