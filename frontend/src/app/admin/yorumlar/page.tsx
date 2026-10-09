import type { Metadata } from "next";
import Link from "next/link";
import { approveOpinion, attachSpeaker, rejectOpinion } from "@/app/admin/actions";
import { AdminForm } from "@/components/admin-form";
import {
  clockTime,
  getOpinions,
  speakerSourceLabel,
  stanceLabel,
  youTubeWatchUrl,
  type Opinion,
} from "@/lib/commentary";
import { timeAgo } from "@/lib/format";

export const metadata: Metadata = {
  title: "Görüş onayı",
  robots: { index: false, follow: false },
};

const TABS = [
  { status: "Pending", label: "Onay bekleyen" },
  { status: "Approved", label: "Yayında" },
  { status: "Rejected", label: "Reddedilen" },
] as const;

type Params = { searchParams: Promise<{ durum?: string; ok?: string; hata?: string }> };

export default async function AdminOpinionsPage({ searchParams }: Params) {
  const { durum, ok, hata } = await searchParams;
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
          Modelin videodan çıkardığı görüşler burada bekler. Videoyu açıp alıntının
          birebir söylendiğini ve kişinin doğru olduğunu teyit etmeden onaylama —
          onay, alıntıyı doğrulanmış sayar.
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

      <p className="mt-5 text-sm text-fog">
        {opinions.total} kayıt
      </p>

      {opinions.items.length === 0 ? (
        <p className="mt-10 rounded-xl border border-line bg-surface p-8 text-center text-fog">
          Bu listede kayıt yok.
        </p>
      ) : (
        <div className="mt-5 space-y-4">
          {opinions.items.map((opinion) => (
            <ReviewCard key={opinion.id} opinion={opinion} status={status} />
          ))}
        </div>
      )}
    </div>
  );
}

function ReviewCard({ opinion, status }: { opinion: Opinion; status: string }) {
  const speaker = opinion.commentatorName ?? opinion.speakerLabel ?? "Kaynak belirsiz";
  const confidence = opinion.attributionConfidence ?? 0;

  return (
    <article className="rounded-xl border border-line bg-surface p-5">
      <header className="flex flex-wrap items-center gap-3 text-sm">
        <span className="font-display font-bold uppercase tracking-wide">{speaker}</span>

        {opinion.commentatorName === null && (
          <span className="rounded-full border border-live/40 bg-live/10 px-2 py-0.5 font-display text-[10px] font-bold uppercase tracking-widest text-live">
            sözlükte yok
          </span>
        )}

        <span className="rounded-full border border-line bg-raised px-2 py-0.5 text-xs text-fog">
          güven {Math.round(confidence * 100)}%
        </span>

        {speakerSourceLabel(opinion.speakerSource) && (
          <span
            className={`rounded-full border px-2 py-0.5 text-xs ${
              opinion.speakerSource === "altbant" || opinion.speakerSource === "baslik"
                ? "border-amber/40 bg-amber/10 text-amber-soft"
                : "border-line bg-raised text-fog"
            }`}
          >
            {speakerSourceLabel(opinion.speakerSource)}
          </span>
        )}

        <span className="rounded-full border border-line bg-raised px-2 py-0.5 text-xs text-fog">
          {stanceLabel(opinion.stance)}
        </span>

        <span className="ml-auto text-xs text-fog">{timeAgo(opinion.videoPublishedAt)}</span>
      </header>

      <p className="mt-4 font-display text-[11px] font-bold uppercase tracking-[0.2em] text-amber">
        {opinion.topic}
      </p>

      <blockquote className="mt-2 border-l-2 border-amber pl-4 text-[17px] leading-snug text-chalk">
        “{opinion.quote}”
      </blockquote>

      <p className="mt-3 text-sm leading-relaxed text-fog">{opinion.summary}</p>

      {opinion.prediction && (
        <p className="mt-3 text-sm text-chalk/90">
          <span className="mr-2 font-display text-[10px] font-bold uppercase tracking-widest text-amber">
            Tahmin
          </span>
          {opinion.prediction}
        </p>
      )}

      <dl className="mt-4 grid gap-2 border-t border-line pt-4 text-xs text-fog sm:grid-cols-2">
        <div>
          <dt className="inline font-display uppercase tracking-widest">Kanal: </dt>
          <dd className="inline">{opinion.channelName}</dd>
        </div>
        <div>
          <dt className="inline font-display uppercase tracking-widest">Video: </dt>
          <dd className="inline">{opinion.videoTitle}</dd>
        </div>
        <div>
          <dt className="inline font-display uppercase tracking-widest">Haber: </dt>
          <dd className="inline">
            {opinion.storySlug ? (
              <Link href={`/haber/${opinion.storySlug}`} className="text-amber hover:underline">
                {opinion.storyHeadline ?? opinion.storySlug}
              </Link>
            ) : (
              "bağlanmadı"
            )}
          </dd>
        </div>
        <div>
          <dt className="inline font-display uppercase tracking-widest">An: </dt>
          <dd className="inline">
            <a
              href={youTubeWatchUrl(opinion.youTubeVideoId, opinion.timestampSeconds)}
              target="_blank"
              rel="noopener noreferrer"
              className="text-amber hover:underline"
            >
              {clockTime(opinion.timestampSeconds)} — videoda dinle
            </a>
          </dd>
        </div>
      </dl>

      {opinion.commentatorName === null && (
        <AdminForm
          action={attachSpeaker}
          className="mt-4 flex flex-wrap items-center gap-2 rounded-lg border border-line bg-raised p-3"
        >
          <input type="hidden" name="id" value={opinion.id} />
          <span className="font-display text-[10px] font-bold uppercase tracking-widest text-fog">
            Konuşmacı sözlükte yok
          </span>
          <input
            type="text"
            name="fullName"
            defaultValue={opinion.speakerLabel ?? ""}
            placeholder="Yorumcunun tam adı"
            className="min-w-48 flex-1 rounded-lg border border-line bg-surface px-3 py-1.5 text-sm text-chalk outline-none placeholder:text-fog focus-visible:border-amber"
          />
          <button
            type="submit"
            className="rounded-lg border border-amber/60 px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-amber transition-colors hover:bg-amber/10"
          >
            Sözlüğe ekle
          </button>
        </AdminForm>
      )}

      {status === "Pending" && (
        <div className="mt-5 flex flex-wrap items-center gap-3">
          <AdminForm action={approveOpinion} className="flex items-center gap-2">
            <input type="hidden" name="id" value={opinion.id} />
            <button
              type="submit"
              className="rounded-lg bg-amber px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft"
            >
              Onayla
            </button>
          </AdminForm>

          <AdminForm action={rejectOpinion} className="flex flex-1 flex-wrap items-center gap-2">
            <input type="hidden" name="id" value={opinion.id} />
            <input
              type="text"
              name="note"
              placeholder="Ret sebebi (isteğe bağlı)"
              className="min-w-48 flex-1 rounded-lg border border-line bg-raised px-3 py-2 text-sm text-chalk outline-none placeholder:text-fog focus-visible:border-amber"
            />
            <button
              type="submit"
              className="rounded-lg border border-live/50 px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-live transition-colors hover:bg-live/10"
            >
              Reddet
            </button>
          </AdminForm>
        </div>
      )}
    </article>
  );
}
