import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getStories } from "@/lib/api";
import { EmptyState } from "@/components/empty-state";
import { HeroStory } from "@/components/hero-story";
import { Pagination } from "@/components/pagination";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { SportChips } from "@/components/sport-chips";
import { SportNav } from "@/components/sport-nav";
import { StoryCard } from "@/components/story-card";
import { findSport, getSports } from "@/lib/sports-nav";

// Önbellek yok, bilerek: bu sayfa searchParams okuyor (sayfalama ve süzgeçler) ve
// sorgu dizesine bakan bir sayfa önceden üretilemiyor. Burada `revalidate` yazmak
// Next'e "bunu önbelleğe al" demek oluyordu ve üretim derlemesinde sayfa
// DYNAMIC_SERVER_USAGE ile 500 veriyordu.

const PAGE_SIZE = 24;

type Props = {
  params: Promise<{ sport: string }>;
  searchParams: Promise<{ page?: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { sport } = await params;
  const match = await findSport(sport);

  return { title: match ? `${match.name} haberleri` : "Haberler" };
}

export default async function SportPage({ params, searchParams }: Props) {
  const { sport } = await params;
  const { page: rawPage } = await searchParams;

  // Çip şeridi menüyle aynı listeyi gösteriyor; ikisi birbirine bağlı değil, paralel gidiyor.
  const [match, sports] = await Promise.all([findSport(sport), getSports(true)]);

  if (!match) notFound();

  const page = Math.max(1, Number(rawPage) || 1);
  const { items, total, pageSize } = await getStories({
    sport: match.slug,
    page,
    pageSize: PAGE_SIZE,
  });

  if (items.length === 0) {
    return (
      <div className="pt-8">
        <div className="mb-6">
          <SportChips
            sports={sports}
            active={match.slug}
            href={(slug) => (slug ? `/${slug}` : "/")}
          />
        </div>
        <SportNav sport={match.slug} hasCompetitions={match.hasCompetitions} />
        <EmptyState
          title={`${match.name} için haber yok`}
          hint="Bu branşta henüz derlenmiş bir konu bulunmuyor."
        />
      </div>
    );
  }

  const showHero = page === 1;
  const hero = showHero ? items[0] : null;
  const rest = showHero ? items.slice(1) : items;

  return (
    <div className="pt-8">
      <div className="mb-6">
        <SportChips
          sports={sports}
          active={match.slug}
          href={(slug) => (slug ? `/${slug}` : "/")}
        />
      </div>

      <SportNav sport={match.slug} hasCompetitions={match.hasCompetitions} />

      <div className="mb-6 flex items-end gap-4">
        <h1 className="font-display text-5xl font-extrabold uppercase leading-none tracking-tight">
          {match.name}
        </h1>
        <span className="mb-1.5 font-display text-sm uppercase tracking-[0.2em] text-fog">
          {total} konu
        </span>
      </div>

      {hero && (
        <Reveal>
          <HeroStory story={hero} />
        </Reveal>
      )}

      {rest.length > 0 && (
        <section className={hero ? "mt-16" : ""}>
          <SectionTitle>Tüm haberler</SectionTitle>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {rest.map((story, index) => (
              <Reveal key={story.id} delay={(index % 3) * 0.08}>
                <StoryCard story={story} />
              </Reveal>
            ))}
          </div>
        </section>
      )}

      <Pagination page={page} pageSize={pageSize} total={total} basePath={`/${match.slug}`} />
    </div>
  );
}
