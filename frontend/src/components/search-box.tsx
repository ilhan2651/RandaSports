"use client";

import { useRouter } from "next/navigation";
import { AnimatePresence, motion } from "framer-motion";
import { useEffect, useRef, useState } from "react";

export function SearchBox({ initialQuery = "" }: { initialQuery?: string }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [value, setValue] = useState(initialQuery);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  function submit(event: React.FormEvent) {
    event.preventDefault();
    const term = value.trim();
    if (term.length < 2) return;

    router.push(`/arama?q=${encodeURIComponent(term)}`);
    setOpen(false);
  }

  return (
    <form onSubmit={submit} className="flex items-center">
      <AnimatePresence initial={false}>
        {open && (
          <motion.input
            ref={inputRef}
            initial={{ width: 0, opacity: 0 }}
            animate={{ width: 200, opacity: 1 }}
            exit={{ width: 0, opacity: 0 }}
            transition={{ duration: 0.28, ease: [0.22, 1, 0.36, 1] }}
            value={value}
            onChange={(event) => setValue(event.target.value)}
            onBlur={() => !value && setOpen(false)}
            placeholder="Haber ara"
            aria-label="Haber ara"
            className="mr-1 rounded-full border border-line bg-surface px-4 py-1.5 text-sm outline-none placeholder:text-fog focus:border-amber"
          />
        )}
      </AnimatePresence>

      <button
        type={open ? "submit" : "button"}
        onClick={() => !open && setOpen(true)}
        aria-label="Ara"
        className="rounded-full border border-line p-2 text-fog outline-none transition-colors hover:border-amber hover:text-amber focus-visible:ring-2 focus-visible:ring-amber"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" className="size-4">
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-3.2-3.2" strokeLinecap="round" />
        </svg>
      </button>
    </form>
  );
}
