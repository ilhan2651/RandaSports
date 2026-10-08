const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

type ApiResult<T> = {
  success: boolean;
  data?: T;
  messages: string[];
};

export type Sport = {
  name: string;
  slug: string;
  displayOrder: number;
  storyCount: number;
  /** Branşın aktif ligi var mı: yoksa fikstür ve puan durumu sekmeleri açılmıyor. */
  hasCompetitions: boolean;
};

/**
 * Branş listesi. Artık kodda sabit değil, veritabanından geliyor — yeni branş
 * eklemek için frontend'e dokunmak gerekmiyor.
 *
 * @param onlyWithStories Menü ve site haritası için true: haberi olmayan branş
 * görünmesin. Adres doğrulaması için false: branş var ama henüz boşsa 404 değil,
 * boş sayfa göstermek istiyoruz.
 */
export async function getSports(onlyWithStories = false): Promise<Sport[]> {
  const params = new URLSearchParams();
  if (onlyWithStories) params.set("onlyWithStories", "true");

  try {
    const response = await fetch(`${API_BASE}/api/sports?${params}`, {
      next: { revalidate: 300 },
    });

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<Sport[]>;
    return result.data ?? [];
  } catch {
    // API kapalıyken menü boş kalsın; sayfa yine açılsın.
    return [];
  }
}

export async function findSport(slug: string): Promise<Sport | undefined> {
  const sports = await getSports();
  return sports.find((x) => x.slug === slug);
}
