import type { Metadata } from "next";
import { getStories } from "@/lib/api";
import { getSports } from "@/lib/sports-nav";
import { EmptyState } from "@/components/empty-state";
import { Pagination } from "@/components/pagination";
import { Reveal } from "@/components/reveal";
import { SportChips } from "@/components/sport-chips";
import { StoryCard } from "@/components/story-card";

export const revalidate = 60;

const PAGE_SIZE = 24;

type Props = {
  searchParams: Promise<{ q?: string; page?: string; sport?: string }>;
};

export async function generateMetadata({ searchParams }: Props): Promise<Metadata> {
  const { q } = await searchParams;
  return { title: q ? `"${q}" araması` : "Arama", robots: { index: false } };
}

export default async function SearchPage({ searchParams }: Props) {
  const { q, page: rawPage, sport } = await searchParams;
  const term = q?.trim() ?? "";
  const page = Math.max(1, Number(rawPage) || 1);

  if (term.length < 2) {
    return <EmptyState title="Ne arıyorsun?" hint="Aramak için en az iki harf yaz." />;
  }

  const [{ items, total, pageSize }, sports] = await Promise.all([
    getStories({ search: term, sport, page, pageSize: PAGE_SIZE }),
    getSports(true),
  ]);

  const active = sports.find((x) => x.slug === sport);

  // Branş değişince sayfa numarası taşınmıyor: yeni filtrede 5. sayfa boş kalabilir.
  const chipHref = (slug: string | null) => {
    const params = new URLSearchParams({ q: term });
    if (slug) params.set("sport", slug);
    return `/arama?${params}`;
  };

  return (
    <div className="pt-8">
      <div className="mb-6 flex flex-wrap items-end gap-4">
        <h1 className="font-display text-4xl font-extrabold uppercase leading-none tracking-tight sm:text-5xl">
          {term}
        </h1>
        <span className="mb-1.5 font-display text-sm uppercase tracking-[0.2em] text-fog">
          {active ? `${active.name} · ${total} sonuç` : `${total} sonuç`}
        </span>
      </div>

      <div className="mb-8">
        <SportChips sports={sports} active={sport} href={chipHref} />
      </div>

      {items.length === 0 ? (
        <EmptyState
          title="Sonuç bulunamadı"
          hint={
            active
              ? `"${term}" için ${active.name} branşında sonuç yok. Çipten "Tümü"ne dönüp tekrar dene.`
              : "Başka bir takım ya da oyuncu adıyla dene."
          }
        />
      ) : (
        <>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((story, index) => (
              <Reveal key={story.id} delay={(index % 3) * 0.08}>
                <StoryCard story={story} />
              </Reveal>
            ))}
          </div>

          <Pagination
            page={page}
            pageSize={pageSize}
            total={total}
            basePath="/arama"
            params={sport ? { q: term, sport } : { q: term }}
          />
        </>
      )}
    </div>
  );
}
