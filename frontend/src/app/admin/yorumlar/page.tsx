import type { Metadata } from "next";
import Link from "next/link";
import { OpinionReviewList } from "@/components/opinion-review-list";
import { getOpinions } from "@/lib/commentary";

export const metadata: Metadata = {
  title: "Görüş onayı",
  robots: { index: false, follow: false },
};

const TABS = [
  { status: "Pending", label: "Onay bekleyen" },
  { status: "Approved", label: "Yayında" },
  { status: "Rejected", label: "Reddedilen" },
] as const;

type Params = { searchParams: Promise<{ durum?: string }> };

export default async function AdminOpinionsPage({ searchParams }: Params) {
  const { durum } = await searchParams;
  const status = TABS.find((tab) => tab.status === durum)?.status ?? "Pending";

  const opinions = await getOpinions(status, 1, 50);

  return (
    <div className="pt-8">
      <header className="border-b border-line pb-5">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Yönetim
        </p>
        <h1 className="mt-2 font-display text-3xl font-extrabold uppercase tracking-tight">
          Görüş onayı
        </h1>
        <p className="mt-3 max-w-[70ch] text-sm leading-relaxed text-fog">
          Modelin videodan çıkardığı görüşler burada bekler. Her kayıtta makinenin
          bir ön kontrolü var ama o bir öneri: onay, alıntıyı doğrulanmış sayar ve
          o imza sende. Emin olmadığın kayıtta videoyu aç.
        </p>
      </header>

      <nav className="mt-6 flex flex-wrap gap-2">
        {TABS.map((tab) => (
          <Link
            key={tab.status}
            href={`/admin/yorumlar?durum=${tab.status}`}
            className={`rounded-lg border px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest transition-colors ${
              tab.status === status
                ? "border-amber bg-amber text-ink"
                : "border-line text-fog hover:border-amber/40 hover:text-chalk"
            }`}
          >
            {tab.label}
          </Link>
        ))}
      </nav>

      <p className="mt-5 text-sm text-fog">{opinions.total} kayıt</p>

      {opinions.items.length === 0 ? (
        <p className="mt-10 rounded-xl border border-line bg-surface p-8 text-center text-fog">
          Bu listede kayıt yok.
        </p>
      ) : (
        <div className="mt-5">
          <OpinionReviewList opinions={opinions.items} pending={status === "Pending"} />
        </div>
      )}
    </div>
  );
}
