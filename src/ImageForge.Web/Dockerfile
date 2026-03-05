# ─── ImageForge Angular Frontend ───
# Multi-stage: build Angular → serve with nginx

# Stage 1: Build
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

# environment.ts'i Docker için güncelle (API proxy üzerinden)
RUN sed -i "s|http://localhost:5265/api|/api|g" src/app/core/environment.ts

RUN npx ng build --configuration=production

# Stage 2: Serve
FROM nginx:alpine

# Angular build çıktısını kopyala (proje adı: ImageForge.Web)
COPY --from=build /app/dist/ImageForge.Web/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
