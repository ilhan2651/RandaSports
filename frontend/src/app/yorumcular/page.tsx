import type { Metadata } from "next";
import { CommentatorFilters } from "@/components/commentator-filters";
import { EmptyState } from "@/components/empty-state";
import { OpinionDayGroups } from "@/components/opinion-day-groups";
import { Pagination } from "@/components/pagination";
import {
  getCommentators,
  getLatestOpinions,
  getOpinionSports,
  getOpinionTeams,
} from "@/lib/commentary";

// Önbellek yok, bilerek: bu sayfa searchParams okuyor (sayfalama ve süzgeçler) ve
// sorgu dizesine bakan bir sayfa önceden üretilemiyor. Burada `revalidate` yazmak
// Next'e "bunu önbelleğe al" demek oluyordu ve üretim derlemesinde sayfa
// DYNAMIC_SERVER_USAGE ile 500 veriyordu.

const SITE_URL = process.env.SITE_URL ?? "http://localhost:3000";
const PAGE_SIZE = 18;

export const metadata: Metadata = {
  title: "Yorumcular",
  description:
    "Spor yorumcularının yayınlarda söyledikleri: kim ne dedi, hangi takım için, hangi videoda, hangi dakikada.",
  alternates: { canonical: `${SITE_URL}/yorumcular` },
};

type Params = {
  searchParams: Promise<{
    yorumcu?: string;
    takim?: string;
    brans?: string;
    gun?: string;
    page?: string;
  }>;
};

export default async function CommentatorsPage({ searchParams }: Params) {
  const { yorumcu, takim, brans, gun, page: pageParam } = await searchParams;

  const days = Number(gun) > 0 ? Number(gun) : 0;
  const page = Number(pageParam) > 0 ? Number(pageParam) : 1;

  const [commentators, teams, sports, opinions] = await Promise.all([
    getCommentators(),
    getOpinionTeams(),
    getOpinionSports(),
    getLatestOpinions({
      commentator: yorumcu,
      team: takim,
      sport: brans,
      days,
      page,
      pageSize: PAGE_SIZE,
    }),
  ]);

  const selectedCommentator = commentators.find((x) => x.slug === yorumcu);
  const selectedTeam = teams.find((x) => x.slug === takim);
  const selectedSport = sports.find((x) => x.slug === brans);

  const scope = [selectedCommentator?.fullName, selectedTeam?.name, selectedSport?.name]
    .filter(Boolean)
    .join(" · ");

  return (
    <div className="pt-8">
      <header className="border-b border-line pb-6">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Yayınlardan
        </p>
        <h1 className="mt-2 font-display text-4xl font-extrabold uppercase leading-[0.95] tracking-tight sm:text-5xl">
          Yorumcular ne dedi
        </h1>
        <p className="mt-4 max-w-[68ch] leading-relaxed text-fog">
          Spor programlarında söylenenler tek tek çıkarıldı, konuşmacısı doğrulandı ve
          yayına alınmadan önce kontrol edildi. Karttaki görsele basınca video, sözün
          söylendiği saniyeden başlıyor.
        </p>
      </header>

      <div className="mt-6">
        <CommentatorFilters
          commentators={commentators}
          teams={teams}
          sports={sports}
          activeCommentator={yorumcu}
          activeTeam={takim}
          activeSport={brans}
          activeDays={days}
        />
      </div>

      <p className="mt-6 text-sm text-fog">
        {scope ? `${scope} — ` : ""}
        {opinions.total} görüş
      </p>

      {opinions.items.length === 0 ? (
        <EmptyState
          title="Bu filtrede görüş yok"
          hint="Zaman aralığını genişletmeyi ya da takım/yorumcu seçimini kaldırmayı dene."
        />
      ) : (
        <>
          <div className="mt-6">
            <OpinionDayGroups opinions={opinions.items} />
          </div>

          <Pagination
            page={opinions.page}
            pageSize={opinions.pageSize}
            total={opinions.total}
            basePath="/yorumcular"
            params={{
              ...(yorumcu ? { yorumcu } : {}),
              ...(takim ? { takim } : {}),
              ...(brans ? { brans } : {}),
              ...(days > 0 ? { gun: String(days) } : {}),
            }}
          />
        </>
      )}
    </div>
  );
}
