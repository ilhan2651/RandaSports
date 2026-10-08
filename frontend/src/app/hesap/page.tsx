import type { Metadata } from "next";
import Link from "next/link";
import { redirect } from "next/navigation";
import { savePreferences } from "@/app/hesap/actions";
import { FollowChips } from "@/components/follow-chips";
import { PreferenceEditor } from "@/components/preference-editor";
import { currentRoles, signOut } from "@/lib/auth";
import { getMe, getOnboardingOptions, getPreferences } from "@/lib/me";

export const metadata: Metadata = {
  title: "Hesabım",
  robots: { index: false, follow: false },
};

export default async function AccountPage() {
  const me = await getMe();

  // Jeton düşmüş ya da hiç yok: girişe alıyoruz.
  if (!me) redirect("/giris?devam=/hesap");

  const roles = await currentRoles();
  const tercihler = await getPreferences();

  // Sihirbaz artık akış sayfasında karşılıyor; buradaki liste düzenleme panelinin girdisi.
  const secenekler = await getOnboardingOptions();

  async function cikisYap(): Promise<void> {
    "use server";
    await signOut();
    redirect("/");
  }

  return (
    <div className="pt-8">
      <header className="border-b border-line pb-6">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Hesabım
        </p>
        <h1 className="mt-2 font-display text-3xl font-extrabold uppercase tracking-tight">
          {me.fullName || me.email}
        </h1>
        <p className="mt-2 text-sm text-fog">{me.email}</p>
      </header>

      <section className="mt-8">
        <h2 className="font-display text-xs font-bold uppercase tracking-widest text-fog">
          Takip listem
        </h2>

        {tercihler && (tercihler.sports.length > 0 || tercihler.commentators.length > 0 || tercihler.teams.length > 0) ? (
          <div className="mt-4 space-y-5">
            <FollowChips title="Branşlar" items={tercihler.sports} hrefPrefix="" />
            <FollowChips
              title="Yorumcular"
              items={tercihler.commentators}
              round
              hrefPrefix="/yorumcu"
            />
            <FollowChips title="Takımlar" items={tercihler.teams} hrefPrefix="/takim" />
          </div>
        ) : (
          <div className="mt-4 rounded-xl border border-dashed border-line bg-surface px-6 py-12 text-center">
            <p className="text-sm text-fog">Henüz bir şey takip etmiyorsun.</p>
            <p className="mt-2 text-xs text-fog">
              Branş, yorumcu ve takım seç; akışın yalnızca onlardan gelsin.
            </p>
          </div>
        )}

        {secenekler && tercihler && (
          <PreferenceEditor
            options={secenekler}
            mevcut={tercihler}
            kaydet={savePreferences}
          />
        )}
      </section>

      <nav className="mt-8 flex flex-wrap gap-2 border-t border-line pt-6">
        <Link
          href="/akis"
          className="rounded-lg border border-amber/40 bg-amber/10 px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-amber transition-colors hover:bg-amber/20"
        >
          Akışım
        </Link>

        {roles.includes("admin") && (
          <Link
            href="/admin/yorumlar"
            className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
          >
            Yönetim
          </Link>
        )}

        <form action={cikisYap} className="ml-auto">
          <button
            type="submit"
            className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-live/40 hover:text-live"
          >
            Çıkış
          </button>
        </form>
      </nav>

    </div>
  );
}
