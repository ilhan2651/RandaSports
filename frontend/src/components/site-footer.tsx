import Link from "next/link";
import type { Sport } from "@/lib/sports-nav";

export function SiteFooter({ sports }: { sports: Sport[] }) {
  return (
    <footer className="border-t border-line bg-surface">
      <div className="mx-auto flex w-full max-w-[1280px] flex-col gap-6 px-4 py-10 sm:px-6 md:flex-row md:items-center md:justify-between">
        <div>
          <div className="flex items-baseline gap-1">
            <span className="font-display text-xl font-extrabold uppercase tracking-tight">
              Randa
            </span>
            <span className="font-display text-xl font-extrabold uppercase tracking-tight text-amber">
              Sports
            </span>
          </div>
          <p className="mt-2 max-w-md text-sm text-fog">
            Haberler farklı kaynaklardan derleniyor, tek metinde birleştiriliyor. Her haberin
            altında kaynakların orijinal bağlantıları yer alır.
          </p>
        </div>

        <nav className="flex max-w-xl flex-wrap gap-x-6 gap-y-2 md:justify-end">
          {sports.map((sport) => (
            <Link
              key={sport.slug}
              href={`/${sport.slug}`}
              className="font-display text-sm font-semibold uppercase tracking-wider text-fog transition-colors hover:text-amber"
            >
              {sport.name}
            </Link>
          ))}

          <Link
            href="/yorumcular"
            className="font-display text-sm font-semibold uppercase tracking-wider text-fog transition-colors hover:text-amber"
          >
            Yorumcular
          </Link>

          <Link
            href="/branslar"
            className="font-display text-sm font-semibold uppercase tracking-wider text-fog transition-colors hover:text-amber"
          >
            Tüm branşlar
          </Link>
        </nav>
      </div>

      <div className="border-t border-line py-4 text-center text-xs text-fog">
        © {new Date().getFullYear()} RandaSports
      </div>
    </footer>
  );
}
