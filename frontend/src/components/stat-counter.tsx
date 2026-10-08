"use client";

import { animate, useInView } from "framer-motion";
import { useEffect, useRef, useState } from "react";

type StatCounterProps = {
  label: string;
  /** API boş alanları JSON'dan çıkarabiliyor: null da undefined da gelebilir. */
  value?: number | null;
  decimals?: number;
};

/** Ekrana girince sıfırdan gerçek değere sayan istatistik kutusu. */
export function StatCounter({ label, value, decimals = 0 }: StatCounterProps) {
  const ref = useRef<HTMLDivElement>(null);
  const inView = useInView(ref, { once: true, margin: "-40px" });
  const [shown, setShown] = useState(0);

  const missing = value === null || value === undefined || Number.isNaN(value);

  useEffect(() => {
    if (!inView || missing) return;

    const controls = animate(0, value as number, {
      duration: 1.1,
      ease: [0.22, 1, 0.36, 1],
      onUpdate: (latest) => setShown(Number.isFinite(latest) ? latest : 0),
    });

    return () => controls.stop();
  }, [inView, missing, value]);

  return (
    <div ref={ref} className="rounded-xl border border-line bg-surface px-4 py-4 text-center">
      <div className="font-display text-3xl font-extrabold tabular-nums text-amber">
        {missing ? "-" : shown.toFixed(decimals)}
      </div>
      <div className="mt-1 font-display text-[11px] uppercase tracking-widest text-fog">
        {label}
      </div>
    </div>
  );
}
