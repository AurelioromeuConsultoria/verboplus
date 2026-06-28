# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Instala dependências primeiro (camada cacheável)
COPY package.json package-lock.json ./
RUN npm ci

# Copia o restante e builda (Vite carrega .env.production no modo production)
COPY . .
RUN npm run build

# Runtime stage — nginx servindo o site estático
FROM nginx:1.27-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
