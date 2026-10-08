import { OpinionCard } from "@/components/opinion-card";
import type { Opinion } from "@/lib/commentary-shared";

/** Haber sayfasının altındaki bölüm: bu habere bağlanmış onaylı görüşler. */
export function OpinionList({ opinions }: { opinions: Opinion[] }) {
  if (opinions.length === 0) return null;

  return (
    <section className="mt-20">
      <div className="mb-4 flex items-baseline justify-between gap-4 border-b border-line pb-3">
        <h2 className="font-display text-2xl font-extrabold uppercase tracking-tight">
          Yorumcular ne dedi
        </h2>
        <span className="font-display text-xs font-bold uppercase tracking-widest text-amber">
          {opinions.length} görüş
        </span>
      </div>

      <p className="mb-8 max-w-[68ch] text-sm leading-relaxed text-fog">
        Görüşler yayın kayıtlarından çıkarıldı ve yayına alınmadan önce tek tek
        kontrol edildi. Her kartın altındaki bağlantı videonun tam o anına gider.
      </p>

      <div className="grid gap-5 lg:grid-cols-2">
        {opinions.map((opinion, index) => (
          <OpinionCard key={opinion.id} opinion={opinion} index={index} />
        ))}
      </div>
    </section>
  );
}
