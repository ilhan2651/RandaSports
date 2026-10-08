"use client";

import { useActionState } from "react";

export type ActionResult = { ok: boolean; message: string };

type Props = {
  action: (prev: ActionResult | null, formData: FormData) => Promise<ActionResult>;
  children: React.ReactNode;
  className?: string;
  /** Başarı mesajını göstermeye değmeyen yerlerde kapatılıyor (satır zaten güncelleniyor). */
  quiet?: boolean;
};

/**
 * Yönetim ekranlarındaki form. Eskiden sunucu eylemi sonuç mesajını adres
 * çubuğuna yazıp redirect ediyordu; her kaydette tam sayfa gezinmesi oluyor,
 * kaydırma yeri kayıyor ve 30 satırlık listede hangi satıra bastığını
 * kaybediyordun. Artık eylem sonucu döndürüyor, bu bileşen yerinde gösteriyor.
 *
 * Veri tazeliğini sunucu tarafındaki revalidatePath sağlıyor: sayfa yeniden
 * yüklenmeden ilgili bölüm sunucudan tazeleniyor.
 */
export function AdminForm({ action, children, className = "", quiet = false }: Props) {
  const [state, formAction, pending] = useActionState(action, null);

  return (
    <form action={formAction} className={className}>
      {/* İstek sürerken alanlar kilitleniyor: çift gönderimi ve "bastım mı?" belirsizliğini bitiriyor. */}
      <fieldset disabled={pending} className="contents">
        {children}
      </fieldset>

      {pending && (
        <span className="font-display text-[10px] font-bold uppercase tracking-widest text-fog">
          …
        </span>
      )}

      {!pending && state && !(quiet && state.ok) && (
        <span
          role="status"
          className={`font-display text-[10px] font-bold uppercase tracking-widest ${
            state.ok ? "text-amber" : "text-live"
          }`}
        >
          {state.ok ? state.message || "Tamam" : state.message || "Hata"}
        </span>
      )}
    </form>
  );
}
