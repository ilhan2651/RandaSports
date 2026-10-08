/**
 * Yönetim ekranlarındaki işlem sonucu şeridi. Sunucu eylemleri sonucu adres
 * çubuğuna yazıyor, burası okuyup gösteriyor — sessizce başarısız olan bir
 * düğme, bozuk bir düğmeden ayırt edilemiyor.
 */
export function AdminFlash({ ok, hata }: { ok?: string; hata?: string }) {
  const message = hata ?? ok;

  if (!message) return null;

  const isError = Boolean(hata);

  return (
    <div
      role="status"
      className={`mt-5 rounded-lg border px-4 py-3 text-sm ${
        isError
          ? "border-live/40 bg-live/10 text-live"
          : "border-amber/40 bg-amber/10 text-amber-soft"
      }`}
    >
      <span className="mr-2 font-display text-[10px] font-bold uppercase tracking-widest">
        {isError ? "Hata" : "Tamam"}
      </span>
      {message}
    </div>
  );
}
