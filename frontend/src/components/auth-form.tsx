/**
 * Giriş ve kayıt ekranlarının ortak kabuğu. İkisi de aynı görünümü paylaşıyor;
 * sayfa dosyasından bileşen dışa aktarmamak için ayrı duruyor.
 */
export function AuthShell({
  title,
  hata,
  children,
  footer,
}: {
  title: string;
  hata?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
}) {
  return (
    <div className="flex min-h-[70vh] items-center justify-center py-12">
      <div className="w-full max-w-sm">
        <div className="flex items-baseline justify-center gap-1">
          <span className="font-display text-2xl font-extrabold uppercase tracking-tight">
            Randa
          </span>
          <span className="font-display text-2xl font-extrabold uppercase tracking-tight text-amber">
            Sports
          </span>
        </div>

        <h1 className="mt-6 text-center font-display text-xl font-bold uppercase tracking-tight">
          {title}
        </h1>

        {hata && (
          <p className="mt-5 rounded-lg border border-live/40 bg-live/10 px-4 py-3 text-sm text-live">
            {hata}
          </p>
        )}

        {children}

        {footer && <p className="mt-5 text-center text-sm text-fog">{footer}</p>}
      </div>
    </div>
  );
}

export function Field({
  label,
  name,
  type,
  autoComplete,
  autoFocus,
  required = true,
  hint,
}: {
  label: string;
  name: string;
  type: string;
  autoComplete?: string;
  autoFocus?: boolean;
  required?: boolean;
  hint?: string;
}) {
  return (
    <label className="block">
      <span className="font-display text-[11px] font-bold uppercase tracking-widest text-fog">
        {label}
      </span>
      <input
        type={type}
        name={name}
        required={required}
        autoComplete={autoComplete}
        autoFocus={autoFocus}
        className="mt-1.5 w-full rounded-lg border border-line bg-surface px-3 py-2.5 text-sm text-chalk outline-none placeholder:text-fog focus-visible:border-amber"
      />
      {hint && <span className="mt-1 block text-xs text-fog">{hint}</span>}
    </label>
  );
}

export function Submit({ children }: { children: React.ReactNode }) {
  return (
    <button
      type="submit"
      className="w-full rounded-lg bg-amber px-4 py-2.5 font-display text-xs font-bold uppercase tracking-widest text-ink transition-colors hover:bg-amber-soft"
    >
      {children}
    </button>
  );
}
