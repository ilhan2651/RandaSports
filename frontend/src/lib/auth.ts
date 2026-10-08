import { cookies } from "next/headers";

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

/** Erişim jetonu. httpOnly: tarayıcıdaki betikler okuyamıyor. */
export const SESSION_COOKIE = "randa_oturum";

/** Yenileme jetonu. Erişim jetonu kısa ömürlü, oturumu bu ayakta tutuyor. */
export const REFRESH_COOKIE = "randa_yenileme";

/** Rol, yalnızca arayüzün ne göstereceğine karar vermek için; yetki kontrolü API'de. */
export const ROLE_COOKIE = "randa_rol";

type AuthResponse = {
  token: string;
  expiresAt: string;
  refreshToken: string;
  refreshExpiresAt: string;
  email: string;
  fullName: string | null;
  roles: string[];
};

type ApiResult<T> = {
  success: boolean;
  data?: T;
  messages: string[];
};

export type AuthOutcome = { ok: true; roles: string[] } | { ok: false; message: string };

export async function signIn(email: string, password: string): Promise<AuthOutcome> {
  return post("/api/auth/login", { email, password }, "E-posta ya da parola hatalı.");
}

export async function signUp(
  email: string,
  password: string,
  fullName: string | null,
): Promise<AuthOutcome> {
  return post("/api/auth/register", { email, password, fullName }, "Kayıt tamamlanamadı.");
}

/**
 * Erişim jetonunun süresi dolduğunda sessizce yenilemeyi deniyor. Başarısızsa
 * çerezleri temizliyor: kullanıcı bir sonraki yönetim isteğinde girişe düşsün.
 */
export async function refreshSession(): Promise<boolean> {
  const store = await cookies();
  const refreshToken = store.get(REFRESH_COOKIE)?.value;

  if (!refreshToken) return false;

  const sonuc = await post("/api/auth/refresh", { refreshToken }, "Oturum yenilenemedi.");

  if (!sonuc.ok) await signOut();

  return sonuc.ok;
}

export async function signOut(): Promise<void> {
  const store = await cookies();
  const refreshToken = store.get(REFRESH_COOKIE)?.value;

  // Sunucudaki yenileme jetonunu da iptal ediyoruz; yoksa çerez silinse de
  // jeton 30 gün boyunca geçerli kalırdı.
  if (refreshToken) {
    try {
      await fetch(`${API_BASE}/api/auth/logout`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken }),
        cache: "no-store",
      });
    } catch {
      // Ağ hatası çıkışı engellemesin; çerezler yine de siliniyor.
    }
  }

  store.delete(SESSION_COOKIE);
  store.delete(REFRESH_COOKIE);
  store.delete(ROLE_COOKIE);
}

/** API isteklerinde kullanılacak başlık; oturum yoksa boş döner. */
export async function authHeader(): Promise<Record<string, string>> {
  const token = (await cookies()).get(SESSION_COOKIE)?.value;
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export async function currentRoles(): Promise<string[]> {
  const raw = (await cookies()).get(ROLE_COOKIE)?.value;
  return raw ? raw.split(",").filter(Boolean) : [];
}

export async function isSignedIn(): Promise<boolean> {
  return Boolean((await cookies()).get(SESSION_COOKIE)?.value);
}

async function post(
  path: string,
  body: unknown,
  fallbackMessage: string,
): Promise<AuthOutcome> {
  let payload: ApiResult<AuthResponse> | null = null;

  try {
    const response = await fetch(`${API_BASE}${path}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body),
      cache: "no-store",
    });

    payload = (await response.json().catch(() => null)) as ApiResult<AuthResponse> | null;

    if (!response.ok || !payload?.success || !payload.data)
      return { ok: false, message: payload?.messages?.join(" ") || fallbackMessage };
  } catch {
    return { ok: false, message: "API'ye ulaşılamadı — backend çalışıyor mu?" };
  }

  // Eski sürüm API yenileme jetonu ve rol döndürmüyor. Eksik alanla devam edersek
  // çerezleri yarım yazıp anlaşılmaz bir hatayla çöküyoruz; baştan söylemek daha iyi.
  if (!payload.data.refreshToken || !Array.isArray(payload.data.roles))
    return {
      ok: false,
      message: "API beklenen cevabı döndürmedi — migration alınıp yeniden başlatıldı mı?",
    };

  await storeSession(payload.data);
  return { ok: true, roles: payload.data.roles };
}

async function storeSession(data: AuthResponse): Promise<void> {
  const store = await cookies();

  const ortak = {
    sameSite: "lax" as const,
    secure: process.env.NODE_ENV === "production",
    path: "/",
  };

  store.set(SESSION_COOKIE, data.token, {
    ...ortak,
    httpOnly: true,
    expires: new Date(data.expiresAt),
  });

  store.set(REFRESH_COOKIE, data.refreshToken, {
    ...ortak,
    httpOnly: true,
    expires: new Date(data.refreshExpiresAt),
  });

  // Bu çerez httpOnly değil ama içinde gizli bir şey yok: yalnızca menüde
  // "Yönetim" bağlantısını gösterip göstermeyeceğimize karar veriyor.
  store.set(ROLE_COOKIE, (data.roles ?? []).join(","), {
    ...ortak,
    httpOnly: false,
    expires: new Date(data.refreshExpiresAt),
  });
}
