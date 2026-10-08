import Link from "next/link";
import { FilterPicker, type PickerItem } from "@/components/filter-picker";
import type { Commentator, SportFacet, TeamFacet } from "@/lib/commentary-shared";

const BASE_PATH = "/yorumcular";

const PERIODS = [
  { days: 0, label: "Tümü" },
  { days: 1, label: "Bugün" },
  { days: 7, label: "Bu hafta" },
  { days: 30, label: "Bu ay" },
] as const;

type Props = {
  commentators: Commentator[];
  teams: TeamFacet[];
  sports: SportFacet[];
  activeCommentator?: string;
  activeTeam?: string;
  activeSport?: string;
  activeDays: number;
};

/**
 * Filtreler bağlantı olarak çalışıyor: durum adres çubuğunda duruyor, sayfa
 * sunucuda render ediliyor ve filtreli hâli paylaşılabiliyor. Zaman dört seçenek
 * olduğu için çip kalıyor; takım ve yorumcu listeleri onlarca satır sürdüğü için
 * modalın arkasına alındı — filtre, filtrelediği içerikten fazla yer kaplamasın.
 */
export function CommentatorFilters({
  commentators,
  teams,
  sports,
  activeCommentator,
  activeTeam,
  activeSport,
  activeDays,
}: Props) {
  const href = (next: { commentator?: string; team?: string; sport?: string; days?: number }) => {
    const params = new URLSearchParams();
    const commentator = next.commentator ?? activeCommentator;
    const team = next.team ?? activeTeam;
    const sport = next.sport ?? activeSport;
    const days = next.days ?? activeDays;

    if (commentator) params.set("yorumcu", commentator);
    if (team) params.set("takim", team);
    if (sport) params.set("brans", sport);
    if (days > 0) params.set("gun", String(days));

    const query = params.toString();
    return query ? `${BASE_PATH}?${query}` : BASE_PATH;
  };

  // Modala düz veri geçiyoruz; adresi kendisi kuruyor.
  const query: Record<string, string> = {};

  if (activeCommentator) query.yorumcu = activeCommentator;
  if (activeTeam) query.takim = activeTeam;
  if (activeSport) query.brans = activeSport;
  if (activeDays > 0) query.gun = String(activeDays);

  const takimlar: PickerItem[] = teams.map((team) => ({
    value: team.slug,
    label: team.name,
    imageUrl: team.logoUrl,
    count: team.opinionCount,
  }));

  // Yorumcu filtresi yalnızca yorumcuları listeliyor. Sözlükte futbolcu ve teknik
  // direktör de var — sözleri doğru atfedilmiş ama onlar yorum değil demeç veriyor,
  // bu listede yerleri yok. Rolü henüz belirlenmemiş olanlar görünmeye devam ediyor.
  const kisiler: PickerItem[] = commentators
    .filter((x) => x.personRole !== "Athlete" && x.personRole !== "Coach" && x.personRole !== "Official")
    .map((person) => ({
      value: person.slug,
      label: person.fullName,
      imageUrl: person.photoUrl,
      count: person.opinionCount,
    }));

  return (
    <div className="flex flex-wrap items-center gap-x-5 gap-y-3">
      <div className="flex flex-wrap items-center gap-2">
        <Etiket>Zaman</Etiket>
        {PERIODS.map((period) => (
          <Chip
            key={period.days}
            href={href({ days: period.days })}
            active={activeDays === period.days}
          >
            {period.label}
          </Chip>
        ))}
      </div>

      {sports.length > 1 && (
        <FilterPicker
          label="Branş"
          items={sports.map((sport) => ({
            value: sport.slug,
            label: sport.name,
            imageUrl: null,
            count: sport.opinionCount,
          }))}
          active={activeSport}
          paramName="brans"
          basePath={BASE_PATH}
          query={query}
          shape="square"
        />
      )}

      {takimlar.length > 0 && (
        <FilterPicker
          label="Takım"
          items={takimlar}
          active={activeTeam}
          paramName="takim"
          basePath={BASE_PATH}
          query={query}
          shape="square"
        />
      )}

      {kisiler.length > 0 && (
        <FilterPicker
          label="Yorumcu"
          items={kisiler}
          active={activeCommentator}
          paramName="yorumcu"
          basePath={BASE_PATH}
          query={query}
        />
      )}
    </div>
  );
}

function Etiket({ children }: { children: React.ReactNode }) {
  return (
    <span className="font-display text-[10px] font-bold uppercase tracking-widest text-fog">
      {children}
    </span>
  );
}

function Chip({
  href,
  active,
  children,
}: {
  href: string;
  active: boolean;
  children: React.ReactNode;
}) {
  return (
    <Link
      href={href}
      className={`rounded-lg border px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
        active
          ? "border-amber bg-amber text-ink"
          : "border-line text-fog hover:border-amber/40 hover:text-chalk"
      }`}
    >
      {children}
    </Link>
  );
}
