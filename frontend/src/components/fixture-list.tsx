import Link from "next/link";
import type { Fixture } from "@/lib/sports";

function dayLabel(value: string): string {
  return new Date(value).toLocaleDateString("tr-TR", {
    weekday: "long",
    day: "numeric",
    month: "long",
  });
}

function timeLabel(value: string): string {
  return new Date(value).toLocaleTimeString("tr-TR", { hour: "2-digit", minute: "2-digit" });
}

function Side({
  name,
  slug,
  logoUrl,
  align,
}: {
  name: string;
  slug: string;
  logoUrl: string | null;
  align: "left" | "right";
}) {
  return (
    <Link
      href={`/takim/${slug}`}
      className={`group flex min-w-0 flex-1 items-center gap-2.5 outline-none focus-visible:ring-2 focus-visible:ring-amber ${
        align === "right" ? "flex-row-reverse text-right" : ""
      }`}
    >
      {logoUrl && (
        // eslint-disable-next-line @next/next/no-img-element
        <img src={logoUrl} alt="" className="size-7 shrink-0 object-contain" />
      )}
      <span className="truncate font-medium transition-colors group-hover:text-amber">{name}</span>
    </Link>
  );
}

function FixtureRow({ fixture }: { fixture: Fixture }) {
  const finished = fixture.status === "Finished";
  const live = fixture.status === "Live";

  return (
    <div className="flex items-center gap-4 border-b border-line/60 px-4 py-3.5 transition-colors last:border-b-0 hover:bg-raised">
      <Side {...fixture.home} align="left" />

      <div className="flex w-24 shrink-0 flex-col items-center">
        {finished || live ? (
          <span
            className={`font-display text-lg font-extrabold tabular-nums ${live ? "text-live" : ""}`}
          >
            {fixture.home.score ?? 0} - {fixture.away.score ?? 0}
          </span>
        ) : (
          <span className="font-display text-lg font-bold tabular-nums text-fog">
            {timeLabel(fixture.startTime)}
          </span>
        )}

        {live && (
          <span className="mt-0.5 flex items-center gap-1 text-[11px] font-bold uppercase tracking-widest text-live">
            <span className="size-1.5 rounded-full bg-live animate-pulse-live" />
            {fixture.statusDetail ?? "canlı"}
          </span>
        )}

        {fixture.status === "Postponed" && (
          <span className="mt-0.5 text-[11px] uppercase tracking-widest text-fog">ertelendi</span>
        )}
      </div>

      <Side {...fixture.away} align="right" />
    </div>
  );
}

export function FixtureList({ fixtures }: { fixtures: Fixture[] }) {
  if (fixtures.length === 0) {
    return (
      <p className="rounded-xl border border-dashed border-line bg-surface px-6 py-12 text-center text-sm text-fog">
        Bu aralıkta maç yok.
      </p>
    );
  }

  const groups = new Map<string, Fixture[]>();

  for (const fixture of fixtures) {
    const key = fixture.startTime.slice(0, 10);
    groups.set(key, [...(groups.get(key) ?? []), fixture]);
  }

  return (
    <div className="space-y-8">
      {[...groups.entries()].map(([day, items]) => (
        <section key={day}>
          <h3 className="mb-3 font-display text-sm font-bold uppercase tracking-[0.2em] text-amber">
            {dayLabel(items[0].startTime)}
          </h3>
          <div className="overflow-hidden rounded-xl border border-line bg-surface">
            {items.map((fixture) => (
              <FixtureRow key={fixture.id} fixture={fixture} />
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}
