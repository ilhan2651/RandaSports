"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { motion } from "framer-motion";

const NEWS_TAB = { segment: "", label: "Haberler" } as const;

// Bu iki sekme lige bağlı: MMA, boks, e-spor gibi branşlarda hiç gösterilmiyor.
const COMPETITION_TABS = [
  { segment: "/puan-durumu", label: "Puan Durumu" },
  { segment: "/fikstur", label: "Fikstür" },
] as const;

export function SportNav({
  sport,
  hasCompetitions,
}: {
  sport: string;
  hasCompetitions: boolean;
}) {
  const pathname = usePathname();

  // Ligi olmayan branşta tek sekme kalıyor; şerit anlamsız olduğu için hiç çizilmiyor.
  if (!hasCompetitions) return null;

  const tabs = [NEWS_TAB, ...COMPETITION_TABS];

  return (
    <nav className="mb-8 flex gap-1 border-b border-line">
      {tabs.map((tab) => {
        const href = `/${sport}${tab.segment}`;
        const active = pathname === href;

        return (
          <Link
            key={tab.label}
            href={href}
            className={`relative px-4 py-3 font-display text-sm font-bold uppercase tracking-wider outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
              active ? "text-chalk" : "text-fog hover:text-chalk"
            }`}
          >
            {tab.label}
            {active && (
              <motion.span
                layoutId="sport-tab"
                className="absolute inset-x-0 -bottom-px h-0.5 bg-amber"
                transition={{ type: "spring", stiffness: 420, damping: 34 }}
              />
            )}
          </Link>
        );
      })}
    </nav>
  );
}
