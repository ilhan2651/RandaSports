import Link from "next/link";
import type { Story } from "@/lib/api";
import { timeAgo } from "@/lib/format";

export function StoryListItem({ story, index }: { story: Story; index: number }) {
  return (
    <Link
      href={story.slug ? `/haber/${story.slug}` : "/"}
      className="group flex gap-4 border-b border-line py-4 last:border-b-0"
    >
      <span className="font-display text-2xl font-extrabold leading-none text-line transition-colors duration-300 group-hover:text-amber">
        {String(index + 1).padStart(2, "0")}
      </span>

      <div className="min-w-0">
        <h4 className="font-display text-base font-bold uppercase leading-tight tracking-tight transition-colors duration-300 group-hover:text-amber">
          {story.headline}
        </h4>
        <div className="mt-1.5 flex items-center gap-2 text-xs text-fog">
          <span>{timeAgo(story.updatedAt)}</span>
          {story.isLive && (
            <>
              <span className="size-1 rounded-full bg-line" />
              <span className="font-semibold text-live">canlı</span>
            </>
          )}
        </div>
      </div>
    </Link>
  );
}
