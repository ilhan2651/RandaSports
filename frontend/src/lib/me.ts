import { cache } from "react";
import { authHeader } from "@/lib/auth";
import type { Opinion } from "@/lib/commentary-shared";
import type { Story } from "@/lib/api";

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

export type Me = {
  id: string;
  email: string;
  fullName: string | null;
  roles: string[];
  createdAt: string;
  onboardingCompleted: boolean;
};

export type Followed = { id: string; name: string; slug: string; imageUrl: string | null };

export type Preferences = { teams: Followed[]; sports: Followed[]; commentators: Followed[] };

export type OnboardingOptions = {
  sports: { id: string; name: string; slug: string; opinionCount: number }[];
  commentators: {
    id: string;
    fullName: string;
    photoUrl: string | null;
    opinionCount: number;
    sportSlugs: string[];
  }[];
  teams: { id: string; name: string; logoUrl: string | null; sportSlug: string | null }[];
};

export type Feed = {
  hasPreferences: boolean;
  sports: Followed[];
  teams: Followed[];
  commentators: Followed[];
  stories: Story[];
  opinions: Opinion[];
};

/**
 * Giriş yapmış kullanıcının uçları. Jeton httpOnly çerezden okunuyor, bu yüzden
 * yalnızca sunucuda çalışıyor. Hata boş sonuca dönüşüyor: API kapalıyken sayfa
 * çökmek yerine "giriş yapılmamış" gibi davranıyor.
 */
async function api<T>(path: string): Promise<T | null> {
  try {
    const response = await fetch(`${API_BASE}${path}`, {
      headers: await authHeader(),
      cache: "no-store",
    });

    if (!response.ok) return null;

    const payload = (await response.json()) as { success: boolean; data?: T };
    return payload.success ? (payload.data ?? null) : null;
  } catch {
    return null;
  }
}

// cache(): aynı istek içinde hem düzen hem sayfa /api/me'yi sorabiliyor; iki tur atmasın.
export const getMe = cache(() => api<Me>("/api/me"));

export const getPreferences = cache(() => api<Preferences>("/api/me/tercihler"));

export const getOnboardingOptions = cache(() => api<OnboardingOptions>("/api/me/secenekler"));

export const getFeed = cache((storyTake = 24, opinionTake = 24) =>
  api<Feed>(`/api/me/akis?haber=${storyTake}&yorum=${opinionTake}`),
);
