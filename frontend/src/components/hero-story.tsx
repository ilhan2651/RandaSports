import Link from "next/link";
import type { Story } from "@/lib/api";
import { StoryImage } from "@/components/story-image";
import { categoryLabel, timeAgo } from "@/lib/format";

export function HeroStory({ story }: { story: Story }) {
  return (
    <Link
      href={story.slug ? `/haber/${story.slug}` : "/"}
      className="group relative flex min-h-[420px] flex-col justify-end overflow-hidden rounded-2xl border border-line bg-surface outline-none focus-visible:ring-2 focus-visible:ring-amber focus-visible:ring-offset-2 focus-visible:ring-offset-ink lg:min-h-[560px]"
    >
      <StoryImage
        src={story.imageUrl}
        priority
        sizes="(max-width: 1024px) 100vw, 800px"
        className="object-cover transition-transform duration-[1200ms] ease-out group-hover:scale-105"
      />

      <div className="absolute inset-0 bg-gradient-to-t from-ink via-ink/75 to-transparent" />

      <div className="relative p-6 sm:p-8">
        <div className="flex flex-wrap items-center gap-3">
          <span className="rounded-full bg-amber px-3 py-1 font-display text-xs font-bold uppercase tracking-widest text-ink">
            {categoryLabel(story.category)}
          </span>
          {story.isLive && (
            <span className="flex items-center gap-2 rounded-full bg-live/15 px-3 py-1 font-display text-xs font-bold uppercase tracking-widest text-live">
              <span className="size-1.5 rounded-full bg-live animate-pulse-live" />
              Devam ediyor
            </span>
          )}
          <span className="text-xs text-fog">{timeAgo(story.updatedAt)}</span>
        </div>

        <h2 className="mt-4 max-w-4xl font-display text-4xl font-extrabold uppercase leading-[0.95] tracking-tight sm:text-5xl lg:text-6xl">
          {story.headline}
        </h2>

        {story.summary && (
          <p className="mt-4 max-w-2xl text-base leading-relaxed text-fog sm:text-lg">
            {story.summary}
          </p>
        )}

        <div className="mt-5 flex items-center gap-3">
          <span className="font-display text-xs font-semibold uppercase tracking-widest text-amber">
            {story.sourceCount} kaynak birleştirildi
          </span>
          <span className="h-px flex-1 bg-line transition-colors duration-500 group-hover:bg-amber" />
        </div>
      </div>
    </Link>
  );
}
