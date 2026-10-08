import Link from "next/link";
import type { Story } from "@/lib/api";
import { StoryImage } from "@/components/story-image";
import { categoryLabel, timeAgo } from "@/lib/format";

export function StoryCard({ story }: { story: Story }) {
  return (
    <Link
      href={story.slug ? `/haber/${story.slug}` : "/"}
      className="group flex h-full flex-col overflow-hidden rounded-xl border border-line bg-surface outline-none transition-colors duration-300 hover:border-amber/50 focus-visible:ring-2 focus-visible:ring-amber focus-visible:ring-offset-2 focus-visible:ring-offset-ink"
    >
      <div className="relative aspect-[16/10] overflow-hidden bg-raised">
        <StoryImage
          src={story.imageUrl}
          sizes="(max-width: 768px) 100vw, 400px"
          className="object-cover transition-transform duration-700 ease-out group-hover:scale-[1.07]"
        />

        <div className="absolute inset-x-0 bottom-0 h-16 bg-gradient-to-t from-surface to-transparent" />

        <span className="absolute left-3 top-3 rounded-full bg-ink/80 px-2.5 py-1 font-display text-[11px] font-bold uppercase tracking-widest text-amber backdrop-blur">
          {categoryLabel(story.category)}
        </span>

        {story.isLive && (
          <span className="absolute right-3 top-3 flex items-center gap-1.5 rounded-full bg-live/90 px-2.5 py-1 font-display text-[11px] font-bold uppercase tracking-widest text-chalk">
            <span className="size-1.5 rounded-full bg-chalk" />
            Canlı
          </span>
        )}
      </div>

      <div className="flex flex-1 flex-col p-4">
        <h3 className="font-display text-xl font-bold uppercase leading-tight tracking-tight transition-colors duration-300 group-hover:text-amber">
          {story.headline}
        </h3>

        {story.summary && (
          <p className="mt-2 line-clamp-3 text-sm leading-relaxed text-fog">{story.summary}</p>
        )}

        <div className="mt-auto flex items-center gap-2 pt-4 text-xs text-fog">
          <span>{timeAgo(story.updatedAt)}</span>
          <span className="size-1 rounded-full bg-line" />
          <span>{story.sourceCount} kaynak</span>
        </div>
      </div>

      <span className="h-0.5 w-0 bg-amber transition-all duration-500 ease-out group-hover:w-full" />
    </Link>
  );
}
