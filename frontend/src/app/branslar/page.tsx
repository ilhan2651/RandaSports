import type { Metadata } from "next";
import Link from "next/link";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { getSports } from "@/lib/sports-nav";

export const revalidate = 300;

export const metadata: Metadata = {
  title: "Tüm branşlar",
  description: "RandaSports'ta takip edilen spor branşları ve her birindeki haber sayısı.",
};

export default async function SportsPage() {
  // false: haberi olmayan branşlar da gelsin — bu sayfanın işi zaten onları göstermek.
  const sports = await getSports(false);

  const withStories = sports.filter((sport) => sport.storyCount > 0);
  const empty = sports.filter((sport) => sport.storyCount === 0);

  return (
    <div className="pt-8">
      <div className="mb-8 flex flex-wrap items-end gap-4">
        <h1 className="font-display text-4xl font-extrabold uppercase leading-none tracking-tight sm:text-5xl">
          Branşlar
        </h1>
        <span className="mb-1.5 font-display text-sm uppercase tracking-[0.2em] text-fog">
          {sports.length} branş
        </span>
      </div>

      {withStories.length > 0 && (
        <section>
          <SectionTitle>Haber akışı olanlar</SectionTitle>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {withStories.map((sport, index) => (
              <Reveal key={sport.slug} delay={(index % 3) * 0.06}>
                <SportTile
                  name={sport.name}
                  slug={sport.slug}
                  storyCount={sport.storyCount}
                  hasCompetitions={sport.hasCompetitions}
                />
              </Reveal>
            ))}
          </div>
        </section>
      )}

      {empty.length > 0 && (
        <section className="mt-16">
          <SectionTitle>Henüz haber gelmedi</SectionTitle>
          <p className="-mt-2 mb-5 max-w-2xl text-sm text-fog">
            Bu branşların kaynakları tanımlı. İlk haber derlendiğinde menüde kendiliğinden
            görünmeye başlıyorlar.
          </p>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {empty.map((sport) => (
              <SportTile
                key={sport.slug}
                name={sport.name}
                slug={sport.slug}
                storyCount={0}
                hasCompetitions={sport.hasCompetitions}
              />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}

function SportTile({
  name,
  slug,
  storyCount,
  hasCompetitions,
}: {
  name: string;
  slug: string;
  storyCount: number;
  hasCompetitions: boolean;
}) {
  const quiet = storyCount === 0;

  return (
    <Link
      href={`/${slug}`}
      className={`group flex items-center justify-between gap-3 rounded-xl border bg-surface px-5 py-4 outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
        quiet
          ? "border-dashed border-line hover:border-amber/40"
          : "border-line hover:border-amber"
      }`}
    >
      <div className="min-w-0">
        <span
          className={`block truncate font-display text-lg font-bold uppercase tracking-tight transition-colors ${
            quiet ? "text-fog group-hover:text-chalk" : "text-chalk group-hover:text-amber"
          }`}
        >
          {name}
        </span>
        <span className="mt-0.5 block text-xs text-fog">
          {quiet ? "Henüz haber yok" : `${storyCount} konu`}
          {hasCompetitions && " · fikstür ve puan durumu"}
        </span>
      </div>

      <span
        className={`shrink-0 font-display text-2xl font-extrabold tabular-nums ${
          quiet ? "text-line" : "text-amber"
        }`}
      >
        {storyCount}
      </span>
    </Link>
  );
}
