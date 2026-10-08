import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Docker imajı için: Next, yalnızca gerçekten kullanılan modülleri içeren
  // kendi kendine yeten bir çıktı üretiyor. node_modules'ün tamamını imaja
  // taşımaktan çok daha küçük kalıyor.
  output: "standalone",

  images: {
    remotePatterns: [
      { protocol: "https", hostname: "**" },
      { protocol: "http", hostname: "**" },
    ],
  },
};

export default nextConfig;
