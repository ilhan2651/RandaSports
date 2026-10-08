import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { FixtureList } from "@/components/fixture-list";
import { Reveal } from "@/components/reveal";
import { SportNav } from "@/components/sport-nav";
import { getFixtures } from "@/lib/sports";
import { findSport } from "@/lib/sports-nav";

export const revalidate = 300;

type Props = { params: Promise<{ sport: string }> };

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { sport } = await params;
  const match = await findSport(sport);

  return { title: match ? `${match.name} fikstür` : "Fikstür" };
}

export default async function FixturesPage({ params }: Props) {
  const { sport } = await params;
  const match = await findSport(sport);

  // Ligi olmayan branşın fikstürü hiç yok: boş sayfa yerine 404 veriyoruz.
  if (!match || !match.hasCompetitions) notFound();

  const fixtures = await getFixtures({ sport: match.slug });

  return (
    <div className="pt-8">
      <SportNav sport={match.slug} hasCompetitions={match.hasCompetitions} />

      <h1 className="mb-6 font-display text-4xl font-extrabold uppercase leading-none tracking-tight sm:text-5xl">
        Fikstür
      </h1>

      <Reveal>
        <FixtureList fixtures={fixtures} />
      </Reveal>
    </div>
  );
}
