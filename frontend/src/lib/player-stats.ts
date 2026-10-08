import type { AthleteCompetition } from "@/lib/sports";

export type RadarAxis = {
  label: string;
  /** 0-1 arası normalize değer; grafiğin yarıçapı bu. */
  value: number;
  /** Okuyucunun göreceği gerçek değer. */
  display: string;
};

function ratio(part: number, whole: number): number {
  return whole > 0 ? Math.min(part / whole, 1) : 0;
}

function per90(total: number, minutes: number): number {
  return minutes > 0 ? (total * 90) / minutes : 0;
}

/**
 * Radar eksenleri. Gol ve asist dakikaya göre normalize ediliyor:
 * yoksa çok oynayan oyuncu her eksende haksız yere önde çıkar.
 * Referans: 90 dakikada 1 gol ve 0.75 asist tavan kabul ediliyor.
 */
export function buildRadar(competitions: AthleteCompetition[]): RadarAxis[] {
  const rows = competitions.length > 0 ? competitions : [];

  const sum = (pick: (row: AthleteCompetition) => number) =>
    rows.reduce((total, row) => total + pick(row), 0);

  const minutes = sum((x) => x.minutes);
  const goals = sum((x) => x.goals);
  const assists = sum((x) => x.assists);

  const goalsPer90 = per90(goals, minutes);
  const assistsPer90 = per90(assists, minutes);

  const passAccuracy =
    rows.length > 0
      ? rows.reduce((total, row) => total + (row.passAccuracy ?? 0) * row.minutes, 0) /
        Math.max(minutes, 1)
      : 0;

  return [
    {
      label: "Gol",
      value: Math.min(goalsPer90 / 1, 1),
      display: `${goalsPer90.toFixed(2)}/90`,
    },
    {
      label: "Asist",
      value: Math.min(assistsPer90 / 0.75, 1),
      display: `${assistsPer90.toFixed(2)}/90`,
    },
    {
      label: "Şut isabeti",
      value: ratio(sum((x) => x.shotsOn), sum((x) => x.shotsTotal)),
      display: `%${Math.round(ratio(sum((x) => x.shotsOn), sum((x) => x.shotsTotal)) * 100)}`,
    },
    {
      label: "Pas isabeti",
      value: Math.min(passAccuracy / 100, 1),
      display: `%${Math.round(passAccuracy)}`,
    },
    {
      label: "İkili mücadele",
      value: ratio(sum((x) => x.duelsWon), sum((x) => x.duelsTotal)),
      display: `%${Math.round(ratio(sum((x) => x.duelsWon), sum((x) => x.duelsTotal)) * 100)}`,
    },
    {
      label: "Çalım",
      value: ratio(sum((x) => x.dribblesSuccess), sum((x) => x.dribblesAttempts)),
      display: `%${Math.round(
        ratio(sum((x) => x.dribblesSuccess), sum((x) => x.dribblesAttempts)) * 100,
      )}`,
    },
  ];
}

export function hasRadarData(axes: RadarAxis[]): boolean {
  return axes.some((axis) => axis.value > 0);
}
