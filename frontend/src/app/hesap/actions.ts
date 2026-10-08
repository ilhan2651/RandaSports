"use server";

import { revalidatePath } from "next/cache";
import { authHeader } from "@/lib/auth";

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

export type Secim = {
  sportIds: string[];
  commentatorIds: string[];
  teamIds: string[];
};

/**
 * Takip listesini kaydeder.
 *
 * <paramref name="tamamla"/> sihirbazdan geliyorsa true: hiçbir şey seçilmemiş olsa
 * bile tamamlanmış sayılıyor, "şimdilik geç" de bir cevap. Düzenleme panelinden
 * geliyorsa false — sihirbaz zaten bitmiş, oraya dokunmuyoruz.
 */
export async function savePreferences(
  secim: Secim,
  tamamla: boolean,
): Promise<{ ok: boolean; message: string }> {
  try {
    const response = await fetch(`${API_BASE}/api/me/tercihler`, {
      method: "PUT",
      headers: { ...(await authHeader()), "Content-Type": "application/json" },
      body: JSON.stringify({ ...secim, completeOnboarding: tamamla }),
      cache: "no-store",
    });

    if (response.status === 401)
      return { ok: false, message: "Oturum süresi doldu, tekrar giriş yap." };

    const payload = (await response.json().catch(() => null)) as
      | { success?: boolean; messages?: string[] }
      | null;

    if (!response.ok || payload?.success === false)
      return {
        ok: false,
        message: payload?.messages?.join(" ") || "Tercihler kaydedilemedi.",
      };

    revalidatePath("/hesap");
    revalidatePath("/akis");
    return { ok: true, message: "Kaydedildi." };
  } catch {
    return { ok: false, message: "API'ye ulaşılamadı — backend çalışıyor mu?" };
  }
}
