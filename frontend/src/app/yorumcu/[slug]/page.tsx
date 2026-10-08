import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { Avatar } from "@/components/opinion-card";
import { EmptyState } from "@/components/empty-state";
import { OpinionDayGroups } from "@/components/opinion-day-groups";
import { Pagination } from "@/components/pagination";
import { getCommentator, getLatestOpinions } from "@/lib/commentary";
import { timeAgo } from "@/lib/format";

// Önbellek yok, bilerek: bu sayfa searchParams okuyor (sayfalama ve süzgeçler) ve
// sorgu dizesine bakan bir sayfa önceden üretilemiyor. Burada `revalidate` yazmak
// Next'e "bunu önbelleğe al" demek oluyordu ve üretim derlemesinde sayfa
// DYNAMIC_SERVER_USAGE ile 500 veriyordu.

const SITE_URL = process.env.SITE_URL ?? "http://localhost:3000";
const PAGE_SIZE = 18;

type Params = {
  params: Promise<{ slug: string }>;
  searchParams: Promise<{ page?: string }>;
};

export async function generateMetadata({ params }: Params): Promise<Metadata> {
  const { slug } = await params;
  const commentator = await getCommentator(slug);

  if (!commentator) return { title: "Yorumcu bulunamadı" };

  return {
    title: commentator.fullName,
    description: `${commentator.fullName} yayınlarda ne dedi — görüşleri, tahminleri ve söylediği anın video bağlantısı.`,
    alternates: { canonical: `${SITE_URL}/yorumcu/${slug}` },
  };
}

export default async function CommentatorPage({ params, searchParams }: Params) {
  const { slug } = await params;
  const { page: pageParam } = await searchParams;
  const page = Number(pageParam) > 0 ? Number(pageParam) : 1;

  const commentator = await getCommentator(slug);

  if (!commentator) notFound();

  const opinions = await getLatestOpinions({
    commentator: slug,
    page,
    pageSize: PAGE_SIZE,
  });

  return (
    <div className="pt-8">
      <nav className="flex items-center gap-2 text-xs uppercase tracking-widest text-fog">
        <Link href="/" className="transition-colors hover:text-amber">
          Anasayfa
        </Link>
        <span>/</span>
        <Link href="/yorumcular" className="transition-colors hover:text-amber">
          Yorumcular
        </Link>
      </nav>

      <header className="mt-6 flex flex-wrap items-center gap-6 border-b border-line pb-8">
        <Avatar name={commentator.fullName} photoUrl={commentator.photoUrl} size="lg" />

        <div className="min-w-0 flex-1">
          <h1 className="font-display text-4xl font-extrabold uppercase leading-[0.95] tracking-tight sm:text-5xl">
            {commentator.fullName}
          </h1>

          <div className="mt-3 flex flex-wrap items-center gap-3 text-sm text-fog">
            <span className="font-display font-bold uppercase tracking-widest text-amber">
              {commentator.opinionCount} görüş
            </span>
            {commentator.lastOpinionAt && (
              <>
                <span className="size-1 rounded-full bg-line" />
                <span>son görüş {timeAgo(commentator.lastOpinionAt)}</span>
              </>
            )}
          </div>

          {commentator.bio && (
            <p className="mt-4 max-w-[68ch] leading-relaxed text-fog">{commentator.bio}</p>
          )}
        </div>
      </header>

      {opinions.items.length === 0 ? (
        <EmptyState
          title="Henüz yayınlanmış görüş yok"
          hint="Bu yorumcunun görüşleri onaylandıkça burada listelenecek."
        />
      ) : (
        <>
          <div className="mt-8">
            <OpinionDayGroups opinions={opinions.items} linkCommentator={false} />
          </div>

          <Pagination
            page={opinions.page}
            pageSize={opinions.pageSize}
            total={opinions.total}
            basePath={`/yorumcu/${slug}`}
          />
        </>
      )}
    </div>
  );
}
