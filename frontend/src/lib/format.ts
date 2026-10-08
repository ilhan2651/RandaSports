const CATEGORY_LABELS: Record<string, string> = {
  transfer: "Transfer",
  "mac-sonucu": "Maç Sonucu",
  sakatlik: "Sakatlık",
  aciklama: "Açıklama",
  yonetim: "Yönetim",
  diger: "Gündem",
};

export function categoryLabel(category: string | null): string {
  if (!category) return "Gündem";
  return CATEGORY_LABELS[category] ?? category;
}

/**
 * Branş adı. Liste artık API'den geldiği için çağıran taraf onu da veriyor;
 * bulunamazsa slug'ın kendisi gösteriliyor ki ekranda boşluk kalmasın.
 */
export function sportLabel(
  slug: string | null,
  sports: { slug: string; name: string }[] = [],
): string | null {
  if (!slug) return null;
  return sports.find((x) => x.slug === slug)?.name ?? slug;
}

export function timeAgo(value: string): string {
  const date = new Date(value);
  const minutes = Math.round((Date.now() - date.getTime()) / 60000);

  if (minutes < 1) return "az önce";
  if (minutes < 60) return `${minutes} dakika önce`;

  const hours = Math.round(minutes / 60);
  if (hours < 24) return `${hours} saat önce`;

  const days = Math.round(hours / 24);
  if (days === 1) return "dün";
  if (days < 7) return `${days} gün önce`;

  return date.toLocaleDateString("tr-TR", {
    day: "numeric",
    month: "long",
    year: "numeric",
  });
}

export function fullDate(value: string): string {
  return new Date(value).toLocaleString("tr-TR", {
    day: "numeric",
    month: "long",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}
