FROM oven/bun:1 AS build
WORKDIR /app
COPY bun.lock package.json ./
RUN bun install --frozen-lockfile
COPY . .
RUN bun run build

FROM nginx:alpine AS client
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80

FROM oven/bun:1 AS dev
WORKDIR /app
COPY bun.lock package.json ./
RUN bun install
COPY . .
EXPOSE 3000
CMD ["bun", "run", "dev", "--host", "0.0.0.0"]
