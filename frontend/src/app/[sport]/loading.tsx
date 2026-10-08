import { CardGridSkeleton } from "@/components/skeletons";

export default function SportLoading() {
  return (
    <div className="pt-10">
      <div className="h-12 w-56 animate-pulse rounded-lg bg-raised" />
      <div className="mt-6 min-h-[420px] animate-pulse rounded-2xl bg-raised lg:min-h-[560px]" />
      <div className="mt-16">
        <CardGridSkeleton count={6} />
      </div>
    </div>
  );
}
