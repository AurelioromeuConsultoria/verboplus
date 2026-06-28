# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# pnpm via corepack (mesma versão do CI)
RUN corepack enable && corepack prepare pnpm@10.4.1 --activate

# Instala dependências primeiro (camada cacheável)
COPY package.json pnpm-lock.yaml ./
RUN pnpm install --frozen-lockfile

# Copia o restante e builda (Vite carrega .env.production no modo production)
COPY . .
RUN pnpm run build

# Runtime stage — nginx servindo o SPA estático
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
