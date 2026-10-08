import Link from "next/link";

/**
 * Başlığın sağ ucu. Girişlinin birincil varış noktası akış: giriş yapmanın karşılığı
 * kişiselleşmiş akış, hesap ayarları değil. Yanındaki küçük "Hesap" bağlantısı
 * tercihlere, yönetime ve çıkışa açılıyor — çıkışı yalnızca akış sayfasındaki küçük
 * yazının arkasına koymak onu bulunamaz yapıyordu.
 *
 * Durumu sunucudan prop olarak alıyor: oturum çerezi httpOnly olduğu için tarayıcıdan
 * okunamıyor, okunabilen bir çereze bakmak da ilk çizimde yanlış yazı gösteriyordu.
 */
export function AccountLink({ girisli }: { girisli: boolean }) {
  if (!girisli)
    return (
      <Link
        href="/giris"
        className="shrink-0 rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog outline-none transition-colors hover:border-amber/40 hover:text-chalk focus-visible:ring-2 focus-visible:ring-amber"
      >
        Giriş
      </Link>
    );

  return (
    <div className="flex shrink-0 items-center gap-2">
      <Link
        href="/akis"
        className="rounded-lg border border-amber/40 px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-amber outline-none transition-colors hover:bg-amber/10 focus-visible:ring-2 focus-visible:ring-amber"
      >
        Akışım
      </Link>

      <Link
        href="/hesap"
        title="Hesabım"
        className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog outline-none transition-colors hover:border-amber/40 hover:text-chalk focus-visible:ring-2 focus-visible:ring-amber"
      >
        Hesap
      </Link>
    </div>
  );
}
