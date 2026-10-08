import Link from "next/link";
import { EntityAvatar } from "@/components/entity-avatar";
import type { Followed } from "@/lib/me";

/**
 * Takip edilenleri rozet olarak çizer. Hem hesap sayfası hem akış kullanıyor;
 * iki yerde ayrı ayrı durunca biri değiştiğinde diğeri geride kalıyordu.
 */
export function FollowChips({
  title,
  items,
  round = false,
  hrefPrefix,
}: {
  title: string;
  items: Followed[];
  /** Yorumcu fotoğrafları yuvarlak, takım logoları kare duruyor. */
  round?: boolean;
  /**
   * Verilirse rozetler tıklanabilir oluyor: "/takim" → /takim/galatasaray.
   * Boş metin kökü gösteriyor: "" → /futbol.
   */
  hrefPrefix?: string;
}) {
  if (items.length === 0) return null;

  return (
    <div>
      <p className="font-display text-[11px] font-bold uppercase tracking-widest text-fog">
        {title}
      </p>

      <div className="mt-2 flex flex-wrap gap-2">
        {items.map((item) => {
          const icerik = (
            <>
              <EntityAvatar
                name={item.name}
                src={item.imageUrl}
                size={24}
                className={round ? "" : "!rounded-md"}
              />
              <span className="font-display text-xs font-bold uppercase tracking-tight text-chalk">
                {item.name}
              </span>
            </>
          );

          const stil =
            "flex items-center gap-2 rounded-lg border border-line bg-surface px-3 py-1.5";

          return hrefPrefix !== undefined ? (
            <Link
              key={item.id}
              href={`${hrefPrefix}/${item.slug}`}
              className={`${stil} outline-none transition-colors hover:border-amber/40 focus-visible:border-amber`}
            >
              {icerik}
            </Link>
          ) : (
            <span key={item.id} className={stil}>
              {icerik}
            </span>
          );
        })}
      </div>
    </div>
  );
}
