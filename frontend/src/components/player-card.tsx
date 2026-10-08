"use client";

import Link from "next/link";
import { motion, useMotionValue, useSpring, useTransform } from "framer-motion";
import type { AthleteDetail } from "@/lib/sports";

/**
 * Fareye göre hafifçe eğilen oyuncu kartı: fotoğraf önde, forma numarası
 * ve takım adı arkada daha yavaş kayıyor (parallax).
 */
export function PlayerCard({ athlete }: { athlete: AthleteDetail }) {
  const pointerX = useMotionValue(0);
  const pointerY = useMotionValue(0);

  const rotateX = useSpring(useTransform(pointerY, [-0.5, 0.5], [8, -8]), {
    stiffness: 180,
    damping: 18,
  });
  const rotateY = useSpring(useTransform(pointerX, [-0.5, 0.5], [-10, 10]), {
    stiffness: 180,
    damping: 18,
  });

  const photoX = useTransform(pointerX, [-0.5, 0.5], [-14, 14]);
  const backdropX = useTransform(pointerX, [-0.5, 0.5], [12, -12]);

  function handleMove(event: React.MouseEvent<HTMLDivElement>) {
    const bounds = event.currentTarget.getBoundingClientRect();
    pointerX.set((event.clientX - bounds.left) / bounds.width - 0.5);
    pointerY.set((event.clientY - bounds.top) / bounds.height - 0.5);
  }

  function handleLeave() {
    pointerX.set(0);
    pointerY.set(0);
  }

  return (
    <div className="[perspective:1200px]" onMouseMove={handleMove} onMouseLeave={handleLeave}>
      <motion.div
        style={{ rotateX, rotateY, transformStyle: "preserve-3d" }}
        className="relative overflow-hidden rounded-2xl border border-line bg-gradient-to-b from-raised to-surface"
      >
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_0%,rgba(255,122,26,0.22),transparent_60%)]" />

        {athlete.shirtNumber !== null && (
          <motion.span
            style={{ x: backdropX }}
            className="pointer-events-none absolute -right-4 top-2 select-none font-display text-[150px] font-extrabold leading-none text-chalk/[0.05]"
          >
            {athlete.shirtNumber}
          </motion.span>
        )}

        <div className="relative flex flex-col items-center px-6 pb-6 pt-8">
          <motion.div style={{ x: photoX }} className="relative">
            {athlete.photoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img
                src={athlete.photoUrl}
                alt={athlete.fullName}
                className="size-36 rounded-full border-2 border-amber/40 object-cover"
              />
            ) : (
              <div className="grid size-36 place-items-center rounded-full border-2 border-line bg-ink font-display text-4xl font-extrabold text-line">
                {athlete.fullName.slice(0, 1)}
              </div>
            )}
          </motion.div>

          <h1 className="mt-5 text-center font-display text-3xl font-extrabold uppercase leading-none tracking-tight">
            {athlete.fullName}
          </h1>

          <div className="mt-3 flex flex-wrap items-center justify-center gap-2 text-xs">
            {athlete.position && (
              <span className="rounded-full bg-amber px-3 py-1 font-display font-bold uppercase tracking-widest text-ink">
                {athlete.position}
              </span>
            )}
            {athlete.team && (
              <Link
                href={`/takim/${athlete.team.slug}`}
                className="rounded-full border border-line px-3 py-1 font-display font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber hover:text-amber"
              >
                {athlete.team.name}
              </Link>
            )}
          </div>

          <dl className="mt-6 grid w-full grid-cols-2 gap-x-6 gap-y-3 border-t border-line pt-5 text-sm sm:grid-cols-4">
            <Fact label="Yaş" value={athlete.age ? `${athlete.age}` : null} />
            <Fact label="Boy" value={athlete.heightCm ? `${athlete.heightCm} cm` : null} />
            <Fact label="Kilo" value={athlete.weightKg ? `${athlete.weightKg} kg` : null} />
            <Fact label="Uyruk" value={athlete.nationality} />
          </dl>
        </div>
      </motion.div>
    </div>
  );
}

function Fact({ label, value }: { label: string; value: string | null }) {
  return (
    <div>
      <dt className="font-display text-[11px] uppercase tracking-widest text-fog">{label}</dt>
      <dd className="mt-0.5 font-medium">{value ?? "-"}</dd>
    </div>
  );
}
