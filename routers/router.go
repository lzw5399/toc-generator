package routers

import (
	"github.com/gin-gonic/gin"
	"toc-generator/controllers"
)

func InitRouter() *gin.Engine{
	r := gin.New()
	r.Use(gin.Logger())
	r.Use(gin.Recovery())

	r.StaticFile("/favicon.ico", "./assets/favicon.ico")
	r.LoadHTMLGlob("views/*")

	// route
	r.GET("/", controllers.Index)
	r.POST("/convert", controllers.Convert)

	return r
}
