import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { EmptyState } from "@/components/empty-state";
import { Reveal } from "@/components/reveal";
import { SportNav } from "@/components/sport-nav";
import { StandingsTable } from "@/components/standings-table";
import { getStandings } from "@/lib/sports";
import { findSport } from "@/lib/sports-nav";

export const revalidate = 300;

type Props = { params: Promise<{ sport: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { sport } = await params;
  const match = await findSport(sport);

  return { title: match ? `${match.name} puan durumu` : "Puan durumu" };
}

export default async function StandingsPage({ params }: Props) {
  const { sport } = await params;
  const match = await findSport(sport);

  // Ligi olmayan branşta puan durumu hiç olmayacak: boş sayfa yerine 404.
  if (!match || !match.hasCompetitions) notFound();

  const standings = await getStandings(match.slug);

  return (
    <div className="pt-8">
      <SportNav sport={match.slug} hasCompetitions={match.hasCompetitions} />

      {!standings || standings.rows.length === 0 ? (
        <EmptyState
          title="Puan durumu yok"
          hint="Bu branş için henüz lig verisi çekilmedi."
        />
      ) : (
        <>
          <div className="mb-6 flex flex-wrap items-end gap-4">
            <h1 className="font-display text-4xl font-extrabold uppercase leading-none tracking-tight sm:text-5xl">
              {standings.competitionName}
            </h1>
            {standings.seasonName && (
              <span className="mb-1 font-display text-sm uppercase tracking-[0.2em] text-fog">
                {standings.seasonName} sezonu
              </span>
            )}
          </div>

          <Reveal>
            <StandingsTable rows={standings.rows} />
          </Reveal>

          <p className="mt-4 text-xs text-fog">
            O: oynanan · G: galibiyet · B: beraberlik · M: mağlubiyet · A: attığı · Y: yediği ·
            Av: averaj · P: puan
          </p>
        </>
      )}
    </div>
  );
}
