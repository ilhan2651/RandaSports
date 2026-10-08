"use client";

export default function GlobalError({ reset }: { error: Error; reset: () => void }) {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center text-center">
      <span className="font-display text-6xl font-extrabold uppercase tracking-tight text-line">
        Hata
      </span>
      <h1 className="mt-4 font-display text-3xl font-extrabold uppercase tracking-tight">
        Bir şeyler ters gitti
      </h1>
      <p className="mt-2 max-w-md text-sm text-fog">
        Haberler yüklenemedi. API ayakta değilse birkaç saniye sonra tekrar deneyebilirsin.
      </p>
      <button
        onClick={reset}
        className="mt-8 rounded-full bg-amber px-6 py-3 font-display text-sm font-bold uppercase tracking-widest text-ink transition-transform hover:scale-105"
      >
        Tekrar dene
      </button>
    </div>
  );
}
