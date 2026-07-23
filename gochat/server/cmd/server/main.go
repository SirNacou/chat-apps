package main

import (
	"log/slog"
	"net/http"

	"github.com/danielgtaylor/huma/v2"
	"github.com/danielgtaylor/huma/v2/adapters/humaecho"
	"github.com/labstack/echo/v5"
)

func main() {
	e := echo.New()

	_ = humaecho.New(e, huma.DefaultConfig("Server", "1.0.0"))

	if e := http.ListenAndServe("0.0.0.0:8080", e); e != nil {
		slog.Error("Failed to start server.", "error", e)
	}
}
