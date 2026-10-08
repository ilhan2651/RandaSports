import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getStories, getStory } from "@/lib/api";
import { getStoryOpinions } from "@/lib/commentary";
import { getSports } from "@/lib/sports-nav";
import { OpinionList } from "@/components/opinion-list";
import { ReadingProgress } from "@/components/reading-progress";
import { Reveal } from "@/components/reveal";
import { SectionTitle } from "@/components/section-title";
import { StoryCard } from "@/components/story-card";
import { StoryImage } from "@/components/story-image";
import { categoryLabel, fullDate, sportLabel, timeAgo } from "@/lib/format";

export const revalidate = 60;

const SITE_URL = process.env.SITE_URL ?? "http://localhost:3000";

type Params = { params: Promise<{ slug: string }> };

export async function generateMetadata({ params }: Params): Promise<Metadata> {
  const { slug } = await params;
  const story = await getStory(slug);

  if (!story) return { title: "Haber bulunamadı" };

  return {
    title: story.headline ?? "Haber",
    description: story.summary ?? undefined,
    alternates: { canonical: `${SITE_URL}/haber/${slug}` },
    openGraph: {
      title: story.headline ?? "Haber",
      description: story.summary ?? undefined,
      images: story.imageUrl ? [story.imageUrl] : undefined,
      type: "article",
      publishedTime: story.publishedAt,
      modifiedTime: story.updatedAt,
    },
    twitter: {
      card: "summary_large_image",
      title: story.headline ?? "Haber",
      description: story.summary ?? undefined,
    },
  };
}

export default async function StoryPage({ params }: Params) {
  const { slug } = await params;
  const story = await getStory(slug);

  if (!story) notFound();

  const paragraphs = (story.body ?? "")
    .split(/\n{2,}/)
    .map((paragraph) => paragraph.trim())
    .filter(Boolean);

  // İki istek birbirine bağlı değil; paralel gidiyor.
  const [relatedPage, opinions, sports] = await Promise.all([
    story.sportSlug ? getStories({ sport: story.sportSlug, pageSize: 4 }) : null,
    getStoryOpinions(story.id),
    getSports(),
  ]);

  const related = (relatedPage?.items ?? [])
    .filter((item) => item.id !== story.id)
    .slice(0, 3);

  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "NewsArticle",
    headline: story.headline,
    description: story.summary,
    image: story.imageUrl ? [story.imageUrl] : undefined,
    datePublished: story.publishedAt,
    dateModified: story.updatedAt,
    mainEntityOfPage: `${SITE_URL}/haber/${slug}`,
    publisher: { "@type": "Organization", name: "RandaSports" },
    citation: story.sources.map((source) => ({
      "@type": "CreativeWork",
      name: source.title,
      url: source.url,
      publisher: { "@type": "Organization", name: source.sourceName },
    })),
  };

  return (
    <article className="pt-8">
      <ReadingProgress />

      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
      />

      <nav className="flex items-center gap-2 text-xs uppercase tracking-widest text-fog">
        <Link href="/" className="transition-colors hover:text-amber">
          Anasayfa
        </Link>
        {story.sportSlug && (
          <>
            <span>/</span>
            <Link href={`/${story.sportSlug}`} className="transition-colors hover:text-amber">
              {sportLabel(story.sportSlug, sports)}
            </Link>
          </>
        )}
        <span>/</span>
        <span className="text-amber">{categoryLabel(story.category)}</span>
      </nav>

      <header className="mt-6 max-w-4xl">
        {story.isLive && (
          <span className="mb-4 inline-flex items-center gap-2 rounded-full bg-live/15 px-3 py-1 font-display text-xs font-bold uppercase tracking-widest text-live">
            <span className="size-1.5 rounded-full bg-live animate-pulse-live" />
            Olay devam ediyor
          </span>
        )}

        <h1 className="font-display text-4xl font-extrabold uppercase leading-[0.95] tracking-tight sm:text-5xl lg:text-6xl">
          {story.headline}
        </h1>

        {story.summary && (
          <p className="mt-5 text-lg leading-relaxed text-fog sm:text-xl">{story.summary}</p>
        )}

        <div className="mt-6 flex flex-wrap items-center gap-3 border-y border-line py-3 text-xs text-fog">
          <span className="font-display font-bold uppercase tracking-widest text-amber">
            {story.sourceCount} kaynak
          </span>
          <span className="size-1 rounded-full bg-line" />
          <span>{fullDate(story.publishedAt)}</span>
          <span className="size-1 rounded-full bg-line" />
          <span>güncellendi {timeAgo(story.updatedAt)}</span>
        </div>
      </header>

      {story.imageUrl && (
        <Reveal className="mt-8">
          {/*
            Haber görselleri her kaynaktan farklı oranda geliyor: 16/9, 1.91/1 (og:image),
            kimi zaman 4/3. Kırpmak yerine görselin tamamını gösteriyoruz; kutuda kalan
            boşluğu da görselin bulanık, büyütülmüş kopyası dolduruyor.
          */}
          <div className="relative aspect-[16/9] overflow-hidden rounded-2xl border border-line bg-surface">
            <div className="absolute inset-0 scale-110 opacity-40 blur-2xl" aria-hidden>
              <StoryImage
                src={story.imageUrl}
                sizes="(max-width: 1280px) 100vw, 1280px"
                className="object-cover"
              />
            </div>

            <StoryImage
              src={story.imageUrl}
              priority
              sizes="(max-width: 1280px) 100vw, 1280px"
              className="object-contain"
            />
          </div>
        </Reveal>
      )}

      <div className="mt-10 grid gap-12 lg:grid-cols-[minmax(0,1fr)_320px]">
        <div>
          <div className="max-w-[68ch] space-y-5 text-[17px] leading-[1.75] text-chalk/90">
            {paragraphs.map((paragraph, index) => (
              <Reveal key={index} delay={Math.min(index * 0.05, 0.3)}>
                <p>{paragraph}</p>
              </Reveal>
            ))}
          </div>

          {story.analysis && (
            <Reveal className="mt-10">
              <aside className="relative max-w-[68ch] overflow-hidden rounded-xl border border-line bg-surface p-6">
                <span className="absolute inset-y-0 left-0 w-1 bg-amber" />
                <h2 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
                  Ne anlama geliyor
                </h2>
                <p className="mt-3 leading-relaxed text-chalk/90">{story.analysis}</p>
              </aside>
            </Reveal>
          )}
        </div>

        <aside className="lg:sticky lg:top-28 lg:self-start">
          <div className="rounded-xl border border-line bg-surface p-5">
            <h2 className="font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
              Kaynaklar
            </h2>
            <p className="mt-2 text-xs leading-relaxed text-fog">
              Bu haber aşağıdaki yayınların içeriklerinden derlendi.
            </p>

            <ul className="mt-4 space-y-3">
              {story.sources.map((source) => (
                <li key={source.url}>
                  <a
                    href={source.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="group block border-l-2 border-line pl-3 outline-none transition-colors hover:border-amber focus-visible:border-amber"
                  >
                    <span className="font-display text-xs font-bold uppercase tracking-widest text-amber">
                      {source.sourceName}
                    </span>
                    <span className="mt-1 block text-sm leading-snug text-fog transition-colors group-hover:text-chalk">
                      {source.title}
                    </span>
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </aside>
      </div>

      <OpinionList opinions={opinions} />

      {related.length > 0 && (
        <section className="mt-20">
          <SectionTitle>İlgili haberler</SectionTitle>
          <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {related.map((item, index) => (
              <Reveal key={item.id} delay={index * 0.08}>
                <StoryCard story={item} />
              </Reveal>
            ))}
          </div>
        </section>
      )}
    </article>
  );
}
