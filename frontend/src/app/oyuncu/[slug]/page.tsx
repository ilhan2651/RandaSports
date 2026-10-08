import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { CompetitionBreakdown } from "@/components/competition-breakdown";
import { PlayerCard } from "@/components/player-card";
import { PlayerRadar } from "@/components/player-radar";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { StatCounter } from "@/components/stat-counter";
import { buildRadar, hasRadarData } from "@/lib/player-stats";
import { getAthlete } from "@/lib/sports";

export const revalidate = 300;

type Props = { params: Promise<{ slug: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  const athlete = await getAthlete(slug);

  if (!athlete) return { title: "Oyuncu bulunamadı" };

  return {
    title: athlete.fullName,
    description: `${athlete.fullName} — ${athlete.team?.name ?? ""} ${athlete.position ?? ""} sezon istatistikleri`.trim(),
  };
}

export default async function AthletePage({ params }: Props) {
  const { slug } = await params;
  const athlete = await getAthlete(slug);

  if (!athlete) notFound();

  const competitions = athlete.competitions ?? [];
  const radar = buildRadar(competitions);

  return (
    <div className="pt-8">
      <nav className="mb-6 flex items-center gap-2 text-xs uppercase tracking-widest text-fog">
        <Link href="/" className="transition-colors hover:text-amber">
          Anasayfa
        </Link>
        {athlete.team && (
          <>
            <span>/</span>
            <Link href={`/takim/${athlete.team.slug}`} className="transition-colors hover:text-amber">
              {athlete.team.name}
            </Link>
          </>
        )}
        <span>/</span>
        <span className="text-amber">Oyuncu</span>
      </nav>

      <div className="grid gap-8 lg:grid-cols-[380px_minmax(0,1fr)]">
        <div>
          <PlayerCard athlete={athlete} />

          {athlete.birthPlace && (
            <p className="mt-4 text-center text-xs text-fog">Doğum yeri: {athlete.birthPlace}</p>
          )}
        </div>

        <div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-5">
            <StatCounter label="Maç" value={athlete.appearances} />
            <StatCounter label="Gol" value={athlete.goals} />
            <StatCounter label="Asist" value={athlete.assists} />
            <StatCounter label="Dakika" value={athlete.minutesPlayed} />
            <StatCounter label="Reyting" value={athlete.rating} decimals={2} />
          </div>

          {hasRadarData(radar) && (
            <Reveal className="mt-8">
              <PlayerRadar axes={radar} />
            </Reveal>
          )}
        </div>
      </div>

      {competitions.length > 0 && (
        <section className="mt-16">
          <SectionTitle>Turnuvalara göre</SectionTitle>
          <Reveal>
            <CompetitionBreakdown rows={competitions} />
          </Reveal>
          {athlete.statsSeason && (
            <p className="mt-3 text-xs text-fog">{athlete.statsSeason} sezonu verileri.</p>
          )}
        </section>
      )}
    </div>
  );
}
