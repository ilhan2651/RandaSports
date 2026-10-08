import { OpinionCard } from "@/components/opinion-card";
import { groupByDay, type Opinion } from "@/lib/commentary-shared";

/**
 * Görüşleri yayın gününe göre öbekleyip çizer. Düz bir ızgarada her kart aynı
 * ağırlıkta duruyordu; gün başlıkları akışa zaman ekseni veriyor.
 */
export function OpinionDayGroups({
  opinions,
  linkCommentator = true,
}: {
  opinions: Opinion[];
  linkCommentator?: boolean;
}) {
  const groups = groupByDay(opinions);

  return (
    <div className="space-y-10">
      {groups.map((group) => (
        <section key={group.label}>
          <div className="mb-4 flex items-center gap-3">
            <h2 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
              {group.label}
            </h2>
            <span className="h-px flex-1 bg-line" />
            <span className="font-display text-xs uppercase tracking-widest text-fog">
              {group.items.length} görüş
            </span>
          </div>

          <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
            {group.items.map((opinion, index) => (
              <OpinionCard
                key={opinion.id}
                opinion={opinion}
                index={index}
                showStory
                linkCommentator={linkCommentator}
              />
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}
