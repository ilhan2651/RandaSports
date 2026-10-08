import type { Metadata } from "next";
import Link from "next/link";
import {
  addChannel,
  scanChannel,
  setChannelSports,
  toggleChannel,
  cikisYap,
} from "@/app/admin/actions";
import { AdminForm } from "@/components/admin-form";
import { SportPicker } from "@/components/sport-picker";
import { getChannels } from "@/lib/commentary";
import { getSports } from "@/lib/sports-nav";
import { timeAgo } from "@/lib/format";

export const metadata: Metadata = {
  title: "Kanallar",
  robots: { index: false, follow: false },
};

type Params = { searchParams: Promise<{ ok?: string; hata?: string }> };

export default async function AdminChannelsPage({ searchParams }: Params) {
  const { ok, hata } = await searchParams;
  const [channels, sports] = await Promise.all([getChannels(), getSports(false)]);

  return (
    <div className="pt-8">
      <header className="border-b border-line pb-5">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Yönetim
        </p>
        <h1 className="mt-2 font-display text-3xl font-extrabold uppercase tracking-tight">
          Kanallar
        </h1>
        <p className="mt-3 max-w-[70ch] text-sm leading-relaxed text-fog">
          Taranan YouTube kanalları. Kullanıcı adı, kanal adresi veya UC... kimliği
          yapıştırabilirsin; kanal kimliği ilk taramada kendiliğinden çözülür.
        </p>
      </header>

      <nav className="mt-6 flex gap-2">
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
          Yorumcu onayı
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

      <p className="mt-4 max-w-[70ch] text-sm leading-relaxed text-fog">
        Kanalın işlediği branşları seç — birden fazla olabilir. Bu seçim görüşün
        branşını belirlemiyor: asıl kararı videoyu izleyen model veriyor, buradaki
        liste ona ipucu olarak gidiyor. Tek branş seçiliyse, model emin olamadığında
        son çare olarak kullanılıyor. Genel spor kanallarında boş bırak.
      </p>

      <AdminForm
        action={addChannel}
        className="mt-6 flex flex-wrap items-center gap-3 rounded-xl border border-line bg-surface p-5"
      >
        <input
          type="text"
          name="name"
          required
          placeholder="Kanal adı"
          className="min-w-48 flex-1 rounded-lg border border-line bg-raised px-3 py-2 text-sm text-chalk outline-none placeholder:text-fog focus-visible:border-amber"
        />
        <input
          type="text"
          name="reference"
          required
          placeholder="@kullaniciadi veya kanal adresi"
          className="min-w-64 flex-[2] rounded-lg border border-line bg-raised px-3 py-2 text-sm text-chalk outline-none placeholder:text-fog focus-visible:border-amber"
        />
        <SportPicker sports={sports} selected={[]} />

        <button
          type="submit"
          className="rounded-lg bg-amber px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft"
        >
          Kanal ekle
        </button>
      </AdminForm>

      {channels.length === 0 ? (
        <p className="mt-10 rounded-xl border border-line bg-surface p-8 text-center text-fog">
          Henüz kanal yok.
        </p>
      ) : (
        <div className="mt-5 space-y-3">
          {channels.map((channel) => (
            <article
              key={channel.id}
              className="flex flex-wrap items-center gap-4 rounded-xl border border-line bg-surface p-4"
            >
              <div className="min-w-48 flex-1">
                <p className="font-display text-sm font-bold uppercase tracking-wide">
                  {channel.name}
                </p>
                <p className="mt-0.5 text-xs text-fog">{channel.handle}</p>
              </div>

              <AdminForm
                action={setChannelSports}
                className="flex flex-wrap items-center gap-2"
                quiet
              >
                <input type="hidden" name="id" value={channel.id} />
                <SportPicker sports={sports} selected={channel.sportSlugs} />
                <button
                  type="submit"
                  className="rounded-lg border border-line px-2.5 py-1.5 font-display text-[10px] font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
                >
                  Kaydet
                </button>
              </AdminForm>

              <div className="min-w-40 text-xs text-fog">
                {channel.youTubeChannelId ? (
                  <span className="text-amber">kimlik çözüldü</span>
                ) : (
                  <span className="text-live">kimlik bekliyor</span>
                )}
                <br />
                {channel.lastCheckedAt
                  ? `son tarama ${timeAgo(channel.lastCheckedAt)}`
                  : "hiç taranmadı"}
              </div>

              {channel.lastError && (
                <p className="w-full rounded-lg border border-live/30 bg-live/5 px-3 py-2 text-xs text-live">
                  {channel.lastError}
                </p>
              )}

              <div className="flex items-center gap-2">
                <AdminForm action={scanChannel} className="flex items-center gap-2">
                  <input type="hidden" name="id" value={channel.id} />
                  <button
                    type="submit"
                    className="rounded-lg bg-amber px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft"
                  >
                    Şimdi tara
                  </button>
                </AdminForm>

                <AdminForm action={toggleChannel} className="flex items-center gap-2">
                  <input type="hidden" name="id" value={channel.id} />
                  <input type="hidden" name="isActive" value={String(channel.isActive)} />
                  <button
                    type="submit"
                    className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
                  >
                    {channel.isActive ? "Durdur" : "Başlat"}
                  </button>
                </AdminForm>
              </div>
            </article>
          ))}
        </div>
      )}
    </div>
  );
}
