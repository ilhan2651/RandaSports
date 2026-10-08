"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { PreferencePicker, type Secim } from "@/components/preference-picker";
import type { OnboardingOptions, Preferences } from "@/lib/me";

type Props = {
  options: OnboardingOptions;
  mevcut: Preferences;
  kaydet: (secim: Secim, tamamla: boolean) => Promise<{ ok: boolean; message: string }>;
};

/**
 * Takip listesini sihirbazdan sonra değiştirme paneli. Sihirbaz bir kez açılıyor,
 * sonrasında tercihleri değiştirmenin yolu yoktu.
 *
 * Kapalı başlıyor: hesap sayfasının asıl işi takip listesini göstermek, üç tam
 * ızgara açılışta sayfayı boğuyordu.
 */
export function PreferenceEditor({ options, mevcut, kaydet }: Props) {
  const router = useRouter();
  const [acik, setAcik] = useState(false);
  const [sonuc, setSonuc] = useState<{ ok: boolean; message: string } | null>(null);
  const [bekliyor, basla] = useTransition();

  // Kayıtlı tercihler başlangıç seçimi; panel her açılışta sunucudaki halle başlıyor.
  const [secim, setSecim] = useState<Secim>(() => ilkSecim(mevcut));

  const kapat = () => {
    setSecim(ilkSecim(mevcut));
    setSonuc(null);
    setAcik(false);
  };

  const gonder = () =>
    basla(async () => {
      // tamamla=false: sihirbaz zaten bitmiş, buradan kaydetmek onu geri açmamalı.
      const cevap = await kaydet(secim, false);
      setSonuc(cevap);

      if (cevap.ok) router.refresh();
    });

  if (!acik)
    return (
      <button
        type="button"
        onClick={() => setAcik(true)}
        className="mt-4 rounded-lg border border-line px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-fog outline-none transition-colors hover:border-amber/40 hover:text-chalk focus-visible:ring-2 focus-visible:ring-amber"
      >
        Takip listemi düzenle
      </button>
    );

  const toplam = secim.sportIds.length + secim.commentatorIds.length + secim.teamIds.length;

  return (
    <div className="mt-6 rounded-2xl border border-line bg-surface p-5">
      <div className="flex flex-wrap items-baseline justify-between gap-3 border-b border-line pb-4">
        <h2 className="font-display text-lg font-extrabold uppercase tracking-tight">
          Takip listesini düzenle
        </h2>
        <p className="font-display text-xs uppercase tracking-widest text-fog">
          {toplam} seçim
        </p>
      </div>

      <div className="mt-5">
        <PreferencePicker options={options} secim={secim} onChange={setSecim} withHeadings />
      </div>

      {sonuc && (
        <p
          className={`mt-5 rounded-lg border px-4 py-2 text-sm ${
            sonuc.ok
              ? "border-amber/40 bg-amber/10 text-amber-soft"
              : "border-live/40 bg-live/10 text-live"
          }`}
          role="status"
        >
          {sonuc.message}
        </p>
      )}

      <div className="mt-5 flex items-center gap-3 border-t border-line pt-4">
        <button
          type="button"
          onClick={kapat}
          disabled={bekliyor}
          className="font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:text-chalk disabled:opacity-50"
        >
          Vazgeç
        </button>

        <button
          type="button"
          onClick={gonder}
          disabled={bekliyor}
          className="ml-auto rounded-lg bg-amber px-5 py-2.5 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft disabled:opacity-50"
        >
          {bekliyor ? "Kaydediliyor…" : "Kaydet"}
        </button>
      </div>
    </div>
  );
}

function ilkSecim(mevcut: Preferences): Secim {
  return {
    sportIds: mevcut.sports.map((x) => x.id),
    commentatorIds: mevcut.commentators.map((x) => x.id),
    teamIds: mevcut.teams.map((x) => x.id),
  };
}
