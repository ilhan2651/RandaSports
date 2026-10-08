import Link from "next/link";

type PaginationProps = {
  page: number;
  pageSize: number;
  total: number;
  basePath: string;
  params?: Record<string, string>;
};

export function Pagination({ page, pageSize, total, basePath, params = {} }: PaginationProps) {
  const lastPage = Math.max(1, Math.ceil(total / Math.max(pageSize, 1)));
  if (lastPage <= 1) return null;

  const href = (target: number) => {
    const query = new URLSearchParams(params);
    if (target > 1) query.set("page", String(target));
    const suffix = query.toString();
    return suffix ? `${basePath}?${suffix}` : basePath;
  };

  const linkClass =
    "rounded-full border border-line px-5 py-2.5 font-display text-sm font-bold uppercase tracking-widest transition-colors hover:border-amber hover:text-amber";

  return (
    <nav className="mt-12 flex items-center justify-center gap-4">
      {page > 1 ? (
        <Link href={href(page - 1)} className={linkClass}>
          Önceki
        </Link>
      ) : (
        <span className={`${linkClass} pointer-events-none opacity-30`}>Önceki</span>
      )}

      <span className="font-display text-sm uppercase tracking-widest text-fog">
        {page} / {lastPage}
      </span>

      {page < lastPage ? (
        <Link href={href(page + 1)} className={linkClass}>
          Sonraki
        </Link>
      ) : (
        <span className={`${linkClass} pointer-events-none opacity-30`}>Sonraki</span>
      )}
    </nav>
  );
}
