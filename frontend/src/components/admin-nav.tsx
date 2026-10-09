"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const BOLUMLER = [
  { href: "/admin", label: "Özet" },
  { href: "/admin/yorumlar", label: "Görüş onayı" },
  { href: "/admin/yorumcular", label: "Yorumcu onayı" },
  { href: "/admin/kanallar", label: "Kanallar" },
  { href: "/admin/takimlar", label: "Takımlar" },
] as const;

/**
 * Yönetim bölümleri arası gezinme. Önceden her sayfa diğerlerine kendi bağlantı
 * listesini taşıyordu: dört ayrı kopya, her birinde farklı bölümler eksik ve
 * hangi sayfada olduğun belli değildi.
 */
export function AdminNav() {
  const pathname = usePathname();

  return (
    <nav className="flex flex-wrap gap-2">
      {BOLUMLER.map((bolum) => {
        // "/admin" her yolun öneki olduğu için tam eşleşme istiyor.
        const aktif =
          bolum.href === "/admin" ? pathname === "/admin" : pathname.startsWith(bolum.href);

        return (
          <Link
            key={bolum.href}
            href={bolum.href}
            aria-current={aktif ? "page" : undefined}
            className={`rounded-lg border px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
              aktif
                ? "border-amber bg-amber text-ink"
                : "border-line text-fog hover:border-amber/40 hover:text-chalk"
            }`}
          >
            {bolum.label}
          </Link>
        );
      })}
    </nav>
  );
}
