import type { Metadata } from "next";
import Link from "next/link";
import { cikisYap, updateTeamLogo } from "@/app/admin/actions";
import { AdminForm } from "@/components/admin-form";
import { EntityAvatar } from "@/components/entity-avatar";
import { authHeader } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Takım logoları",
  robots: { index: false, follow: false },
};

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

type Team = {
  id: string;
  name: string;
  slug: string;
  country: string | null;
  isNational: boolean;
  sportSlug: string | null;
  logoUrl: string | null;
};

type Params = { searchParams: Promise<{ ok?: string; hata?: string }> };

export default async function AdminTeamsPage({ searchParams }: Params) {
  const { ok, hata } = await searchParams;
  const teams = await getTeams();

  const eksik = teams.filter((x) => !x.logoUrl);

  return (
    <div className="pt-8">
      <header className="border-b border-line pb-5">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Yönetim
        </p>
        <h1 className="mt-2 font-display text-3xl font-extrabold uppercase tracking-tight">
          Takım logoları
        </h1>
        <p className="mt-3 max-w-[70ch] text-sm leading-relaxed text-fog">
          Logolar önce API-Football senkronundan, sonra Wikidata aramasından
          dolduruluyor. Kulüp armaları ticari marka olduğu için çoğu açık lisanslı
          arşivlerde yok — ikisinin de bulamadığı takımlar burada elle giriliyor.
          Adresi boşaltıp kaydedersen arka plan işi o takımı yeniden denemeye başlıyor.
        </p>
      </header>

      <nav className="mt-6 flex flex-wrap gap-2">
        <Link
          href="/admin/yorumlar"
          className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
        >
          Görüş onayı
        </Link>
        <Link
          href="/admin/yorumcular"
          className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
        >
          Yorumcular
        </Link>
        <Link
          href="/admin/kanallar"
          className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
        >
          Kanallar
        </Link>

        <form action={cikisYap} className="ml-auto">
          <button
            type="submit"
            className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-live/40 hover:text-live"
          >
            Çıkış
          </button>
        </form>
      </nav>

      <p className="mt-6 font-display text-xs font-bold uppercase tracking-widest text-fog">
        {teams.length} takım
        {eksik.length > 0 && <span className="text-amber"> · {eksik.length} logosuz</span>}
      </p>

      <div className="mt-4 space-y-2">
        {teams.map((team) => (
          <TeamRow key={team.id} team={team} />
        ))}
      </div>
    </div>
  );
}

function TeamRow({ team }: { team: Team }) {
  return (
    <AdminForm
      action={updateTeamLogo}
      className="flex flex-wrap items-center gap-3 rounded-xl border border-line bg-surface px-4 py-3"
      quiet
    >
      <input type="hidden" name="id" value={team.id} />

      <EntityAvatar name={team.name} src={team.logoUrl} size={40} className="!rounded-md" />

      <div className="min-w-[12rem] flex-1">
        <p className="font-display text-sm font-bold uppercase tracking-tight text-chalk">
          {team.name}
        </p>
        <p className="text-xs text-fog">
          {team.isNational ? "Millî takım" : team.country}
          {team.sportSlug && ` · ${team.sportSlug}`}
        </p>
      </div>

      <input
        type="url"
        name="logoUrl"
        defaultValue={team.logoUrl ?? ""}
        placeholder="https://… logo adresi"
        className="min-w-0 flex-[2] rounded-lg border border-line bg-ink px-3 py-2 text-sm text-chalk outline-none placeholder:text-fog/60 focus-visible:border-amber"
      />

      <button
        type="submit"
        className="rounded-lg bg-amber px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft"
      >
        Kaydet
      </button>
    </AdminForm>
  );
}

async function getTeams(): Promise<Team[]> {
  try {
    const response = await fetch(`${API_BASE}/api/admin/teams`, {
      headers: await authHeader(),
      cache: "no-store",
    });

    if (!response.ok) return [];

    const payload = (await response.json()) as { success: boolean; data?: Team[] };
    return payload.success ? (payload.data ?? []) : [];
  } catch {
    return [];
  }
}
