import { authHeader } from "@/lib/auth";
import type {
  Channel,
  Commentator,
  Opinion,
  SportFacet,
  TeamFacet,
  UnverifiedCommentator,
} from "@/lib/commentary-shared";

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

type ApiResult<T> = {
  success: boolean;
  data?: T;
  messages: string[];
};

type Paged<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};

export * from "@/lib/commentary-shared";

const EMPTY_OPINIONS: Paged<Opinion> = { items: [], total: 0, page: 1, pageSize: 0 };

export async function getStoryOpinions(storyId: string): Promise<Opinion[]> {
  try {
    const response = await fetch(
      `${API_BASE}/api/commentary/opinions/story/${encodeURIComponent(storyId)}`,
      { next: { revalidate: 60 } },
    );

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<Opinion[]>;
    return result.data ?? [];
  } catch {
    return [];
  }
}

type OpinionFeedQuery = {
  commentator?: string;
  team?: string;
  sport?: string;
  days?: number;
  page?: number;
  pageSize?: number;
};

/** Siteye açık görüş akışı: anasayfa bölümü, yorumcu sayfaları ve filtreler. */
export async function getLatestOpinions(
  query: OpinionFeedQuery = {},
): Promise<Paged<Opinion>> {
  const params = new URLSearchParams();
  if (query.commentator) params.set("commentator", query.commentator);
  if (query.team) params.set("team", query.team);
  if (query.sport) params.set("sport", query.sport);
  if (query.days) params.set("days", String(query.days));
  params.set("page", String(query.page ?? 1));
  params.set("pageSize", String(query.pageSize ?? 12));

  try {
    const response = await fetch(`${API_BASE}/api/commentary/opinions/latest?${params}`, {
      next: { revalidate: 60 },
    });

    if (!response.ok) return EMPTY_OPINIONS;

    const result = (await response.json()) as ApiResult<Paged<Opinion>>;
    return result.data ?? EMPTY_OPINIONS;
  } catch {
    return EMPTY_OPINIONS;
  }
}

/** Filtre çubuğu için: hakkında görüş bulunan takımlar. */
export async function getOpinionTeams(): Promise<TeamFacet[]> {
  try {
    const response = await fetch(`${API_BASE}/api/commentary/teams`, {
      next: { revalidate: 300 },
    });

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<TeamFacet[]>;
    return result.data ?? [];
  } catch {
    return [];
  }
}

/** Filtre çubuğu için: hakkında görüş bulunan branşlar. */
export async function getOpinionSports(): Promise<SportFacet[]> {
  try {
    const response = await fetch(`${API_BASE}/api/commentary/sports`, {
      next: { revalidate: 300 },
    });

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<SportFacet[]>;
    return result.data ?? [];
  } catch {
    return [];
  }
}

/** Görüşü yayına çıkmış yorumcular, görüş sayılarıyla. */
export async function getCommentators(): Promise<Commentator[]> {
  try {
    const response = await fetch(`${API_BASE}/api/commentary/commentators`, {
      next: { revalidate: 300 },
    });

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<Commentator[]>;
    return result.data ?? [];
  } catch {
    return [];
  }
}

export async function getCommentator(slug: string): Promise<Commentator | null> {
  try {
    const response = await fetch(
      `${API_BASE}/api/commentary/commentators/${encodeURIComponent(slug)}`,
      { next: { revalidate: 300 } },
    );

    if (!response.ok) return null;

    const result = (await response.json()) as ApiResult<Commentator>;
    return result.data ?? null;
  } catch {
    return null;
  }
}

/** Yönetim listesi: onay bekleyenler anlık görünmeli, önbelleğe alınmıyor. */
export async function getOpinions(
  status: string | null = "Pending",
  page = 1,
  pageSize = 20,
): Promise<Paged<Opinion>> {
  const params = new URLSearchParams();
  if (status) params.set("status", status);
  params.set("page", String(page));
  params.set("pageSize", String(pageSize));

  try {
    // Yönetim ucu: jeton gerekiyor.
    const response = await fetch(`${API_BASE}/api/commentary/opinions?${params}`, {
      headers: await authHeader(),
      cache: "no-store",
    });

    if (!response.ok) return EMPTY_OPINIONS;

    const result = (await response.json()) as ApiResult<Paged<Opinion>>;
    return result.data ?? EMPTY_OPINIONS;
  } catch {
    return EMPTY_OPINIONS;
  }
}

export async function getChannels(): Promise<Channel[]> {
  try {
    const response = await fetch(`${API_BASE}/api/commentary/channels`, {
      headers: await authHeader(),
      cache: "no-store",
    });

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<Channel[]>;
    return result.data ?? [];
  } catch {
    return [];
  }
}

/** Onay ekranı için: her zaman taze, önbelleksiz. */
export async function getUnverifiedCommentators(): Promise<UnverifiedCommentator[]> {
  try {
    const response = await fetch(`${API_BASE}/api/commentary/commentators/unverified`, {
      headers: await authHeader(),
      cache: "no-store",
    });

    if (!response.ok) return [];

    const result = (await response.json()) as ApiResult<UnverifiedCommentator[]>;
    return result.data ?? [];
  } catch {
    return [];
  }
}
