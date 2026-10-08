const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

/** Backend'in Result<T> sarmalayıcısı. */
type ApiResult<T> = {
  success: boolean;
  data?: T;
  messages: string[];
};

export type Paged<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
};

export type Story = {
  id: string;
  slug: string | null;
  headline: string | null;
  summary: string | null;
  imageUrl: string | null;
  category: string | null;
  sportSlug: string | null;
  isLive: boolean;
  sourceCount: number;
  publishedAt: string;
  updatedAt: string;
};

export type StorySource = {
  sourceName: string;
  title: string;
  url: string;
  publishedAt: string;
};

export type StoryDetail = Story & {
  body: string | null;
  analysis: string | null;
  sources: StorySource[];
};

const EMPTY_PAGE: Paged<Story> = { items: [], total: 0, page: 1, pageSize: 0 };

type StoryQuery = {
  page?: number;
  pageSize?: number;
  sport?: string;
  search?: string;
};

/** API kapalıyken sayfa çökmesin diye hatalar boş sonuca dönüşüyor. */
export async function getStories(query: StoryQuery = {}): Promise<Paged<Story>> {
  const params = new URLSearchParams();
  params.set("page", String(query.page ?? 1));
  params.set("pageSize", String(query.pageSize ?? 20));
  if (query.sport) params.set("sport", query.sport);
  if (query.search) params.set("search", query.search);

  try {
    const response = await fetch(`${API_BASE}/api/stories?${params}`, {
      next: { revalidate: 60 },
    });

    if (!response.ok) return EMPTY_PAGE;

    const result = (await response.json()) as ApiResult<Paged<Story>>;
    return result.data ?? EMPTY_PAGE;
  } catch {
    return EMPTY_PAGE;
  }
}

export async function getStory(slug: string): Promise<StoryDetail | null> {
  try {
    const response = await fetch(`${API_BASE}/api/stories/${encodeURIComponent(slug)}`, {
      next: { revalidate: 60 },
    });

    if (!response.ok) return null;

    const result = (await response.json()) as ApiResult<StoryDetail>;
    return result.data ?? null;
  } catch {
    return null;
  }
}
