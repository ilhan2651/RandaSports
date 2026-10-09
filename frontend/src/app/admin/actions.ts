"use server";

import { revalidatePath } from "next/cache";
import { redirect } from "next/navigation";
import { authHeader, refreshSession, signOut } from "@/lib/auth";

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

export type ActionResult = { ok: boolean; message: string };

async function send(
  path: string,
  method: "POST" | "PATCH",
  body?: unknown,
): Promise<ActionResult> {
  // Oturum jetonu yalnızca sunucuda; tarayıcıya hiç inmiyor.
  const gonder = async () => {
    const headers: Record<string, string> = { ...(await authHeader()) };

    if (body) headers["Content-Type"] = "application/json";

    return fetch(`${API_BASE}${path}`, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
      cache: "no-store",
    });
  };

  // redirect() bir istisna fırlatarak çalışıyor; try içinde çağrılırsa catch onu
  // yutup "API'ye ulaşılamadı" der. Bu yüzden ağ işi try içinde, yönlendirme dışında.
  let yetkisiz = false;

  const sonuc = await (async (): Promise<ActionResult> => {
    try {
      // Erişim jetonu kısa ömürlü. 401 alırsak bir kez yenilemeyi deniyoruz;
      // yenileme jetonu tutarsa kullanıcı hiçbir şey fark etmiyor.
      let response = await gonder();

      if (response.status === 401 && (await refreshSession())) response = await gonder();

      if (response.status === 401) {
        yetkisiz = true;
        return { ok: false, message: "Oturum süresi doldu." };
      }

      const payload = (await response.json().catch(() => null)) as
        | { success?: boolean; messages?: string[] }
        | null;

      const message = payload?.messages?.join(" ") ?? "";

      if (!response.ok || payload?.success === false)
        return { ok: false, message: message || `Sunucu ${response.status} döndü.` };

      return { ok: true, message };
    } catch {
      // En sık sebep: API ayakta değil. Sessiz kalırsak ekran bozuk görünüyor.
      return { ok: false, message: "API'ye ulaşılamadı — backend çalışıyor mu?" };
    }
  })();

  // Jeton düşmüş: boş ekranla baş başa bırakmak yerine girişe alıyoruz. Oturum
  // kaybı, yerinde gösterilecek bir hata değil; sayfa değiştirmek gerekiyor.
  if (yetkisiz) {
    await signOut();
    redirect(`/giris?hata=${encodeURIComponent("Oturum süresi doldu, tekrar giriş yap.")}`);
  }

  return sonuc;
}

/**
 * İşlem bitince ilgili sayfayı sunucudan tazeliyor. Eskiden burada redirect
 * vardı ve her kaydette tam sayfa gezinmesi oluyordu; revalidatePath aynı işi
 * sayfayı yeniden yüklemeden yapıyor.
 */
function done(page: string, result: ActionResult, fallback: string): ActionResult {
  revalidatePath(page);

  return {
    ok: result.ok,
    message: result.message || (result.ok ? fallback : "İşlem başarısız."),
  };
}

export async function approveOpinion(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Görüş seçilmedi." };

  const result = await send(`/api/commentary/opinions/${id}/approve`, "POST", { note: null });
  return done("/admin/yorumlar", result, "Yayına alındı.");
}

/**
 * Onay ekranındaki klavye ve toplu işlem buradan geçiyor. Form yerine doğrudan
 * çağrılıyor: tuşa basınca form göndermek yerine eylemi çağırmak, satırda
 * kalmayı ve seçimi korumayı kolaylaştırıyor.
 */
export async function reviewOpinion(input: {
  id: string;
  approve: boolean;
  note?: string | null;
  /** Dolu gelirse alıntı bununla değiştirilip öyle onaylanıyor. */
  quote?: string | null;
  timestampSeconds?: number | null;
}): Promise<ActionResult> {
  if (!input.id) return { ok: false, message: "Görüş seçilmedi." };

  const yol = input.approve ? "approve" : "reject";

  const result = await send(`/api/commentary/opinions/${input.id}/${yol}`, "POST", {
    note: input.note ?? null,
    quote: input.quote ?? null,
    timestampSeconds: input.timestampSeconds ?? null,
  });

  return done("/admin/yorumlar", result, input.approve ? "Yayına alındı." : "Reddedildi.");
}

/** Ön kontrolden temiz çıkanları tek hamlede geçirmek için. */
export async function bulkReviewOpinions(input: {
  ids: string[];
  approve: boolean;
  note?: string | null;
}): Promise<ActionResult> {
  if (input.ids.length === 0) return { ok: false, message: "Hiç görüş seçilmedi." };

  const result = await send("/api/commentary/opinions/bulk-review", "POST", {
    ids: input.ids,
    approve: input.approve,
    note: input.note ?? null,
  });

  return done("/admin/yorumlar", result, "İşlendi.");
}

export async function rejectOpinion(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Görüş seçilmedi." };

  const note = String(formData.get("note") ?? "").trim();

  const result = await send(`/api/commentary/opinions/${id}/reject`, "POST", { note: note || null });
  return done("/admin/yorumlar", result, "Reddedildi.");
}

/** Konuşmacıyı sözlüğe ekler ve görüşe bağlar; sözlük kullandıkça büyüsün diye. */
export async function attachSpeaker(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Görüş seçilmedi." };

  const fullName = String(formData.get("fullName") ?? "").trim();

  const result = await send(`/api/commentary/opinions/${id}/attach-speaker`, "POST", {
    fullName: fullName || null,
  });

  return done("/admin/yorumlar", result, "Sözlüğe eklendi.");
}

export async function addChannel(_prev: ActionResult | null, formData: FormData) {
  const name = String(formData.get("name") ?? "").trim();
  const reference = String(formData.get("reference") ?? "").trim();
  const sportSlugs = formData.getAll("sportSlugs").map(String).filter(Boolean);

  if (!name || !reference) return { ok: false, message: "Kanal adı ve adresi gerekli." };

  const result = await send("/api/commentary/channels", "POST", {
    name,
    reference,
    sportSlugs,
  });

  return done("/admin/kanallar", result, `${name} eklendi.`);
}

export async function scanChannel(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Kanal seçilmedi." };

  const result = await send(`/api/commentary/channels/${id}/scan?maxAgeDays=7&maxPerRun=3`, "POST");
  return done("/admin/kanallar", result, "Tarandı.");
}

export async function toggleChannel(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Kanal seçilmedi." };

  const isActive = String(formData.get("isActive") ?? "") === "true";

  const result = await send(`/api/commentary/channels/${id}`, "PATCH", { isActive: !isActive });
  return done("/admin/kanallar", result, isActive ? "Durduruldu." : "Başlatıldı.");
}

/**
 * Kanalın kapsadığı branşlar. Görüşün branşını belirlemiyor — modele ipucu
 * olarak gidiyor ve tek branş varsa son çare yedek olarak kullanılıyor.
 */
export async function setChannelSports(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Kanal seçilmedi." };

  const sportSlugs = formData.getAll("sportSlugs").map(String).filter(Boolean);

  const result = await send(`/api/commentary/channels/${id}`, "PATCH", { sportSlugs });

  return done(
    "/admin/kanallar",
    result,
    sportSlugs.length > 0 ? "Branşlar kaydedildi." : "Branşlar kaldırıldı.",
  );
}

/** Kişi gerçekten yorumcu: doğrulanmış sayılıyor, kanal kadrosu güvenini kazanıyor. */
export async function verifyCommentator(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Yorumcu seçilmedi." };

  const result = await send(`/api/commentary/commentators/${id}/verify`, "POST");
  return done("/admin/yorumcular", result, "Doğrulandı.");
}

/** Yanlış eklenmiş kişi: kayıt siliniyor, görüşleri görüş onayına geri dönüyor. */
export async function rejectCommentator(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Yorumcu seçilmedi." };

  const result = await send(`/api/commentary/commentators/${id}/reject`, "POST");
  return done("/admin/yorumcular", result, "Silindi.");
}

/**
 * Yorumcu görselini elle giriyoruz. Boş gönderilince görsel siliniyor ve arayüz
 * baş harflere düşüyor — videodan türetilen yanlış portreleri böyle temizliyoruz.
 */
export async function updateCommentatorPhoto(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Yorumcu seçilmedi." };

  const photoUrl = String(formData.get("photoUrl") ?? "").trim();

  const result = await send(`/api/commentary/commentators/${id}`, "PATCH", {
    photoUrl,
    bio: null,
  });

  return done("/admin/yorumcular", result, photoUrl ? "Görsel kaydedildi." : "Görsel kaldırıldı.");
}

/** Arka plan işinin bulamadığı takım logosunu elle giriyoruz. */
export async function updateTeamLogo(_prev: ActionResult | null, formData: FormData) {
  const id = String(formData.get("id") ?? "");
  if (!id) return { ok: false, message: "Takım seçilmedi." };

  const logoUrl = String(formData.get("logoUrl") ?? "").trim();

  const result = await send(`/api/admin/teams/${id}`, "PATCH", { logoUrl });
  return done("/admin/takimlar", result, logoUrl ? "Logo kaydedildi." : "Logo kaldırıldı.");
}

export async function cikisYap(): Promise<void> {
  await signOut();
  redirect("/");
}
