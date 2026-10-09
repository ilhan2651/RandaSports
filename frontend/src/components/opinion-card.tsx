"use client";

import { useState } from "react";
import Link from "next/link";
import { AnimatePresence, motion } from "framer-motion";
import {
  clockTime,
  stanceLabel,
  thumbnailUrl,
  youTubeEmbedUrl,
  youTubeWatchUrl,
  type Opinion,
} from "@/lib/commentary-shared";
import { timeAgo } from "@/lib/format";

const STANCE_STYLES: Record<Opinion["stance"], string> = {
  Positive: "border-amber/40 bg-amber/10 text-amber-soft",
  Negative: "border-live/40 bg-live/10 text-live",
  Neutral: "border-line bg-raised text-fog",
};

type Props = {
  opinion: Opinion;
  index?: number;
  /** Görüşün bağlandığı haberi göster — haber sayfasında gereksiz, akışta gerekli. */
  showStory?: boolean;
  /** Yorumcu adını kendi sayfasına bağla — yorumcu profilinde gereksiz. */
  linkCommentator?: boolean;
};

export function OpinionCard({
  opinion,
  index = 0,
  showStory = false,
  linkCommentator = true,
}: Props) {
  const [playing, setPlaying] = useState(false);
  const speaker = opinion.commentatorName ?? opinion.speakerLabel ?? "Kaynak belirsiz";

  return (
    <motion.article
      initial={{ opacity: 0, y: 18 }}
      whileInView={{ opacity: 1, y: 0 }}
      viewport={{ once: true, margin: "-60px" }}
      transition={{ duration: 0.45, delay: Math.min(index * 0.07, 0.35) }}
      className="group flex h-full flex-col overflow-hidden rounded-xl border border-line bg-surface transition-colors hover:border-amber/40"
    >
      <div className="relative aspect-video w-full shrink-0 overflow-hidden bg-ink">
        <AnimatePresence initial={false} mode="wait">
          {playing ? (
            <motion.div
              key="player"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              className="absolute inset-0"
            >
              {/* iframe yalnızca tıklanınca yükleniyor; liste açılışında sayfayı yavaşlatmıyor. */}
              <iframe
                src={youTubeEmbedUrl(opinion.youTubeVideoId, opinion.timestampSeconds)}
                title={opinion.videoTitle}
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowFullScreen
                className="h-full w-full"
              />
            </motion.div>
          ) : (
            <motion.button
              key="cover"
              type="button"
              onClick={() => setPlaying(true)}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.2 }}
              aria-label={`${speaker} — videoda ${clockTime(opinion.timestampSeconds)} anından dinle`}
              className="absolute inset-0 outline-none focus-visible:ring-2 focus-visible:ring-inset focus-visible:ring-amber"
            >
              {/* Kapak YouTube'dan geliyor; next/image alan adı ayarı gerekmesin diye img. */}
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={thumbnailUrl(opinion)}
                alt=""
                loading="lazy"
                className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-[1.03]"
              />

              <span className="absolute inset-0 bg-gradient-to-t from-ink via-ink/20 to-transparent" />

              <span className="absolute left-1/2 top-1/2 flex size-14 -translate-x-1/2 -translate-y-1/2 items-center justify-center rounded-full bg-amber/90 text-ink shadow-lg transition-transform duration-300 group-hover:scale-110">
                <svg viewBox="0 0 24 24" fill="currentColor" className="ml-0.5 size-6">
                  <path d="M8 5v14l11-7z" />
                </svg>
              </span>

              {/* Zaman bilinmiyorsa rozet hiç çıkmıyor: uydurma bir "0:00" göstermektense
                  hiçbir şey göstermemek doğru. */}
              {opinion.timestampSeconds !== null && (
                <span className="absolute bottom-2 right-2 rounded bg-ink/85 px-2 py-0.5 font-display text-[11px] font-bold tracking-wider text-chalk">
                  {clockTime(opinion.timestampSeconds)}
                </span>
              )}

              {opinion.teamName && (
                <span className="absolute left-2 top-2 rounded bg-ink/85 px-2 py-0.5 font-display text-[10px] font-bold uppercase tracking-widest text-amber">
                  {opinion.teamName}
                </span>
              )}
            </motion.button>
          )}
        </AnimatePresence>
      </div>

      <div className="flex flex-1 flex-col p-5">
        <header className="flex items-center gap-3">
          <Avatar name={speaker} photoUrl={opinion.commentatorPhotoUrl} />

          <div className="min-w-0 flex-1">
            {linkCommentator && opinion.commentatorSlug ? (
              <Link
                href={`/yorumcu/${opinion.commentatorSlug}`}
                className="block truncate font-display text-sm font-bold uppercase tracking-wide outline-none transition-colors hover:text-amber focus-visible:text-amber"
              >
                {speaker}
              </Link>
            ) : (
              <p className="truncate font-display text-sm font-bold uppercase tracking-wide">
                {speaker}
              </p>
            )}
            <p className="truncate text-xs text-fog">
              {opinion.channelName} · {timeAgo(opinion.videoPublishedAt)}
            </p>
          </div>

          <span
            className={`shrink-0 rounded-full border px-2.5 py-1 font-display text-[10px] font-bold uppercase tracking-widest ${STANCE_STYLES[opinion.stance]}`}
          >
            {stanceLabel(opinion.stance)}
          </span>
        </header>

        <p className="mt-4 font-display text-[11px] font-bold uppercase tracking-[0.2em] text-amber">
          {opinion.topic}
        </p>

        <blockquote className="mt-2 border-l-2 border-amber pl-4 text-[17px] leading-snug text-chalk">
          “{opinion.quote}”
        </blockquote>

        <p className="mt-3 text-sm leading-relaxed text-fog">{opinion.summary}</p>

        {opinion.prediction && (
          <p className="mt-4 rounded-lg border border-line bg-raised px-3 py-2 text-sm leading-snug text-chalk/90">
            <span className="mr-2 font-display text-[10px] font-bold uppercase tracking-widest text-amber">
              Tahmin
            </span>
            {opinion.prediction}
          </p>
        )}

        {showStory && opinion.storySlug && (
          <Link
            href={`/haber/${opinion.storySlug}`}
            className="mt-4 block border-t border-line pt-3 text-sm leading-snug text-fog outline-none transition-colors hover:text-chalk focus-visible:text-chalk"
          >
            <span className="font-display text-[10px] font-bold uppercase tracking-widest text-amber">
              İlgili haber
            </span>
            <span className="mt-1 block">{opinion.storyHeadline}</span>
          </Link>
        )}

        <div className="mt-auto flex flex-wrap items-center gap-4 pt-5 text-xs">
          {playing && (
            <button
              type="button"
              onClick={() => setPlaying(false)}
              className="font-display font-bold uppercase tracking-widest text-amber outline-none transition-colors hover:text-amber-soft"
            >
              Videoyu kapat
            </button>
          )}

          <a
            href={youTubeWatchUrl(opinion.youTubeVideoId, opinion.timestampSeconds)}
            target="_blank"
            rel="noopener noreferrer"
            className="text-fog underline-offset-4 transition-colors hover:text-chalk hover:underline"
          >
            YouTube&apos;da aç
          </a>
        </div>
      </div>
    </motion.article>
  );
}

export function Avatar({
  name,
  photoUrl,
  size = "md",
}: {
  name: string;
  photoUrl: string | null;
  size?: "md" | "lg";
}) {
  const box = size === "lg" ? "size-20 text-xl" : "size-10 text-sm";

  if (photoUrl) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img
        src={photoUrl}
        alt={name}
        className={`${box} shrink-0 rounded-full object-cover ring-1 ring-line`}
      />
    );
  }

  return (
    <span
      className={`${box} flex shrink-0 items-center justify-center rounded-full bg-amber/15 font-display font-bold text-amber ring-1 ring-amber/30`}
    >
      {initials(name)}
    </span>
  );
}

function initials(name: string): string {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toLocaleUpperCase("tr-TR") ?? "")
    .join("");
}
