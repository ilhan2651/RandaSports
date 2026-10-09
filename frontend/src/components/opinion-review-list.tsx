"use client";

import { useCallback, useEffect, useRef, useState, useTransition } from "react";
import Link from "next/link";
import { attachSpeaker, bulkReviewOpinions, reviewOpinion } from "@/app/admin/actions";
import { AdminForm } from "@/components/admin-form";
import {
  QUOTE_CHECK_INFO,
  clockTime,
  speakerSourceLabel,
  stanceLabel,
  youTubeEmbedUrl,
  youTubeWatchUrl,
  type Opinion,
} from "@/lib/commentary-shared";
import { timeAgo } from "@/lib/format";

type Props = {
  opinions: Opinion[];
  pending: boolean;
};

const TON = {
  iyi: "border-amber/50 bg-amber/10 text-amber-soft",
  uyari: "border-amber/30 bg-raised text-amber-soft",
  kotu: "border-live/50 bg-live/10 text-live",
  notr: "border-line bg-raised text-fog",
} as const;

/**
 * Onay ekranının listesi.
 *
 * Elle onay bu projenin en yavaş işi: her kayıt için videoyu açıp o dakikayı
 * dinlemek gerekiyordu. Burada üç şey o süreyi kısaltıyor — makinenin ön
 * kontrolü kartın üstünde hazır duruyor, video sayfadan çıkmadan açılıyor,
 * ve gezinme klavyeden yapılıyor.
 *
 * Ön kontrol bir ÖNERİ. Onaylama tuşu hâlâ insanın; "birebir" damgası yeşil
 * diye körlemesine basmak için değil, hangi kayda bakmayacağını bilmek için var.
 */
export function OpinionReviewList({ opinions, pending }: Props) {
  const [aktif, setAktif] = useState(0);
  const [secili, setSecili] = useState<Set<string>>(new Set());
  const [acikVideo, setAcikVideo] = useState<string | null>(null);
  const [mesaj, setMesaj] = useState<string | null>(null);
  const [bekliyor, basla] = useTransition();
  const kartlar = useRef<(HTMLElement | null)[]>([]);

  // Liste sunucudan tazelendiğinde (onaylanan kayıt listeden düşüyor) imleç
  // listenin dışında kalabiliyor.
  useEffect(() => {
    setAktif((x) => Math.min(x, Math.max(0, opinions.length - 1)));
    setSecili((x) => {
      const kalan = new Set(opinions.map((o) => o.id));
      const yeni = new Set([...x].filter((id) => kalan.has(id)));
      return yeni.size === x.size ? x : yeni;
    });
  }, [opinions]);

  const karar = useCallback(
    (opinion: Opinion, approve: boolean, duzeltme?: { quote?: string; timestampSeconds?: number }) => {
      basla(async () => {
        const sonuc = await reviewOpinion({
          id: opinion.id,
          approve,
          quote: duzeltme?.quote ?? null,
          timestampSeconds: duzeltme?.timestampSeconds ?? null,
        });
        setMesaj(sonuc.message);
      });
    },
    [],
  );

  const topluKarar = useCallback(
    (approve: boolean) => {
      const ids = [...secili];
      basla(async () => {
        const sonuc = await bulkReviewOpinions({ ids, approve });
        setMesaj(sonuc.message);
        setSecili(new Set());
      });
    },
    [secili],
  );

  const sec = useCallback((id: string) => {
    setSecili((x) => {
      const yeni = new Set(x);
      if (!yeni.delete(id)) yeni.add(id);
      return yeni;
    });
  }, []);

  useEffect(() => {
    if (!pending) return;

    function tus(event: KeyboardEvent) {
      // Bir alana yazıyorsa kısayol çalışmamalı: ret sebebi yazarken "r" tuşu
      // kaydı reddederse ekran kullanılamaz hale gelir.
      const hedef = event.target as HTMLElement | null;
      if (hedef?.closest("input, textarea, select, [contenteditable=true]")) return;
      if (event.metaKey || event.ctrlKey || event.altKey) return;

      const opinion = opinions[aktif];

      switch (event.key.toLowerCase()) {
        case "j":
        case "arrowdown":
          event.preventDefault();
          setAktif((x) => Math.min(x + 1, opinions.length - 1));
          break;
        case "k":
        case "arrowup":
          event.preventDefault();
          setAktif((x) => Math.max(x - 1, 0));
          break;
        case "a":
          if (opinion) { event.preventDefault(); karar(opinion, true); }
          break;
        case "r":
          if (opinion) { event.preventDefault(); karar(opinion, false); }
          break;
        case "x":
          if (opinion) { event.preventDefault(); sec(opinion.id); }
          break;
        case "v":
          if (opinion) {
            event.preventDefault();
            setAcikVideo((x) => (x === opinion.id ? null : opinion.id));
          }
          break;
        case "escape":
          setAcikVideo(null);
          break;
      }
    }

    window.addEventListener("keydown", tus);
    return () => window.removeEventListener("keydown", tus);
  }, [aktif, opinions, pending, karar, sec]);

  // İmleç klavyeyle taşındığında kart görünür alana gelsin.
  useEffect(() => {
    kartlar.current[aktif]?.scrollIntoView({ block: "nearest", behavior: "smooth" });
  }, [aktif]);

  const temizOlanlar = opinions.filter((x) => x.quoteCheck === "Verbatim").map((x) => x.id);

  return (
    <>
      {pending && (
        <div className="sticky top-0 z-20 -mx-1 mb-4 flex flex-wrap items-center gap-3 border-b border-line bg-ink/95 px-1 py-3 backdrop-blur">
          <span className="font-display text-[10px] font-bold uppercase tracking-widest text-fog">
            {secili.size > 0 ? `${secili.size} seçili` : "Seçim yok"}
          </span>

          {temizOlanlar.length > 0 && (
            <button
              type="button"
              onClick={() => setSecili(new Set(temizOlanlar))}
              className="rounded-lg border border-line px-3 py-1.5 font-display text-[10px] font-bold uppercase tracking-widest text-fog transition-colors hover:border-amber/40 hover:text-chalk"
            >
              Birebir olanları seç ({temizOlanlar.length})
            </button>
          )}

          {secili.size > 0 && (
            <>
              <button
                type="button"
                disabled={bekliyor}
                onClick={() => topluKarar(true)}
                className="rounded-lg bg-amber px-3 py-1.5 font-display text-[10px] font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft disabled:opacity-50"
              >
                Seçilenleri onayla
              </button>
              <button
                type="button"
                disabled={bekliyor}
                onClick={() => topluKarar(false)}
                className="rounded-lg border border-live/50 px-3 py-1.5 font-display text-[10px] font-bold uppercase tracking-widest text-live transition-colors hover:bg-live/10 disabled:opacity-50"
              >
                Reddet
              </button>
              <button
                type="button"
                onClick={() => setSecili(new Set())}
                className="font-display text-[10px] font-bold uppercase tracking-widest text-fog hover:text-chalk"
              >
                Bırak
              </button>
            </>
          )}

          <span className="ml-auto hidden text-[11px] text-fog sm:block">
            <kbd className="rounded border border-line px-1">J</kbd>
            <kbd className="ml-1 rounded border border-line px-1">K</kbd> gez ·{" "}
            <kbd className="rounded border border-line px-1">V</kbd> video ·{" "}
            <kbd className="rounded border border-line px-1">X</kbd> seç ·{" "}
            <kbd className="rounded border border-line px-1">A</kbd> onayla ·{" "}
            <kbd className="rounded border border-line px-1">R</kbd> reddet
          </span>

          {(bekliyor || mesaj) && (
            <span
              role="status"
              className="w-full font-display text-[10px] font-bold uppercase tracking-widest text-amber"
            >
              {bekliyor ? "…" : mesaj}
            </span>
          )}
        </div>
      )}

      <div className="space-y-4">
        {opinions.map((opinion, index) => (
          <ReviewCard
            key={opinion.id}
            ref={(el) => { kartlar.current[index] = el; }}
            opinion={opinion}
            pending={pending}
            isActive={pending && index === aktif}
            isSelected={secili.has(opinion.id)}
            videoOpen={acikVideo === opinion.id}
            busy={bekliyor}
            onFocus={() => setAktif(index)}
            onToggleSelect={() => sec(opinion.id)}
            onToggleVideo={() =>
              setAcikVideo((x) => (x === opinion.id ? null : opinion.id))
            }
            onDecide={(approve, duzeltme) => karar(opinion, approve, duzeltme)}
          />
        ))}
      </div>
    </>
  );
}

type CardProps = {
  opinion: Opinion;
  pending: boolean;
  isActive: boolean;
  isSelected: boolean;
  videoOpen: boolean;
  busy: boolean;
  onFocus: () => void;
  onToggleSelect: () => void;
  onToggleVideo: () => void;
  onDecide: (approve: boolean, duzeltme?: { quote?: string; timestampSeconds?: number }) => void;
  ref?: React.Ref<HTMLElement>;
};

function ReviewCard({
  opinion,
  pending,
  isActive,
  isSelected,
  videoOpen,
  busy,
  onFocus,
  onToggleSelect,
  onToggleVideo,
  onDecide,
  ref,
}: CardProps) {
  const speaker = opinion.commentatorName ?? opinion.speakerLabel ?? "Kaynak belirsiz";
  const confidence = opinion.attributionConfidence ?? 0;
  const kontrol = QUOTE_CHECK_INFO[opinion.quoteCheck] ?? QUOTE_CHECK_INFO.NotChecked;

  // Videoyu hangi andan açacağız: ön kontrol daha isabetli bir an verdiyse onu
  // kullanıyoruz, çünkü dar bir pencereye bakarak bulmuş.
  const an = opinion.quoteCheckTimestampSeconds ?? opinion.timestampSeconds;

  const sapma =
    opinion.quoteCheckTimestampSeconds !== null && opinion.timestampSeconds !== null
      ? opinion.quoteCheckTimestampSeconds - opinion.timestampSeconds
      : null;

  // Modelin duyduğu hal alıntıdan farklıysa düzeltme önerilebilir.
  const duyulan = opinion.quoteCheckHeard?.trim();
  const farkliDuyulmus = !!duyulan && duyulan !== opinion.quote.trim();

  return (
    <article
      ref={ref}
      onClick={pending ? onFocus : undefined}
      className={`scroll-mt-24 rounded-xl border bg-surface p-5 transition-colors ${
        isActive ? "border-amber" : isSelected ? "border-amber/40" : "border-line"
      }`}
    >
      <header className="flex flex-wrap items-center gap-3 text-sm">
        {pending && (
          <input
            type="checkbox"
            checked={isSelected}
            onChange={onToggleSelect}
            aria-label="Toplu işlem için seç"
            className="size-4 accent-amber"
          />
        )}

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

      {pending && (
        <div className={`mt-4 rounded-lg border p-3 ${TON[kontrol.tone]}`}>
          <p className="font-display text-[10px] font-bold uppercase tracking-widest">
            Ön kontrol: {kontrol.label}
            {sapma !== null && sapma !== 0 && (
              <span className="ml-2 font-normal normal-case tracking-normal opacity-80">
                (damga {sapma > 0 ? "+" : ""}{sapma} sn kaymış)
              </span>
            )}
          </p>

          <p className="mt-1 text-xs leading-relaxed opacity-90">{kontrol.hint}</p>

          {opinion.quoteCheckSpeaker && (
            <p className="mt-2 text-xs opacity-90">
              <span className="font-display uppercase tracking-widest">Duyulan kişi: </span>
              {opinion.quoteCheckSpeaker}
            </p>
          )}

          {farkliDuyulmus && (
            <div className="mt-2 border-t border-current/20 pt-2">
              <p className="text-xs leading-relaxed opacity-90">
                <span className="font-display uppercase tracking-widest">Duyulan: </span>
                “{duyulan}”
              </p>
              <button
                type="button"
                disabled={busy}
                onClick={() =>
                  onDecide(true, {
                    quote: duyulan,
                    ...(opinion.quoteCheckTimestampSeconds !== null
                      ? { timestampSeconds: opinion.quoteCheckTimestampSeconds }
                      : {}),
                  })
                }
                className="mt-2 rounded-lg border border-current/50 px-3 py-1.5 font-display text-[10px] font-bold uppercase tracking-widest transition-opacity hover:opacity-80 disabled:opacity-50"
              >
                Duyulanı alıntıya geçir ve onayla
              </button>
            </div>
          )}
        </div>
      )}

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
            {an === null ? (
              "an belirsiz"
            ) : (
              <>
                <button
                  type="button"
                  onClick={onToggleVideo}
                  className="text-amber hover:underline"
                >
                  {clockTime(an)} — {videoOpen ? "kapat" : "burada dinle"}
                </button>
                <a
                  href={youTubeWatchUrl(opinion.youTubeVideoId, an)}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="ml-2 text-fog hover:text-chalk hover:underline"
                >
                  YouTube'da aç
                </a>
              </>
            )}
          </dd>
        </div>
      </dl>

      {/* Gömülü oynatıcı yalnızca açıkken basılıyor: elli kartın elli iframe'i
          sayfayı dizlerinin üstüne çökertir. */}
      {videoOpen && an !== null && (
        <div className="mt-4 aspect-video overflow-hidden rounded-lg border border-line">
          <iframe
            src={youTubeEmbedUrl(opinion.youTubeVideoId, Math.max(0, an - 3))}
            title={opinion.videoTitle}
            allow="accelerometer; autoplay; encrypted-media; picture-in-picture"
            allowFullScreen
            className="size-full"
          />
        </div>
      )}

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
            defaultValue={opinion.quoteCheckSpeaker ?? opinion.speakerLabel ?? ""}
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

      {pending && (
        <div className="mt-5 flex flex-wrap items-center gap-3">
          <button
            type="button"
            disabled={busy}
            onClick={() => onDecide(true)}
            className="rounded-lg bg-amber px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft disabled:opacity-50"
          >
            Onayla
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={() => onDecide(false)}
            className="rounded-lg border border-live/50 px-4 py-2 font-display text-xs font-bold uppercase tracking-widest text-live transition-colors hover:bg-live/10 disabled:opacity-50"
          >
            Reddet
          </button>
        </div>
      )}
    </article>
  );
}
