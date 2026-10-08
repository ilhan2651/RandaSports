"use client";

import Link from "next/link";
import type { Story } from "@/lib/api";

/** Devam eden olayların kayan şeridi. Fare üstüne gelince duruyor. */
export function LiveTicker({ stories }: { stories: Story[] }) {
  if (stories.length === 0) return null;

  const loop = [...stories, ...stories];

  return (
    <div className="group relative mt-4 flex items-stretch overflow-hidden rounded-xl border border-line bg-surface">
      <div className="z-10 flex shrink-0 items-center gap-2 bg-live/10 px-4">
        <span className="size-2 rounded-full bg-live animate-pulse-live" />
        <span className="font-display text-sm font-bold uppercase tracking-widest text-live">
          Canlı
        </span>
      </div>

      <div className="relative flex-1 overflow-hidden py-3">
        <div className="flex w-max animate-marquee gap-10 group-hover:[animation-play-state:paused]">
          {loop.map((story, index) => (
            <Link
              key={`${story.id}-${index}`}
              href={story.slug ? `/haber/${story.slug}` : "/"}
              className="flex items-center gap-3 whitespace-nowrap text-sm text-fog transition-colors hover:text-chalk"
            >
              <span className="size-1.5 rounded-full bg-amber" />
              {story.headline}
            </Link>
          ))}
        </div>
      </div>

      <div className="pointer-events-none absolute inset-y-0 right-0 w-16 bg-gradient-to-l from-surface to-transparent" />
    </div>
  );
}
