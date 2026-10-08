function Block({ className }: { className?: string }) {
  return <div className={`animate-pulse rounded-lg bg-raised ${className ?? ""}`} />;
}

export function HeroSkeleton() {
  return (
    <div className="grid gap-6 lg:grid-cols-[1.9fr_1fr]">
      <Block className="min-h-[420px] rounded-2xl lg:min-h-[560px]" />
      <div className="rounded-2xl border border-line bg-surface p-5">
        <Block className="h-4 w-32" />
        <div className="mt-6 space-y-6">
          {Array.from({ length: 5 }).map((_, index) => (
            <div key={index} className="space-y-2">
              <Block className="h-4 w-full" />
              <Block className="h-4 w-2/3" />
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}

export function CardGridSkeleton({ count = 6 }: { count?: number }) {
  return (
    <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="overflow-hidden rounded-xl border border-line bg-surface">
          <Block className="aspect-[16/10] rounded-none" />
          <div className="space-y-3 p-4">
            <Block className="h-5 w-full" />
            <Block className="h-5 w-3/4" />
            <Block className="h-3 w-1/3" />
          </div>
        </div>
      ))}
    </div>
  );
}

export function ArticleSkeleton() {
  return (
    <div className="pt-8">
      <Block className="h-3 w-48" />
      <div className="mt-6 space-y-4">
        <Block className="h-12 w-full" />
        <Block className="h-12 w-4/5" />
      </div>
      <Block className="mt-8 aspect-[16/9] rounded-2xl" />
      <div className="mt-10 max-w-[68ch] space-y-3">
        {Array.from({ length: 8 }).map((_, index) => (
          <Block key={index} className={`h-4 ${index % 3 === 2 ? "w-2/3" : "w-full"}`} />
        ))}
      </div>
    </div>
  );
}
