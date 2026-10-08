# Bağlam: frontend/ klasörü.

FROM node:22-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

FROM node:22-alpine AS build
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_TELEMETRY_DISABLED=1
RUN npm run build

FROM node:22-alpine AS runtime
WORKDIR /app

ENV NODE_ENV=production \
    NEXT_TELEMETRY_DISABLED=1 \
    PORT=3000 \
    HOSTNAME=0.0.0.0

# next/image üretimde görsel işlemek için sharp istiyor; standalone çıktısı onu
# kendiliğinden taşımıyor, burada ayrıca kuruyoruz.
RUN npm install --omit=dev sharp && npm cache clean --force

RUN addgroup -g 1001 -S nodejs && adduser -S nextjs -u 1001

# standalone çıktısı yalnızca gerçekten kullanılan modülleri taşıyor; imaj
# node_modules'ün tamamını taşımaktan çok daha küçük kalıyor.
COPY --from=build --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=build --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000

CMD ["node", "server.js"]
