import type { Metadata } from "next";
import { Barlow_Condensed, Inter } from "next/font/google";
import { SiteHeader } from "@/components/site-header";
import { SiteFooter } from "@/components/site-footer";
import { isSignedIn } from "@/lib/auth";
import { getSports } from "@/lib/sports-nav";
import "./globals.css";

const display = Barlow_Condensed({
  subsets: ["latin", "latin-ext"],
  weight: ["600", "700", "800"],
  variable: "--font-display",
});

const body = Inter({
  subsets: ["latin", "latin-ext"],
  variable: "--font-body",
});

export const metadata: Metadata = {
  title: {
    default: "RandaSports — Spor haberleri, tek yerde",
    template: "%s · RandaSports",
  },
  description:
    "Futbol, basketbol, MMA ve NFL haberleri. Birden fazla kaynak tek habere indiriliyor.",
};

export default async function RootLayout({ children }: { children: React.ReactNode }) {
  // Menü yalnızca haberi olan branşları gösteriyor; boş sekme kullanıcıyı yoruyor.
  const sports = await getSports(true);

  // Başlıktaki hesap bağlantısı için; çerez sunucuda okunuyor.
  const girisli = await isSignedIn();

  return (
    <html lang="tr" className={`${display.variable} ${body.variable}`}>
      <body className="min-h-screen bg-ink text-chalk antialiased">
        <SiteHeader sports={sports} girisli={girisli} />
        <main className="mx-auto w-full max-w-[1280px] px-4 pb-24 sm:px-6">{children}</main>
        <SiteFooter sports={sports} />
      </body>
    </html>
  );
}
