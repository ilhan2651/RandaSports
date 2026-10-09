/**
 * Görüş verisinin tipleri ve saf yardımcıları.
 *
 * Ayrı dosyada duruyorlar çünkü bunları istemci bileşenleri de kullanıyor.
 * commentary.ts sunucuya ait: oturum çerezini okumak için next/headers'a bağlı,
 * ve o zincir istemci paketine girdiğinde derleme kırılıyor.
 */

export type Opinion = {
  id: string;
  commentatorName: string | null;
  commentatorSlug: string | null;
  commentatorPhotoUrl: string | null;
  speakerLabel: string | null;
  speakerSource: string | null;
  attributionConfidence: number | null;
  topic: string;
  summary: string;
  quote: string;
  timestampSeconds: number | null;
  stance: "Positive" | "Negative" | "Neutral";
  prediction: string | null;
  status: "Pending" | "Approved" | "Rejected";
  isQuoteVerified: boolean;
  youTubeVideoId: string;
  videoTitle: string;
  videoThumbnailUrl: string | null;
  channelName: string;
  videoPublishedAt: string;
  teamName: string | null;
  teamSlug: string | null;
  sportSlug: string | null;
  storyId: string | null;
  storySlug: string | null;
  storyHeadline: string | null;

  /** Makinenin ön kontrolü. İnsanın onayının yerine geçmiyor, onu hazırlıyor. */
  quoteCheck: QuoteCheck;
  /** Modelin o pencerede gerçekten duyduğu cümle. */
  quoteCheckHeard: string | null;
  quoteCheckSpeaker: string | null;
  /** Modelin ikinci bakışta verdiği an; alıntınınkiyle farkı kaymayı gösteriyor. */
  quoteCheckTimestampSeconds: number | null;
};

export type QuoteCheck =
  | "NotChecked"
  | "Verbatim"
  | "Paraphrased"
  | "Different"
  | "WrongSpeaker"
  | "NotHeard"
  | "Failed";

type QuoteCheckInfo = {
  label: string;
  /** Onaylamaya engel mi — ekran buna göre renkleniyor. */
  tone: "iyi" | "uyari" | "kotu" | "notr";
  hint: string;
};

export const QUOTE_CHECK_INFO: Record<QuoteCheck, QuoteCheckInfo> = {
  NotChecked: { label: "kontrol edilmedi", tone: "notr", hint: "Henüz bakılmadı — videoyu kendin aç." },
  Verbatim: { label: "birebir", tone: "iyi", hint: "Model de aynı cümleyi duydu." },
  Paraphrased: {
    label: "kelimeler farklı",
    tone: "uyari",
    hint: "Anlam aynı ama kelimeler tutmuyor. Alıntı birebir olmalı — duyulan hali geçir ya da reddet.",
  },
  Different: { label: "farklı", tone: "kotu", hint: "Bu anda söylenen şey bu değil." },
  WrongSpeaker: { label: "konuşan başkası", tone: "kotu", hint: "Alıntı doğru ama kişi yanlış." },
  NotHeard: { label: "duyulmadı", tone: "kotu", hint: "Bu pencerede böyle bir söz geçmiyor; damga kaymış olabilir." },
  Failed: { label: "kontrol edilemedi", tone: "notr", hint: "Modele ulaşılamadı; videoyu kendin aç." },
};

export type SportFacet = {
  name: string;
  slug: string;
  opinionCount: number;
};

export type TeamFacet = {
  name: string;
  slug: string;
  sportSlug: string | null;
  logoUrl: string | null;
  opinionCount: number;
};

export type Commentator = {
  id: string;
  fullName: string;
  slug: string;
  photoUrl: string | null;
  bio: string | null;
  isActive: boolean;
  opinionCount: number;
  lastOpinionAt: string | null;
  personRole: PersonRole;
};

/** Sözlükteki kişinin ne olduğu; yorumcu olmayanlar yorumcu filtresinde listelenmiyor. */
export type PersonRole = "Unknown" | "Commentator" | "Athlete" | "Coach" | "Official";

/** Videodan otomatik eklenmiş, onay bekleyen konuşmacı. */
export type UnverifiedCommentator = {
  id: string;
  fullName: string;
  slug: string;
  photoUrl: string | null;
  opinionCount: number;
  approvedCount: number;
  lastOpinionAt: string | null;
  channels: string[];
  sample: OpinionSample[];
};

export type OpinionSample = {
  topic: string;
  quote: string;
  videoTitle: string;
  youTubeVideoId: string;
  timestampSeconds: number | null;
};

export type Channel = {
  id: string;
  name: string;
  slug: string;
  handle: string;
  youTubeChannelId: string | null;
  isActive: boolean;
  lastCheckedAt: string | null;
  lastError: string | null;
  commentatorCount: number;
  sportSlugs: string[];
  sportNames: string[];
};

const SPEAKER_SOURCE_LABELS: Record<string, string> = {
  altbant: "alt banttan okundu",
  baslik: "başlıktan alındı",
  hitap: "konuşmada geçti",
  aciklama: "açıklamadan seçildi",
  tahmin: "tahmin",
};

/** İsmin nereden geldiği; onay verirken ne kadar güveneceğini bu belirliyor. */
export function speakerSourceLabel(source: string | null): string | null {
  if (!source) return null;
  return SPEAKER_SOURCE_LABELS[source] ?? source;
}

export function stanceLabel(stance: Opinion["stance"]): string {
  if (stance === "Positive") return "Olumlu";
  if (stance === "Negative") return "Olumsuz";
  return "Nötr";
}

/**
 * Kapak görseli. Beslemeden gelen adres yoksa YouTube'un sabit kapak adresine
 * düşüyoruz — video kimliğinden türediği için her videoda çalışır.
 */
export function thumbnailUrl(opinion: Opinion): string {
  return (
    opinion.videoThumbnailUrl ??
    `https://i.ytimg.com/vi/${opinion.youTubeVideoId}/hqdefault.jpg`
  );
}

/** Görüşleri yayın gününe göre öbekler; akışa zaman ekseni veriyor. */
export function groupByDay(opinions: Opinion[]): { label: string; items: Opinion[] }[] {
  const groups = new Map<string, { label: string; items: Opinion[] }>();

  for (const opinion of opinions) {
    const date = new Date(opinion.videoPublishedAt);
    const key = date.toLocaleDateString("tr-TR");

    if (!groups.has(key)) groups.set(key, { label: dayLabel(date), items: [] });
    groups.get(key)!.items.push(opinion);
  }

  return [...groups.values()];
}

function dayLabel(date: Date): string {
  const today = new Date();
  const days = Math.round(
    (new Date(today.toDateString()).getTime() - new Date(date.toDateString()).getTime()) / 86400000,
  );

  if (days <= 0) return "Bugün";
  if (days === 1) return "Dün";

  return date.toLocaleDateString("tr-TR", {
    day: "numeric",
    month: "long",
    ...(date.getFullYear() === today.getFullYear() ? {} : { year: "numeric" }),
  });
}

export function youTubeWatchUrl(videoId: string, seconds: number | null): string {
  const base = `https://www.youtube.com/watch?v=${videoId}`;
  return seconds !== null && seconds > 0 ? `${base}&t=${seconds}s` : base;
}

export function youTubeEmbedUrl(videoId: string, seconds: number | null): string {
  const params = new URLSearchParams({ autoplay: "1", rel: "0" });
  if (seconds !== null && seconds > 0) params.set("start", String(seconds));
  return `https://www.youtube.com/embed/${videoId}?${params}`;
}

/**
 * Zaman damgası yazısı. null, modelin anı bilemediği anlamına geliyor — o durumda
 * uydurma bir "0:00" göstermek yerine zamanın bilinmediğini söylüyoruz.
 */
export function clockTime(seconds: number | null): string {
  if (seconds === null) return "an belirsiz";
  if (seconds <= 0) return "başından";

  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const rest = seconds % 60;
  const pad = (value: number) => String(value).padStart(2, "0");

  return hours > 0
    ? `${hours}:${pad(minutes)}:${pad(rest)}`
    : `${minutes}:${pad(rest)}`;
}
