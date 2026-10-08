"use client";

import { motion, useScroll, useSpring } from "framer-motion";

/** Haber sayfasının üstünde ne kadar okunduğunu gösteren ince amber çizgi. */
export function ReadingProgress() {
  const { scrollYProgress } = useScroll();
  const scaleX = useSpring(scrollYProgress, { stiffness: 220, damping: 40, restDelta: 0.001 });

  return (
    <motion.div
      style={{ scaleX }}
      className="fixed inset-x-0 top-0 z-[60] h-0.5 origin-left bg-amber"
      aria-hidden
    />
  );
}
