import Link from "next/link";
import { OpinionCard } from "@/components/opinion-card";
import type { Opinion } from "@/lib/commentary-shared";

/** Anasayfa bölümü: en son onaylanmış görüşler. */
export function OpinionRail({ opinions }: { opinions: Opinion[] }) {
  if (opinions.length === 0) return null;

  return (
    <section className="mt-16">
      <div className="mb-5 flex items-baseline justify-between gap-4 border-b border-line pb-3">
        <h2 className="font-display text-2xl font-extrabold uppercase tracking-tight">
          Yorumcular ne dedi
        </h2>
        <Link
          href="/yorumcular"
          className="font-display text-xs font-bold uppercase tracking-widest text-amber underline-offset-4 outline-none transition-colors hover:underline focus-visible:underline"
        >
          Tümü
        </Link>
      </div>

      <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
        {opinions.map((opinion, index) => (
          <OpinionCard key={opinion.id} opinion={opinion} index={index} showStory />
        ))}
      </div>
    </section>
  );
}
