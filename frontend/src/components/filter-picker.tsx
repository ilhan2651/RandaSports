"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { EntityAvatar } from "@/components/entity-avatar";

export type PickerItem = {
  value: string;
  label: string;
  imageUrl: string | null;
  count: number;
};

type Props = {
  /** Düğmenin üstündeki etiket: "Takım", "Yorumcu". */
  label: string;
  items: PickerItem[];
  active?: string;
  /** Adres çubuğunda bu filtreyi tutan parametre: "takim", "yorumcu". */
  paramName: string;
  basePath: string;
  /**
   * Diğer filtrelerin güncel hâli; seçim yapılınca korunuyorlar. Adresi burada
   * kuruyoruz çünkü sunucu bileşeninden istemci bileşenine fonksiyon geçirilemiyor —
   * yalnızca serileştirilebilir veri geçiyor.
   */
  query: Record<string, string>;
  /** Takım logoları dörtgen, yorumcu portreleri yuvarlak duruyor. */
  shape?: "circle" | "square";
};

/**
 * Uzun listeyi çip satırına serip sayfayı yutmak yerine bir düğmenin arkasına
 * alıyor. Seçim hâlâ bağlantı: adres çubuğunda duruyor, sunucuda render ediliyor
 * ve filtreli hâli paylaşılabiliyor — modal yalnızca seçme biçimi.
 */
export function FilterPicker({
  label,
  items,
  active,
  paramName,
  basePath,
  query,
  shape = "circle",
}: Props) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [arama, setArama] = useState("");
  const searchRef = useRef<HTMLInputElement>(null);

  const selected = items.find((x) => x.value === active);

  const filtered = useMemo(() => {
    const q = arama.trim().toLocaleLowerCase("tr");

    if (!q) return items;

    return items.filter((x) => x.label.toLocaleLowerCase("tr").includes(q));
  }, [items, arama]);

  useEffect(() => {
    if (!open) return;

    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") setOpen(false);
    };

    document.addEventListener("keydown", onKey);

    // Açılır açılmaz aramaya odaklanıyoruz: klavyeyle gelen kullanıcı
    // doğrudan yazmaya başlasın.
    searchRef.current?.focus();

    // Arkadaki sayfa kaymasın.
    const eski = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    return () => {
      document.removeEventListener("keydown", onKey);
      document.body.style.overflow = eski;
    };
  }, [open]);

  const sec = (value: string) => {
    setOpen(false);
    setArama("");

    const params = new URLSearchParams(query);

    if (value) params.set(paramName, value);
    else params.delete(paramName);

    const qs = params.toString();
    router.push(qs ? `${basePath}?${qs}` : basePath);
  };

  return (
    <>
      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => setOpen(true)}
          className="flex items-center gap-2 rounded-lg border border-line px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-fog outline-none transition-colors hover:border-amber/40 hover:text-chalk focus-visible:ring-2 focus-visible:ring-amber"
        >
          {label}
          <span className="text-fog/60">{items.length}</span>
        </button>

        {selected && (
          <button
            type="button"
            onClick={() => sec("")}
            className="flex items-center gap-1.5 rounded-lg border border-amber bg-amber px-3 py-1.5 font-display text-xs font-bold uppercase tracking-widest text-ink outline-none transition-opacity hover:opacity-85 focus-visible:ring-2 focus-visible:ring-amber"
            aria-label={`${selected.label} filtresini kaldır`}
          >
            {selected.label}
            <span aria-hidden>✕</span>
          </button>
        )}
      </div>

      {open && (
        <div
          className="fixed inset-0 z-[60] flex items-start justify-center bg-ink/80 p-4 pt-[10vh] backdrop-blur-sm"
          onClick={() => setOpen(false)}
          role="presentation"
        >
          <div
            className="flex max-h-[70vh] w-full max-w-2xl flex-col overflow-hidden rounded-2xl border border-line bg-surface shadow-2xl"
            onClick={(event) => event.stopPropagation()}
            role="dialog"
            aria-modal="true"
            aria-label={`${label} seç`}
          >
            <div className="flex items-center gap-3 border-b border-line px-4 py-3">
              <input
                ref={searchRef}
                value={arama}
                onChange={(event) => setArama(event.target.value)}
                placeholder={`${label} ara…`}
                className="min-w-0 flex-1 bg-transparent text-sm text-chalk outline-none placeholder:text-fog"
              />
              <button
                type="button"
                onClick={() => setOpen(false)}
                className="shrink-0 font-display text-xs font-bold uppercase tracking-widest text-fog transition-colors hover:text-chalk"
              >
                Kapat
              </button>
            </div>

            <div className="overflow-y-auto p-2">
              <button
                type="button"
                onClick={() => sec("")}
                className={`mb-1 flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-left transition-colors ${
                  active ? "text-fog hover:bg-raised hover:text-chalk" : "bg-raised text-chalk"
                }`}
              >
                <span className="font-display text-sm font-bold uppercase tracking-tight">
                  Hepsi
                </span>
              </button>

              {filtered.length === 0 ? (
                <p className="px-3 py-8 text-center text-sm text-fog">Eşleşen yok.</p>
              ) : (
                <div className="grid gap-1 sm:grid-cols-2">
                  {filtered.map((item) => (
                    <button
                      key={item.value}
                      type="button"
                      onClick={() => sec(item.value)}
                      className={`flex items-center gap-3 rounded-lg px-3 py-2 text-left transition-colors ${
                        item.value === active
                          ? "bg-amber text-ink"
                          : "text-chalk hover:bg-raised"
                      }`}
                    >
                      <EntityAvatar
                        name={item.label}
                        src={item.imageUrl}
                        size={32}
                        className={shape === "square" ? "!rounded-md" : ""}
                      />

                      <span className="min-w-0 flex-1 truncate font-display text-sm font-bold uppercase tracking-tight">
                        {item.label}
                      </span>

                      <span
                        className={`shrink-0 text-xs ${
                          item.value === active ? "text-ink/70" : "text-fog"
                        }`}
                      >
                        {item.count}
                      </span>
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
}
