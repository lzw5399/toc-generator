package main

import (
	"os"
	"toc-generator/routers"
)

func main() {
	r := routers.InitRouter()

	port := os.Getenv("PORT")
	if port == "" {
		port = "8081"
	}

	_ = r.Run(":" + port)
}
