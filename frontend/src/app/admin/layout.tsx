import { cikisYap } from "@/app/admin/actions";
import { AdminNav } from "@/components/admin-nav";

/**
 * Yönetim ekranlarının ortak çerçevesi: bölüm menüsü ve çıkış. Yetki kontrolü
 * burada değil, middleware'de — admin rolü olmayan bu yollara hiç ulaşamıyor.
 */
export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="pt-6">
      <div className="flex flex-wrap items-center gap-3 border-b border-line pb-4">
        <AdminNav />

        <form action={cikisYap} className="ml-auto">
          <button
            type="submit"
            className="rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog outline-none transition-colors hover:border-live/40 hover:text-live focus-visible:ring-2 focus-visible:ring-live"
          >
            Çıkış
          </button>
        </form>
      </div>

      {children}
    </div>
  );
}
