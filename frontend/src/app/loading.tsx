import { CardGridSkeleton, HeroSkeleton } from "@/components/skeletons";

export default function HomeLoading() {
  return (
    <div className="pt-10">
      <HeroSkeleton />
      <div className="mt-16">
        <CardGridSkeleton count={6} />
      </div>
    </div>
  );
}
