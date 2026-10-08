import Link from "next/link";
import type { StandingRow } from "@/lib/sports";

/** Sıralama bantları: Avrupa kupaları üstte, küme düşme hattı altta. */
function rankAccent(rank: number, total: number): string {
  if (rank <= 2) return "bg-amber";
  if (rank <= 4) return "bg-amber/50";
  if (rank > total - 3) return "bg-live/70";
  return "bg-line";
}

function FormPill({ form }: { form: string | null }) {
  if (!form) return null;

  const results = form.slice(-5).split("");

  return (
    <div className="flex gap-1">
      {results.map((result, index) => (
        <span
          key={index}
          title={result}
          className={`grid size-5 place-items-center rounded text-[10px] font-bold ${
            result === "W"
              ? "bg-amber/20 text-amber"
              : result === "D"
                ? "bg-line text-fog"
                : "bg-live/20 text-live"
          }`}
        >
          {result === "W" ? "G" : result === "D" ? "B" : "M"}
        </span>
      ))}
    </div>
  );
}

export function StandingsTable({ rows }: { rows: StandingRow[] }) {
  return (
    <div className="overflow-x-auto rounded-xl border border-line bg-surface">
      <table className="w-full min-w-[720px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-line text-left font-display text-xs uppercase tracking-widest text-fog">
            <th className="py-3 pl-4 pr-2 font-semibold">#</th>
            <th className="px-2 py-3 font-semibold">Takım</th>
            <th className="px-2 py-3 text-center font-semibold">O</th>
            <th className="px-2 py-3 text-center font-semibold">G</th>
            <th className="px-2 py-3 text-center font-semibold">B</th>
            <th className="px-2 py-3 text-center font-semibold">M</th>
            <th className="px-2 py-3 text-center font-semibold">A</th>
            <th className="px-2 py-3 text-center font-semibold">Y</th>
            <th className="px-2 py-3 text-center font-semibold">Av</th>
            <th className="px-2 py-3 text-center font-semibold text-amber">P</th>
            <th className="px-4 py-3 font-semibold">Form</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr
              key={row.team.slug}
              className="border-b border-line/60 transition-colors last:border-b-0 hover:bg-raised"
            >
              <td className="py-3 pl-4 pr-2">
                <div className="flex items-center gap-2.5">
                  <span className={`h-5 w-0.5 rounded-full ${rankAccent(row.rank, rows.length)}`} />
                  <span className="font-display font-bold tabular-nums">{row.rank}</span>
                </div>
              </td>
              <td className="px-2 py-3">
                <Link
                  href={`/takim/${row.team.slug}`}
                  className="group flex items-center gap-3 outline-none focus-visible:ring-2 focus-visible:ring-amber"
                >
                  {row.team.logoUrl && (
                    // eslint-disable-next-line @next/next/no-img-element
                    <img src={row.team.logoUrl} alt="" className="size-6 object-contain" />
                  )}
                  <span className="font-medium transition-colors group-hover:text-amber">
                    {row.team.name}
                  </span>
                </Link>
              </td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.played}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.won}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.drawn}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.lost}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.goalsFor}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.goalsAgainst}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">
                {row.goalDifference > 0 ? `+${row.goalDifference}` : row.goalDifference}
              </td>
              <td className="px-2 py-3 text-center font-display text-base font-extrabold tabular-nums text-amber">
                {row.points}
              </td>
              <td className="px-4 py-3">
                <FormPill form={row.form} />
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
