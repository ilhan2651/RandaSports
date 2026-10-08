"use client";

import { useState } from "react";

type Sport = { slug: string; name: string };

/**
 * Kanalın kapsadığı branşlar. Çoklu seçim şart: HTalks hem NFL hem futbol,
 * 8GEN hem MMA hem boks yapıyor. Bu seçim görüşün branşını belirlemiyor —
 * modele verilen bir ipucu ve tek branş varsa son çare yedek.
 *
 * Gizli input'larla gönderiyoruz: aynı adla birden çok değer, sunucu tarafında
 * formData.getAll ile okunuyor; çoklu <select> mobilde kullanılmaz hâlde.
 */
export function SportPicker({
  sports,
  selected,
  name = "sportSlugs",
}: {
  sports: Sport[];
  selected: string[];
  name?: string;
}) {
  const [secili, setSecili] = useState<string[]>(selected);

  const degistir = (slug: string) =>
    setSecili((onceki) =>
      onceki.includes(slug) ? onceki.filter((x) => x !== slug) : [...onceki, slug],
    );

  return (
    <div className="flex flex-wrap items-center gap-1.5">
      {secili.map((slug) => (
        <input key={slug} type="hidden" name={name} value={slug} />
      ))}

      {sports.map((sport) => {
        const aktif = secili.includes(sport.slug);

        return (
          <button
            key={sport.slug}
            type="button"
            onClick={() => degistir(sport.slug)}
            aria-pressed={aktif}
            className={`rounded-md border px-2 py-1 font-display text-[10px] font-bold uppercase tracking-widest outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
              aktif
                ? "border-amber bg-amber text-ink"
                : "border-line text-fog hover:border-amber/40 hover:text-chalk"
            }`}
          >
            {sport.name}
          </button>
        );
      })}

      {secili.length === 0 && (
        <span className="ml-1 text-[10px] uppercase tracking-widest text-fog">
          genel
        </span>
      )}
    </div>
  );
}
