import { NextResponse, type NextRequest } from "next/server";

const SESSION_COOKIE = "randa_oturum";
const ROLE_COOKIE = "randa_rol";

/**
 * Kapı kontrolü. Yalnızca çereze bakıyor — imza ve rol doğrulamasını API yapıyor,
 * çünkü middleware Edge'de çalışıyor ve imza anahtarını oraya taşımak istemiyoruz.
 * Sahte çerezle giren biri sayfayı görse de hiçbir veri alamıyor: API jetonu
 * doğrulayıp 401, yetkisiz rolde 403 döndürüyor.
 */
export function middleware(request: NextRequest) {
  const session = request.cookies.get(SESSION_COOKIE)?.value;

  if (!session) return girise(request);

  // Yönetim ekranları ayrıca yönetici rolü istiyor; okuyucu hesabı buraya giremez.
  if (request.nextUrl.pathname.startsWith("/admin")) {
    const roles = request.cookies.get(ROLE_COOKIE)?.value?.split(",") ?? [];

    if (!roles.includes("admin"))
      return NextResponse.redirect(new URL("/hesap", request.url));
  }

  return NextResponse.next();
}

function girise(request: NextRequest) {
  const url = new URL("/giris", request.url);

  // Girişten sonra gelmek istediği sayfaya dönebilsin.
  url.searchParams.set("devam", request.nextUrl.pathname + request.nextUrl.search);

  return NextResponse.redirect(url);
}

export const config = {
  matcher: ["/admin/:path*", "/hesap/:path*"],
};
