package controllers

import (
	"github.com/gin-gonic/gin"
	"net/http"
	"toc-generator/services"
)

// Method: GET
// Url: /
func Index(c *gin.Context) {
	c.HTML(http.StatusOK, "index.html", gin.H{
		"title": "hello a lzw",
	})
}

// Method: POST
// Url: /convert
func Convert(c *gin.Context) {
	var content string
	if err := c.Bind(&content); err == nil {
		result := services.HandleContent(content)
		c.PureJSON(http.StatusOK, result)
	} else {
		c.PureJSON(http.StatusInternalServerError, gin.Error{
			Err:  nil,
			Type: 0,
			Meta: nil,
		})
	}
}
