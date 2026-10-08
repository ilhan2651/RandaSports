"use client";

import { motion } from "framer-motion";
import type { RadarAxis } from "@/lib/player-stats";

const SIZE = 300;
const CENTER = SIZE / 2;
const RADIUS = 104;
const RINGS = [0.25, 0.5, 0.75, 1];

function pointAt(index: number, total: number, distance: number) {
  const angle = (Math.PI * 2 * index) / total - Math.PI / 2;
  return {
    x: CENTER + Math.cos(angle) * distance,
    y: CENTER + Math.sin(angle) * distance,
    angle,
  };
}

function polygon(values: number[]): string {
  return values
    .map((value, index) => {
      const { x, y } = pointAt(index, values.length, RADIUS * value);
      return `${x.toFixed(1)},${y.toFixed(1)}`;
    })
    .join(" ");
}

/** Tek serili radar: lejant yok, başlık ve eksen etiketleri seriyi zaten adlandırıyor. */
export function PlayerRadar({ axes }: { axes: RadarAxis[] }) {
  return (
    <figure className="rounded-xl border border-line bg-surface p-5">
      <figcaption className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
        Sezon profili
      </figcaption>

      <svg
        viewBox={`0 0 ${SIZE} ${SIZE}`}
        className="mx-auto mt-2 w-full max-w-[320px] overflow-visible"
        role="img"
        aria-label={axes.map((axis) => `${axis.label}: ${axis.display}`).join(", ")}
      >
        {RINGS.map((ring) => (
          <polygon
            key={ring}
            points={polygon(axes.map(() => ring))}
            fill="none"
            stroke="var(--color-line)"
            strokeWidth="1"
          />
        ))}

        {axes.map((axis, index) => {
          const { x, y } = pointAt(index, axes.length, RADIUS);
          return (
            <line
              key={axis.label}
              x1={CENTER}
              y1={CENTER}
              x2={x}
              y2={y}
              stroke="var(--color-line)"
              strokeWidth="1"
            />
          );
        })}

        <motion.g
          initial={{ scale: 0, opacity: 0 }}
          whileInView={{ scale: 1, opacity: 1 }}
          viewport={{ once: true, margin: "-40px" }}
          transition={{ duration: 0.7, ease: [0.22, 1, 0.36, 1] }}
          style={{ transformOrigin: `${CENTER}px ${CENTER}px` }}
        >
          <polygon
            points={polygon(axes.map((axis) => axis.value))}
            fill="var(--color-amber)"
            fillOpacity="0.18"
            stroke="var(--color-amber)"
            strokeWidth="2"
            strokeLinejoin="round"
          />

          {axes.map((axis, index) => {
            const { x, y } = pointAt(index, axes.length, RADIUS * axis.value);
            return (
              <circle
                key={axis.label}
                cx={x}
                cy={y}
                r="4"
                fill="var(--color-amber)"
                stroke="var(--color-surface)"
                strokeWidth="2"
              />
            );
          })}
        </motion.g>

        {axes.map((axis, index) => {
          const { x, y, angle } = pointAt(index, axes.length, RADIUS + 26);
          const anchor =
            Math.abs(Math.cos(angle)) < 0.3 ? "middle" : Math.cos(angle) > 0 ? "start" : "end";

          return (
            <g key={axis.label}>
              <text
                x={x}
                y={y - 4}
                textAnchor={anchor}
                className="fill-[var(--color-fog)] text-[10px] uppercase tracking-wider"
              >
                {axis.label}
              </text>
              <text
                x={x}
                y={y + 9}
                textAnchor={anchor}
                className="fill-[var(--color-chalk)] text-[12px] font-bold tabular-nums"
              >
                {axis.display}
              </text>
            </g>
          );
        })}
      </svg>
    </figure>
  );
}
