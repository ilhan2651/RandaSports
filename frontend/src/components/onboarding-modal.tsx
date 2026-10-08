"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import {
  BOLUM_BILGI,
  PreferencePicker,
  type Bolum,
  type Secim,
} from "@/components/preference-picker";
import type { OnboardingOptions } from "@/lib/me";

const ADIMLAR: Bolum[] = ["sports", "commentators", "teams"];

type Props = {
  options: OnboardingOptions;
  kaydet: (secim: Secim, tamamla: boolean) => Promise<{ ok: boolean; message: string }>;
};

/**
 * Giriş sonrası takip listesi sihirbazı. Üç adım: branş, yorumcu, takım.
 * İkinci ve üçüncü adım birinciye göre süzülüyor — MMA seçen birine Galatasaray
 * sormanın anlamı yok.
 *
 * Atlamak serbest ve atlamak da bir cevap: kullanıcı hiçbir şey seçmese bile
 * sihirbaz tamamlanmış sayılıyor, her girişte tekrar açılmıyor.
 */
export function OnboardingModal({ options, kaydet }: Props) {
  const router = useRouter();
  const [adim, setAdim] = useState(0);
  const [secim, setSecim] = useState<Secim>({
    sportIds: [],
    commentatorIds: [],
    teamIds: [],
  });
  const [hata, setHata] = useState<string | null>(null);
  const [bekliyor, basla] = useTransition();

  const bitir = () =>
    basla(async () => {
      const sonuc = await kaydet(secim, true);

      if (!sonuc.ok) {
        setHata(sonuc.message);
        return;
      }

      // Seçim yapıldıysa ödülü hemen gösteriyoruz: kişiselleşmiş akış. "Şimdilik geç"
      // diyene akışı açmak anlamsız, orada hesap sayfasında kalıyor.
      if (
        secim.sportIds.length > 0 ||
        secim.commentatorIds.length > 0 ||
        secim.teamIds.length > 0
      ) {
        router.push("/akis");
        return;
      }

      router.refresh();
    });

  const son = adim === ADIMLAR.length - 1;
  const bolum = ADIMLAR[adim];

  return (
    <div className="fixed inset-0 z-[70] flex items-start justify-center bg-ink/85 p-4 pt-[8vh] backdrop-blur-sm">
      <div className="flex max-h-[80vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl border border-line bg-surface shadow-2xl">
        <header className="border-b border-line px-6 py-5">
          <div className="flex items-center gap-1.5">
            {ADIMLAR.map((_, i) => (
              <span
                key={i}
                className={`h-1 flex-1 rounded-full transition-colors ${
                  i <= adim ? "bg-amber" : "bg-line"
                }`}
              />
            ))}
          </div>

          <h2 className="mt-4 font-display text-2xl font-extrabold uppercase tracking-tight">
            {BOLUM_BILGI[bolum].baslik}
          </h2>
          <p className="mt-1 text-sm text-fog">{BOLUM_BILGI[bolum].aciklama}</p>
        </header>

        <div className="overflow-y-auto px-6 py-5">
          <PreferencePicker options={options} secim={secim} onChange={setSecim} only={bolum} />
        </div>

        {hata && (
          <p className="mx-6 mb-3 rounded-lg border border-live/40 bg-live/10 px-4 py-2 text-sm text-live">
            {hata}
          </p>
        )}

        <footer className="flex items-center gap-3 border-t border-line px-6 py-4">
          {adim > 0 && (
            <button
              type="button"
              onClick={() => setAdim(adim - 1)}
              className="font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:text-chalk"
            >
              Geri
            </button>
          )}

          <button
            type="button"
            onClick={bitir}
            disabled={bekliyor}
            className="ml-auto font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:text-chalk disabled:opacity-50"
          >
            Şimdilik geç
          </button>

          <button
            type="button"
            onClick={() => (son ? bitir() : setAdim(adim + 1))}
            disabled={bekliyor}
            className="rounded-lg bg-amber px-5 py-2.5 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft disabled:opacity-50"
          >
            {bekliyor ? "Kaydediliyor…" : son ? "Bitir" : "Devam"}
          </button>
        </footer>
      </div>
    </div>
  );
}
