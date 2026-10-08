import { getStories } from "@/lib/api";
import { getLatestOpinions } from "@/lib/commentary";
import { getSports } from "@/lib/sports-nav";
import { EmptyState } from "@/components/empty-state";
import { OpinionRail } from "@/components/opinion-rail";
import { HeroStory } from "@/components/hero-story";
import { LiveTicker } from "@/components/live-ticker";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { SportChips } from "@/components/sport-chips";
import { StoryCard } from "@/components/story-card";
import { StoryListItem } from "@/components/story-list-item";

export const revalidate = 60;

export default async function HomePage() {
  // Üçü birbirine bağlı değil; paralel gidiyor.
  const [{ items }, opinions, sports] = await Promise.all([
    getStories({ pageSize: 25 }),
    getLatestOpinions({ pageSize: 6 }),
    getSports(true),
  ]);

  if (items.length === 0) {
    return (
      <EmptyState
        title="Henüz haber yok"
        hint="Worker çalışıyorsa haberler birkaç dakika içinde burada görünecek. API'nin ayakta olduğundan emin ol."
      />
    );
  }

  const live = items.filter((story) => story.isLive).slice(0, 8);
  const [hero, ...rest] = items;
  const aside = rest.slice(0, 5);
  const grid = rest.slice(5, 14);
  const more = rest.slice(14);

  return (
    <div className="pt-4">
      <LiveTicker stories={live} />

      <div className="mt-6">
        <SportChips sports={sports} href={(slug) => (slug ? `/${slug}` : "/")} />
      </div>

      <section className="mt-6 grid gap-6 lg:grid-cols-[1.9fr_1fr]">
        <Reveal>
          <HeroStory story={hero} />
        </Reveal>

        <Reveal delay={0.1}>
          <div className="rounded-2xl border border-line bg-surface p-5">
            <h2 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
              Son gelenler
            </h2>
            <div className="mt-2">
              {aside.map((story, index) => (
                <StoryListItem key={story.id} story={story} index={index} />
              ))}
            </div>
          </div>
        </Reveal>
      </section>

      {grid.length > 0 && (
        <section className="mt-16">
          <SectionTitle>Gündem</SectionTitle>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {grid.map((story, index) => (
              <Reveal key={story.id} delay={(index % 3) * 0.08}>
                <StoryCard story={story} />
              </Reveal>
            ))}
          </div>
        </section>
      )}

      <OpinionRail opinions={opinions.items} />

      {more.length > 0 && (
        <section className="mt-16">
          <SectionTitle>Daha fazla</SectionTitle>
          <div className="grid gap-x-10 md:grid-cols-2">
            {more.map((story, index) => (
              <StoryListItem key={story.id} story={story} index={index} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
