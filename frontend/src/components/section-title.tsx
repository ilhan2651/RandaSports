export function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <div className="mb-5 flex items-center gap-4">
      <h2 className="font-display text-2xl font-extrabold uppercase tracking-tight">{children}</h2>
      <span className="h-px flex-1 bg-line" />
    </div>
  );
}
