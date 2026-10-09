import type { Metadata } from "next";
import Link from "next/link";
import { authHeader } from "@/lib/auth";
import { getChannels, getOpinions, getUnverifiedCommentators } from "@/lib/commentary";

export const metadata: Metadata = {
  title: "Yönetim",
  robots: { index: false, follow: false },
};

const API_BASE = process.env.API_BASE_URL ?? "http://localhost:5122";

export default async function AdminHomePage() {
  // Hepsi birbirinden bağımsız; paralel gidiyor.
  const [bekleyen, yayinda, reddedilen, dogrulanmamis, kanallar, takimlar] = await Promise.all([
    getOpinions("Pending", 1, 1),
    getOpinions("Approved", 1, 1),
    getOpinions("Rejected", 1, 1),
    getUnverifiedCommentators(),
    getChannels(),
    getTeams(),
  ]);

  const hataliKanal = kanallar.filter((x) => x.lastError).length;
  const kapaliKanal = kanallar.filter((x) => !x.isActive).length;
  const logosuz = takimlar.filter((x) => !x.logoUrl).length;

  return (
    <div>
      <header className="mt-8">
        <p className="font-display text-xs font-bold uppercase tracking-[0.2em] text-amber">
          Yönetim
        </p>
        <h1 className="mt-2 font-display text-3xl font-extrabold uppercase tracking-tight">
          Özet
        </h1>
        <p className="mt-3 max-w-[70ch] text-sm leading-relaxed text-fog">
          Bekleyen işler burada. Rakamlar canlı; bir bölümde sayı varsa oraya girip
          elden geçirmen gerekiyor.
        </p>
      </header>

      <section className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <Kart
          href="/admin/yorumlar?durum=Pending"
          baslik="Onay bekleyen görüş"
          sayi={bekleyen.total}
          alt="Atıf güveni eşiğin altında kaldı, elle teyit istiyor."
          vurgu={bekleyen.total > 0}
        />
        <Kart
          href="/admin/yorumcular"
          baslik="Doğrulanmamış yorumcu"
          sayi={dogrulanmamis.length}
          alt="Kadroya alınmayan kişi, görüşlerinin güvenini de düşürüyor."
          vurgu={dogrulanmamis.length > 0}
        />
        <Kart
          href="/admin/kanallar"
          baslik="Hata veren kanal"
          sayi={hataliKanal}
          alt={`${kanallar.length} kanalın ${kapaliKanal} tanesi kapalı.`}
          vurgu={hataliKanal > 0}
        />
        <Kart
          href="/admin/takimlar"
          baslik="Logosuz takım"
          sayi={logosuz}
          alt={`${takimlar.length} takımın ${takimlar.length - logosuz} tanesinde logo var.`}
          vurgu={logosuz > 0}
        />
        <Kart
          href="/admin/yorumlar?durum=Approved"
          baslik="Yayındaki görüş"
          sayi={yayinda.total}
          alt="Siteye çıkmış, doğrulanmış alıntılar."
        />
        <Kart
          href="/admin/yorumlar?durum=Rejected"
          baslik="Reddedilen görüş"
          sayi={reddedilen.total}
          alt="Yanlış atıf ya da yorum sayılmayan demeçler."
        />
      </section>
    </div>
  );
}

function Kart({
  href,
  baslik,
  sayi,
  alt,
  vurgu = false,
}: {
  href: string;
  baslik: string;
  sayi: number;
  alt: string;
  /** Sıfırdan büyükse ilgilenilmesi gereken bir iş var; kart öne çıkıyor. */
  vurgu?: boolean;
}) {
  return (
    <Link
      href={href}
      className={`group rounded-2xl border bg-surface p-5 outline-none transition-colors focus-visible:ring-2 focus-visible:ring-amber ${
        vurgu ? "border-amber/40 hover:border-amber" : "border-line hover:border-amber/40"
      }`}
    >
      <p className="font-display text-xs font-bold uppercase tracking-widest text-fog">
        {baslik}
      </p>

      <p
        className={`mt-2 font-display text-4xl font-extrabold tabular-nums ${
          vurgu ? "text-amber" : "text-chalk"
        }`}
      >
        {sayi}
      </p>

      <p className="mt-2 text-xs leading-relaxed text-fog">{alt}</p>
    </Link>
  );
}

type TakimOzet = { logoUrl: string | null };

async function getTeams(): Promise<TakimOzet[]> {
  try {
    const response = await fetch(`${API_BASE}/api/admin/teams`, {
      headers: await authHeader(),
      cache: "no-store",
    });

    if (!response.ok) return [];

    const payload = (await response.json()) as { success: boolean; data?: TakimOzet[] };
    return payload.success ? (payload.data ?? []) : [];
  } catch {
    return [];
  }
}
