"use client";

import Image from "next/image";
import { useState } from "react";

type StoryImageProps = {
  src: string | null;
  sizes: string;
  priority?: boolean;
  className?: string;
};

/**
 * Haber görselleri dış sitelerden geliyor; biri kırıkça kart bozulmasın diye
 * yükleme hatasında degrade zemine düşüyoruz.
 */
export function StoryImage({ src, sizes, priority, className }: StoryImageProps) {
  const [failed, setFailed] = useState(false);

  if (!src || failed) {
    return (
      <div className="absolute inset-0 bg-gradient-to-br from-raised via-surface to-ink">
        <div className="absolute inset-0 opacity-[0.07] [background-image:repeating-linear-gradient(135deg,transparent,transparent_12px,var(--color-chalk)_12px,var(--color-chalk)_13px)]" />
      </div>
    );
  }

  return (
    <Image
      src={src}
      alt=""
      fill
      priority={priority}
      sizes={sizes}
      onError={() => setFailed(true)}
      className={className}
    />
  );
}
