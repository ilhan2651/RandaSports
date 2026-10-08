"use client";

import Image from "next/image";
import { useState } from "react";

type Props = {
  name: string;
  src: string | null;
  /** Kenar uzunluğu (px). Izgarada 56, listede 44 kullanılıyor. */
  size?: number;
  className?: string;
};

/**
 * Yorumcu ve takım görselleri dışarıdan geliyor, hepsi de dolu değil. Görsel yoksa
 * ya da yüklenemezse baş harflere düşüyoruz: kırık görsel yerine kasıtlı duran bir
 * rozet çıkıyor.
 *
 * Renk kasıtlı olarak sabit ve sessiz. Önceden addan rastgele ton üretiyordu; bir
 * listede on tane boş görsel olunca on ayrı parlak renk çıkıp gerçek logoları
 * bastırıyordu. Artık boş olanlar geri çekiliyor, logolar öne çıkıyor.
 */
export function EntityAvatar({ name, src, size = 44, className = "" }: Props) {
  const [failed, setFailed] = useState(false);

  const kutu = `relative shrink-0 overflow-hidden rounded-full border border-line ${className}`;
  const stil = { width: size, height: size };

  if (!src || failed)
    return (
      <div
        className={`${kutu} flex items-center justify-center bg-raised`}
        style={stil}
        aria-hidden
      >
        <span
          className="font-display font-bold uppercase leading-none text-fog"
          style={{ fontSize: Math.round(size * 0.34) }}
        >
          {basHarfler(name)}
        </span>
      </div>
    );

  return (
    <div className={`${kutu} bg-surface`} style={stil}>
      <Image
        src={src}
        alt=""
        fill
        sizes={`${size}px`}
        onError={() => setFailed(true)}
        className="object-cover"
      />
    </div>
  );
}

function basHarfler(name: string): string {
  const parcalar = name.trim().split(/\s+/).filter(Boolean);

  if (parcalar.length === 0) return "?";
  if (parcalar.length === 1) return parcalar[0].slice(0, 2);

  // Ortadaki adları atlayıp ilk ve son: "Serdar Ali Çeliker" → "SÇ".
  return parcalar[0][0] + parcalar[parcalar.length - 1][0];
}

