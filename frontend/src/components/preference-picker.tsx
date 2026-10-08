"use client";

import { useMemo } from "react";
import { EntityAvatar } from "@/components/entity-avatar";
import type { OnboardingOptions } from "@/lib/me";

export type Secim = {
  sportIds: string[];
  commentatorIds: string[];
  teamIds: string[];
};

export type Bolum = "sports" | "commentators" | "teams";

/** Başlık metinleri sihirbazla düzenleme panelinde aynı; tek yerde duruyor. */
export const BOLUM_BILGI: Record<Bolum, { baslik: string; aciklama: string }> = {
  sports: {
    baslik: "Hangi branşları takip ediyorsun?",
    aciklama: "Seçtiklerin akışının omurgası olacak. Sonradan değiştirebilirsin.",
  },
  commentators: {
    baslik: "Hangi yorumcuları takip ediyorsun?",
    aciklama: "Seçtiğin branşlarda görüş veren yorumcular.",
  },
  teams: {
    baslik: "Hangi takımları takip ediyorsun?",
    aciklama: "Takımı olmayan branşlarda bu bölümü boş bırakabilirsin.",
  },
};

type Props = {
  options: OnboardingOptions;
  secim: Secim;
  onChange: (secim: Secim) => void;
  /** Verilirse yalnızca o bölüm çiziliyor — sihirbaz adım adım gösteriyor. */
  only?: Bolum;
  /** Bölüm başlıklarını çiz — tek panelde üç bölüm varken gerekiyor. */
  withHeadings?: boolean;
};

/**
 * Takip listesi seçim ızgaraları. Sihirbaz ve hesap sayfasındaki düzenleme paneli
 * aynı bileşeni kullanıyor; iki kopya tutulduğunda biri değişip diğeri geride kalıyordu.
 */
export function PreferencePicker({ options, secim, onChange, only, withHeadings }: Props) {
  const seciliSlugs = useMemo(
    () => options.sports.filter((x) => secim.sportIds.includes(x.id)).map((x) => x.slug),
    [options.sports, secim.sportIds],
  );

  // Branş seçilmediyse hepsi gösteriliyor: boş liste seçimi çıkmaza sokar.
  //
  // Zaten takip edilen bir kayıt branş süzgecine uymasa bile listede kalıyor. Yoksa
  // kullanıcı branşı listeden çıkardığında takip ettiği takım ekrandan kaybolup
  // kaydederken sessizce siliniyordu.
  const uygunKisiler = useMemo(() => {
    if (seciliSlugs.length === 0) return options.commentators;

    return options.commentators.filter(
      (x) =>
        secim.commentatorIds.includes(x.id) ||
        x.sportSlugs.length === 0 ||
        x.sportSlugs.some((s) => seciliSlugs.includes(s)),
    );
  }, [options.commentators, seciliSlugs, secim.commentatorIds]);

  const uygunTakimlar = useMemo(() => {
    if (seciliSlugs.length === 0) return options.teams;

    return options.teams.filter(
      (x) => secim.teamIds.includes(x.id) || !x.sportSlug || seciliSlugs.includes(x.sportSlug),
    );
  }, [options.teams, seciliSlugs, secim.teamIds]);

  const bolumler: { ad: Bolum; icerik: React.ReactNode }[] = [
    {
      ad: "sports",
      icerik: (
        <Izgara>
          {options.sports.map((sport) => (
            <Secenek
              key={sport.id}
              aktif={secim.sportIds.includes(sport.id)}
              onClick={() =>
                onChange({ ...secim, sportIds: degistir(secim.sportIds, sport.id) })
              }
              baslik={sport.name}
              alt={`${sport.opinionCount} görüş`}
            />
          ))}
        </Izgara>
      ),
    },
    {
      ad: "commentators",
      icerik:
        uygunKisiler.length === 0 ? (
          <Bos>Seçtiğin branşlarda henüz yorumcu yok.</Bos>
        ) : (
          <Izgara>
            {uygunKisiler.map((kisi) => (
              <Secenek
                key={kisi.id}
                aktif={secim.commentatorIds.includes(kisi.id)}
                onClick={() =>
                  onChange({
                    ...secim,
                    commentatorIds: degistir(secim.commentatorIds, kisi.id),
                  })
                }
                baslik={kisi.fullName}
                alt={`${kisi.opinionCount} görüş`}
                gorsel={<EntityAvatar name={kisi.fullName} src={kisi.photoUrl} size={36} />}
              />
            ))}
          </Izgara>
        ),
    },
    {
      ad: "teams",
      icerik:
        uygunTakimlar.length === 0 ? (
          <Bos>Seçtiğin branşlarda takım yok — bu bölümü geç.</Bos>
        ) : (
          <Izgara>
            {uygunTakimlar.map((takim) => (
              <Secenek
                key={takim.id}
                aktif={secim.teamIds.includes(takim.id)}
                onClick={() => onChange({ ...secim, teamIds: degistir(secim.teamIds, takim.id) })}
                baslik={takim.name}
                gorsel={
                  <EntityAvatar
                    name={takim.name}
                    src={takim.logoUrl}
                    size={36}
                    className="!rounded-md"
                  />
                }
              />
            ))}
          </Izgara>
        ),
    },
  ];

  const gosterilecek = only ? bolumler.filter((x) => x.ad === only) : bolumler;

  return (
    <div className="space-y-8">
      {gosterilecek.map((bolum) => (
        <section key={bolum.ad}>
          {withHeadings && (
            <div className="mb-3">
              <h3 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
                {BOLUM_BILGI[bolum.ad].baslik}
              </h3>
              <p className="mt-1 text-xs text-fog">{BOLUM_BILGI[bolum.ad].aciklama}</p>
            </div>
          )}
          {bolum.icerik}
        </section>
      ))}
    </div>
  );
}

export function degistir(liste: string[], id: string) {
  return liste.includes(id) ? liste.filter((x) => x !== id) : [...liste, id];
}

function Izgara({ children }: { children: React.ReactNode }) {
  return <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">{children}</div>;
}

function Bos({ children }: { children: React.ReactNode }) {
  return (
    <p className="rounded-xl border border-dashed border-line px-6 py-12 text-center text-sm text-fog">
      {children}
    </p>
  );
}

function Secenek({
  aktif,
  onClick,
  baslik,
  alt,
  gorsel,
}: {
  aktif: boolean;
  onClick: () => void;
  baslik: string;
  alt?: string;
  gorsel?: React.ReactNode;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={aktif}
      className={`flex items-center gap-3 rounded-xl border px-3 py-2.5 text-left outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
        aktif ? "border-amber bg-amber/10" : "border-line hover:border-amber/40 hover:bg-raised"
      }`}
    >
      {gorsel}

      <span className="min-w-0 flex-1">
        <span className="block truncate font-display text-sm font-bold uppercase tracking-tight text-chalk">
          {baslik}
        </span>
        {alt && <span className="block text-xs text-fog">{alt}</span>}
      </span>

      <span
        className={`flex size-5 shrink-0 items-center justify-center rounded-md border text-[11px] ${
          aktif ? "border-amber bg-amber text-ink" : "border-line text-transparent"
        }`}
        aria-hidden
      >
        ✓
      </span>
    </button>
  );
}
