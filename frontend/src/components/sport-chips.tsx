import Link from "next/link";
import type { Sport } from "@/lib/sports-nav";

type Props = {
  sports: Sport[];
  /** Seçili branşın slug'ı; hiçbiri seçili değilse "Tümü" vurgulanıyor. */
  active?: string;
  /**
   * Çipin gideceği adres. Anasayfada branş sayfasına, aramada arama terimini
   * koruyan bir adrese gitmesi gerekiyor; o yüzden üretimi çağıran tarafta.
   */
  href: (slug: string | null) => string;
};

/**
 * Branş seçimi bağlantı olarak çiziliyor: durum adres çubuğunda duruyor,
 * sayfa sunucuda render ediliyor, filtreli hâli paylaşılabiliyor.
 *
 * Listede yalnızca haberi olan branşlar var — boş kategori tıklanınca
 * kullanıcıyı boş sayfaya düşürüyor. Hepsini görmek isteyen sondaki
 * bağlantıdan /branslar sayfasına gidiyor.
 */
export function SportChips({ sports, active, href }: Props) {
  if (sports.length === 0) return null;

  return (
    <nav
      aria-label="Branşlar"
      className="-mx-4 flex items-center gap-2 overflow-x-auto px-4 pb-1 sm:mx-0 sm:px-0"
    >
      <Chip href={href(null)} active={!active}>
        Tümü
      </Chip>

      {sports.map((sport) => (
        <Chip key={sport.slug} href={href(sport.slug)} active={active === sport.slug}>
          {sport.name}
          <span className="ml-1.5 opacity-60">{sport.storyCount}</span>
        </Chip>
      ))}

      <span className="h-5 w-px shrink-0 bg-line" aria-hidden />

      <Link
        href="/branslar"
        className="shrink-0 whitespace-nowrap px-1 font-display text-xs font-bold uppercase tracking-widest text-fog outline-none transition-colors hover:text-amber focus-visible:ring-2 focus-visible:ring-amber"
      >
        Tüm branşlar →
      </Link>
    </nav>
  );
}

function Chip({
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
      className={`shrink-0 whitespace-nowrap rounded-lg border px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
        active
          ? "border-amber bg-amber text-ink"
          : "border-line text-fog hover:border-amber/40 hover:text-chalk"
      }`}
    >
      {children}
    </Link>
  );
}
