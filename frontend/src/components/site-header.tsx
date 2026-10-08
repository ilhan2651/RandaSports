"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { motion } from "framer-motion";
import { useEffect, useState } from "react";
import { AccountLink } from "@/components/account-link";
import { SearchBox } from "@/components/search-box";
import type { Sport } from "@/lib/sports-nav";

/** Menüde yan yana duracak branş sayısı; gerisi "Diğer" altına iniyor. */
const INLINE_COUNT = 6;

export function SiteHeader({ sports, girisli }: { sports: Sport[]; girisli: boolean }) {
  const pathname = usePathname();
  const [scrolled, setScrolled] = useState(false);
  const [moreOpen, setMoreOpen] = useState(false);

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 8);
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  // Sayfa değişince açık kalan menü kapanıyor.
  useEffect(() => setMoreOpen(false), [pathname]);

  const inline = sports.slice(0, INLINE_COUNT);
  const rest = sports.slice(INLINE_COUNT);
  const restActive = rest.some((x) => pathname === `/${x.slug}`);

  return (
    <header
      className={`sticky top-0 z-50 border-b transition-colors duration-300 ${
        scrolled ? "border-line bg-ink/85 backdrop-blur-xl" : "border-transparent bg-ink"
      }`}
    >
      <div className="mx-auto flex w-full max-w-[1280px] items-center gap-6 px-4 py-4 sm:px-6">
        <Link
          href="/"
          className="flex items-baseline gap-1 outline-none focus-visible:ring-2 focus-visible:ring-amber"
        >
          <span className="font-display text-2xl font-extrabold uppercase tracking-tight">
            Randa
          </span>
          <span className="font-display text-2xl font-extrabold uppercase tracking-tight text-amber">
            Sports
          </span>
          <motion.span
            className="ml-1 size-2 rounded-full bg-amber"
            animate={{ scale: [1, 1.35, 1], opacity: [1, 0.6, 1] }}
            transition={{ duration: 2.4, repeat: Infinity, ease: "easeInOut" }}
          />
        </Link>

        <nav className="ml-auto hidden items-center gap-1 md:flex">
          {inline.map((sport) => (
            <NavLink key={sport.slug} href={`/${sport.slug}`} active={pathname === `/${sport.slug}`}>
              {sport.name}
            </NavLink>
          ))}

          {rest.length > 0 && (
            <div className="relative">
              <button
                type="button"
                onClick={() => setMoreOpen((value) => !value)}
                aria-expanded={moreOpen}
                className={`relative px-3 py-2 font-display text-sm font-semibold uppercase tracking-wider outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
                  restActive ? "text-chalk" : "text-fog hover:text-chalk"
                }`}
              >
                Diğer
                <span aria-hidden className="ml-1 text-[10px]">
                  ▾
                </span>
              </button>

              {moreOpen && (
                <div className="absolute right-0 top-full z-50 mt-1 min-w-44 rounded-xl border border-line bg-surface p-2 shadow-xl">
                  {rest.map((sport) => (
                    <Link
                      key={sport.slug}
                      href={`/${sport.slug}`}
                      className={`block rounded-lg px-3 py-2 font-display text-sm font-semibold uppercase tracking-wider transition-colors ${
                        pathname === `/${sport.slug}`
                          ? "bg-amber/10 text-amber"
                          : "text-fog hover:bg-raised hover:text-chalk"
                      }`}
                    >
                      {sport.name}
                    </Link>
                  ))}

                  <span className="my-1 block h-px bg-line" aria-hidden />

                  <Link
                    href="/branslar"
                    className={`block rounded-lg px-3 py-2 font-display text-sm font-semibold uppercase tracking-wider transition-colors ${
                      pathname === "/branslar"
                        ? "bg-amber/10 text-amber"
                        : "text-fog hover:bg-raised hover:text-chalk"
                    }`}
                  >
                    Tüm branşlar
                  </Link>
                </div>
              )}
            </div>
          )}

          <NavLink href="/yorumcular" active={pathname.startsWith("/yorumcu")}>
            Yorumcular
          </NavLink>
        </nav>

        <div className="ml-auto flex items-center gap-2 md:ml-0">
          <SearchBox />
          <AccountLink girisli={girisli} />
        </div>
      </div>

      <nav className="flex gap-5 overflow-x-auto px-4 pb-3 [-ms-overflow-style:none] [scrollbar-width:none] md:hidden [&::-webkit-scrollbar]:hidden">
        {sports.map((sport) => (
          <Link
            key={sport.slug}
            href={`/${sport.slug}`}
            className={`whitespace-nowrap font-display text-sm font-semibold uppercase tracking-wider ${
              pathname === `/${sport.slug}` ? "text-amber" : "text-fog"
            }`}
          >
            {sport.name}
          </Link>
        ))}

        <Link
          href="/yorumcular"
          className={`whitespace-nowrap font-display text-sm font-semibold uppercase tracking-wider ${
            pathname.startsWith("/yorumcu") ? "text-amber" : "text-fog"
          }`}
        >
          Yorumcular
        </Link>

        <Link
          href="/branslar"
          className={`whitespace-nowrap font-display text-sm font-semibold uppercase tracking-wider ${
            pathname === "/branslar" ? "text-amber" : "text-fog"
          }`}
        >
          Tüm branşlar
        </Link>
      </nav>
    </header>
  );
}

function NavLink({
  href,
  active,
  children,
}: {
  href: string;
  active: boolean;
  children: React.ReactNode;
}) {
  return (
    <Link
      href={href}
      className={`relative px-3 py-2 font-display text-sm font-semibold uppercase tracking-wider outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
        active ? "text-chalk" : "text-fog hover:text-chalk"
      }`}
    >
      {children}
      {active && (
        <motion.span
          layoutId="nav-underline"
          className="absolute inset-x-2 -bottom-px h-0.5 rounded-full bg-amber"
          transition={{ type: "spring", stiffness: 420, damping: 34 }}
        />
      )}
    </Link>
  );
}
