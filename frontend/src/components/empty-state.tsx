export function EmptyState({ title, hint }: { title: string; hint?: string }) {
  return (
    <div className="mt-10 flex flex-col items-center rounded-2xl border border-dashed border-line bg-surface px-6 py-20 text-center">
      <span className="font-display text-3xl font-extrabold uppercase tracking-tight text-line">
        Randa<span className="text-amber/40">Sports</span>
      </span>
      <h2 className="mt-6 font-display text-2xl font-bold uppercase tracking-tight">{title}</h2>
      {hint && <p className="mt-2 max-w-md text-sm text-fog">{hint}</p>}
    </div>
  );
}
