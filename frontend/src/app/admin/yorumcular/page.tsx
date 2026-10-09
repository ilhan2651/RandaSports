import type { Metadata } from "next";
import {
  rejectCommentator,
  updateCommentatorPhoto,
  verifyCommentator,
} from "@/app/admin/actions";
import { AdminForm } from "@/components/admin-form";
import { youTubeWatchUrl } from "@/lib/commentary-shared";
import { EntityAvatar } from "@/components/entity-avatar";
import {
  getCommentators,
  getUnverifiedCommentators,
  type Commentator,
  type UnverifiedCommentator,
} from "@/lib/commentary";
import { timeAgo } from "@/lib/format";

export const metadata: Metadata = {
  title: "Yorumcular",
  robots: { index: false, follow: false },
};

type Params = { searchParams: Promise<{ ok?: string; hata?: string }> };

export default async function AdminCommentatorsPage({ searchParams }: Params) {
  const { ok, hata } = await searchParams;

  const [people, all] = await Promise.all([
    getUnverifiedCommentators(),
    getCommentators(),
  ]);

  const gorselsiz = all.filter((x) => !x.photoUrl).length;

  return (
    <div className="pt-8">
      <header className="border-b border-line pb-5">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Yönetim
        </p>
        <h1 className="mt-2 font-display text-3xl font-extrabold uppercase tracking-tight">
          Yorumcular
        </h1>
        <p className="mt-3 max-w-[70ch] text-sm leading-relaxed text-fog">
          Üstte videodan otomatik eklenmiş, kişi onayı bekleyen isimler var. Altta ise
          yayındaki tüm yorumcular ve görselleri. Görseli yalnızca buradan, elle
          giriyorsun — sistem artık videodan portre türetmiyor.
        </p>
      </header>


      <section className="mt-10">
        <h2 className="font-display text-xs font-bold uppercase tracking-widest text-fog">
          Kişi onayı
          {people.length > 0 && <span className="text-amber"> · {people.length} bekliyor</span>}
        </h2>

        {people.length === 0 ? (
          <p className="mt-4 rounded-xl border border-dashed border-line bg-surface px-6 py-10 text-center text-sm text-fog">
            Onay bekleyen yorumcu yok.
          </p>
        ) : (
          <div className="mt-4 space-y-4">
            {people.map((person) => (
              <PersonCard key={person.id} person={person} />
            ))}
          </div>
        )}
      </section>

      <section className="mt-12 border-t border-line pt-8">
        <h2 className="font-display text-xs font-bold uppercase tracking-widest text-fog">
          Görseller
          <span className="text-amber">
            {" · "}
            {all.length} yorumcu
            {gorselsiz > 0 && `, ${gorselsiz} tanesinde görsel yok`}
          </span>
        </h2>
        <p className="mt-2 max-w-[70ch] text-sm leading-relaxed text-fog">
          Adresi boş bırakıp kaydedersen görsel silinir ve baş harflere döner. Açık
          lisanslı bir portre (Wikipedia/Wikimedia) ya da kişinin kendi paylaştığı
          profil görseli en güvenlisi.
        </p>

        <div className="mt-5 space-y-2">
          {all.map((person) => (
            <PhotoRow key={person.id} person={person} />
          ))}
        </div>
      </section>
    </div>
  );
}

function PhotoRow({ person }: { person: Commentator }) {
  return (
    <AdminForm
      action={updateCommentatorPhoto}
      className="flex flex-wrap items-center gap-3 rounded-xl border border-line bg-surface px-4 py-3"
      quiet
    >
      <input type="hidden" name="id" value={person.id} />

      <EntityAvatar name={person.fullName} src={person.photoUrl} size={44} />

      <div className="min-w-[12rem] flex-1">
        <p className="font-display text-sm font-bold uppercase tracking-tight text-chalk">
          {person.fullName}
        </p>
        <p className="text-xs text-fog">
          {person.opinionCount} görüş
          {person.lastOpinionAt && ` · ${timeAgo(person.lastOpinionAt)}`}
        </p>
      </div>

      <input
        type="url"
        name="photoUrl"
        defaultValue={person.photoUrl ?? ""}
        placeholder="https://… portre adresi"
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

function PersonCard({ person }: { person: UnverifiedCommentator }) {
  return (
    <article className="rounded-xl border border-line bg-surface p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="flex min-w-0 items-center gap-3">
          <EntityAvatar name={person.fullName} src={person.photoUrl} size={44} />

          <div className="min-w-0">
            <h3 className="font-display text-xl font-bold uppercase tracking-tight text-chalk">
              {person.fullName}
            </h3>
            <p className="mt-1 text-xs text-fog">
              {person.opinionCount} görüş
              {person.approvedCount > 0 && (
                <span className="text-amber"> · {person.approvedCount} tanesi yayında</span>
              )}
              {person.channels.length > 0 && ` · ${person.channels.join(", ")}`}
              {person.lastOpinionAt && ` · ${timeAgo(person.lastOpinionAt)}`}
            </p>
          </div>
        </div>

        <div className="flex shrink-0 gap-2">
          <AdminForm action={verifyCommentator} className="flex items-center gap-2">
            <input type="hidden" name="id" value={person.id} />
            <button
              type="submit"
              className="rounded-lg bg-amber px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft"
            >
              Doğrula
            </button>
          </AdminForm>

          <AdminForm action={rejectCommentator} className="flex items-center gap-2">
            <input type="hidden" name="id" value={person.id} />
            <button
              type="submit"
              className="rounded-lg border border-live px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-live transition-colors hover:bg-live/10"
            >
              Reddet
            </button>
          </AdminForm>
        </div>
      </div>

      {person.sample.length > 0 && (
        <div className="mt-4 space-y-3 border-t border-line pt-4">
          {person.sample.map((item, index) => (
            <div key={index} className="border-l-2 border-line pl-3">
              <p className="font-display text-[11px] font-bold uppercase tracking-widest text-amber">
                {item.topic}
              </p>
              <p className="mt-1 text-sm leading-relaxed text-chalk/90">“{item.quote}”</p>
              <a
                href={youTubeWatchUrl(item.youTubeVideoId, item.timestampSeconds)}
                target="_blank"
                rel="noopener noreferrer"
                className="mt-1 inline-block text-xs text-fog underline-offset-2 transition-colors hover:text-amber hover:underline"
              >
                {item.videoTitle} — videoda dinle
              </a>
            </div>
          ))}
        </div>
      )}
    </article>
  );
}
