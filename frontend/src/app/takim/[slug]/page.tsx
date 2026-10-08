import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { FixtureList } from "@/components/fixture-list";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { getStories } from "@/lib/api";
import { getTeam } from "@/lib/sports";
import { StoryCard } from "@/components/story-card";

export const revalidate = 300;

type Props = { params: Promise<{ slug: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  const team = await getTeam(slug);

  return { title: team?.name ?? "Takım" };
}

export default async function TeamPage({ params }: Props) {
  const { slug } = await params;
  const team = await getTeam(slug);

  if (!team) notFound();

  // Takımın haberleri: konu başlıklarında takım adı geçenler.
  const stories = (await getStories({ search: team.name, pageSize: 3 })).items;

  return (
    <div className="pt-8">
      <header className="flex flex-wrap items-center gap-5 border-b border-line pb-8">
        {team.logoUrl && (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={team.logoUrl} alt="" className="size-20 object-contain" />
        )}

        <div>
          <h1 className="font-display text-4xl font-extrabold uppercase leading-none tracking-tight sm:text-5xl">
            {team.name}
          </h1>
          {team.country && (
            <p className="mt-2 font-display text-sm uppercase tracking-[0.2em] text-fog">
              {team.country}
            </p>
          )}
        </div>

        {team.standing && (
          <div className="ml-auto flex gap-6">
            <Metric label="Sıra" value={`${team.standing.rank}.`} />
            <Metric label="Puan" value={`${team.standing.points}`} accent />
            <Metric label="Oynadığı" value={`${team.standing.played}`} />
            <Metric
              label="Averaj"
              value={
                team.standing.goalDifference > 0
                  ? `+${team.standing.goalDifference}`
                  : `${team.standing.goalDifference}`
              }
            />
          </div>
        )}
      </header>

      <div className="mt-12 grid gap-12 lg:grid-cols-[minmax(0,1fr)_380px]">
        <section>
          <SectionTitle>Kadro</SectionTitle>

          {team.squad.length === 0 ? (
            <p className="rounded-xl border border-dashed border-line bg-surface px-6 py-10 text-center text-sm text-fog">
              Kadro henüz çekilmedi.
            </p>
          ) : (
            <div className="grid gap-3 sm:grid-cols-2">
              {team.squad.map((player) => (
                <Link
                  key={player.slug}
                  href={`/oyuncu/${player.slug}`}
                  className="group flex items-center gap-3 rounded-xl border border-line bg-surface p-3 outline-none transition-colors hover:border-amber/50 focus-visible:ring-2 focus-visible:ring-amber"
                >
                  {player.photoUrl ? (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img
                      src={player.photoUrl}
                      alt=""
                      className="size-11 rounded-full object-cover"
                    />
                  ) : (
                    <div className="grid size-11 place-items-center rounded-full bg-raised font-display font-bold text-line">
                      {player.fullName.slice(0, 1)}
                    </div>
                  )}

                  <div className="min-w-0 flex-1">
                    <div className="truncate font-medium transition-colors group-hover:text-amber">
                      {player.fullName}
                    </div>
                    <div className="text-xs text-fog">{player.position ?? "-"}</div>
                  </div>

                  {player.shirtNumber !== null && (
                    <span className="font-display text-xl font-extrabold tabular-nums text-line transition-colors group-hover:text-amber">
                      {player.shirtNumber}
                    </span>
                  )}
                </Link>
              ))}
            </div>
          )}
        </section>

        <aside className="space-y-10">
          <section>
            <h2 className="mb-3 font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
              Son maçlar
            </h2>
            <FixtureList fixtures={team.recentFixtures} />
          </section>

          <section>
            <h2 className="mb-3 font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
              Gelecek maçlar
            </h2>
            <FixtureList fixtures={team.upcomingFixtures} />
          </section>
        </aside>
      </div>

      {stories.length > 0 && (
        <section className="mt-16">
          <SectionTitle>{team.name} haberleri</SectionTitle>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {stories.map((story, index) => (
              <Reveal key={story.id} delay={index * 0.08}>
                <StoryCard story={story} />
              </Reveal>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}

function Metric({ label, value, accent }: { label: string; value: string; accent?: boolean }) {
  return (
    <div className="text-center">
      <div
        className={`font-display text-3xl font-extrabold tabular-nums ${accent ? "text-amber" : ""}`}
      >
        {value}
      </div>
      <div className="mt-1 font-display text-[11px] uppercase tracking-widest text-fog">
        {label}
      </div>
    </div>
  );
}
