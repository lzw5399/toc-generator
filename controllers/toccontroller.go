package controllers

import (
	"fmt"
	"github.com/gin-gonic/gin"
	"github.com/satori/go.uuid"
	"net/http"
	"os"
	"toc-generator/services"
)

var uid = uuid.NewV4().String()

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

// Method: GET
// Url: /version
func Version(c *gin.Context) {
	buildNumber := os.Getenv("BUILD_NUMBER")
	if buildNumber == "" {
		buildNumber = "no build number available"
	}

	result := fmt.Sprintf("buildNumber: %s\r\n" +
		"uuid: %s", buildNumber, uid)

	c.String(http.StatusOK, result)
}
