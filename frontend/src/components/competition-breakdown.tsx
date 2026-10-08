import type { AthleteCompetition } from "@/lib/sports";

/** Kulüp ligi, Avrupa kupası ve millî takım aynı tabloda ama ayrı satırlarda. */
export function CompetitionBreakdown({ rows }: { rows: AthleteCompetition[] }) {
  if (rows.length === 0) return null;

  const sorted = [...rows].sort((a, b) => b.minutes - a.minutes);

  return (
    <div className="overflow-x-auto rounded-xl border border-line bg-surface">
      <table className="w-full min-w-[560px] border-collapse text-sm">
        <thead>
          <tr className="border-b border-line text-left font-display text-xs uppercase tracking-widest text-fog">
            <th className="py-3 pl-4 pr-2 font-semibold">Turnuva</th>
            <th className="px-2 py-3 text-center font-semibold">Maç</th>
            <th className="px-2 py-3 text-center font-semibold">Dakika</th>
            <th className="px-2 py-3 text-center font-semibold">Gol</th>
            <th className="px-2 py-3 text-center font-semibold">Asist</th>
            <th className="px-4 py-3 text-center font-semibold">Reyting</th>
          </tr>
        </thead>
        <tbody>
          {sorted.map((row, index) => (
            <tr key={index} className="border-b border-line/60 last:border-b-0">
              <td className="py-3 pl-4 pr-2">
                <div className="font-medium">{row.leagueName ?? "-"}</div>
                <div className="mt-0.5 text-xs text-fog">
                  {row.teamName}
                  {row.isNationalTeam && (
                    <span className="ml-2 rounded bg-amber/15 px-1.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-amber">
                      millî takım
                    </span>
                  )}
                </div>
              </td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.appearances}</td>
              <td className="px-2 py-3 text-center tabular-nums text-fog">{row.minutes}</td>
              <td className="px-2 py-3 text-center font-display font-bold tabular-nums">
                {row.goals}
              </td>
              <td className="px-2 py-3 text-center font-display font-bold tabular-nums">
                {row.assists}
              </td>
              <td className="px-4 py-3 text-center tabular-nums text-fog">
                {row.rating ? row.rating.toFixed(2) : "-"}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
