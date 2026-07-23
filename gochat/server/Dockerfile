FROM golang:1-alpine AS base
WORKDIR /app
COPY go.mod go.sum ./
RUN go mod download

FROM base AS dev
RUN go install github.com/air-verse/air@latest
COPY . .
EXPOSE 8080
CMD [ "air", "-c", ".air.toml" ]

FROM base AS builder
COPY . .
RUN CGO_ENABLED=0 GOOS=linux go build -ldflags="-w -s" -o /app/server ./cmd/server

FROM alpine:3 AS prod
WORKDIR /app
RUN apk --no-cache add ca-certificates
COPY --from=builder /app/server /app/server
EXPOSE 8080
CMD [ "/app/server" ]