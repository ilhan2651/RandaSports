import Link from "next/link";

export default function NotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center text-center">
      <span className="font-display text-7xl font-extrabold uppercase tracking-tight text-line">
        404
      </span>
      <h1 className="mt-4 font-display text-3xl font-extrabold uppercase tracking-tight">
        Sayfa bulunamadı
      </h1>
      <p className="mt-2 text-sm text-fog">Aradığın haber kaldırılmış ya da hiç var olmamış.</p>
      <Link
        href="/"
        className="mt-8 rounded-full bg-amber px-6 py-3 font-display text-sm font-bold uppercase tracking-widest text-ink transition-transform hover:scale-105"
      >
        Anasayfaya dön
      </Link>
    </div>
  );
}
