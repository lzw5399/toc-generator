package main

import (
	"bufio"
	"fmt"
	linq "github.com/ahmetb/go-linq/v3"
	"github.com/gin-gonic/gin"
	"net/http"
	"strings"
)

func main() {
	router := gin.Default()
	router.StaticFile("/favicon.ico", "./assets/favicon.ico")

	router.LoadHTMLGlob("views/*")
	router.GET("/", Home)
	router.POST("/convert", Convert)

	_ = router.Run(":8081")
}

// 处理post
func Convert(c *gin.Context) {
	var content string
	if c.Bind(&content) == nil {
		handleContent(content)
	} else {
		c.PureJSON(http.StatusInternalServerError, gin.Error{
			Err:  nil,
			Type: 0,
			Meta: nil,
		})
	}
}

var unorderedList = [3]string{"- ", "+ ", "* "}

func handleContent(content string) {
	//var insertStr string
	//var orgStr string
	//lastStatus := -1
	//currentStatus := -1
	//headlineCounter := 0
	isCode := false
	isUnorderedlist := false

	reader := bufio.NewReader(strings.NewReader(content))
	for {
		bytes, _, _ := reader.ReadLine()
		if bytes == nil {
			break
		}
		line := string(bytes)

		if strings.TrimSpace(line) == "[TOC]" {
			line = ""
		}

		if len(line) >= 3 && line[:3] == "```" {
			isCode = !isCode
		}

		existUnorderList := linq.From(unorderedList).AnyWith(func(i interface{}) bool {
			return i == "- "
		})

		if len(strings.TrimLeft(line, " ")) >= 2 && existUnorderList {
			isUnorderedlist = true
		}

		if !isCode && !isUnorderedlist {
			line = strings.TrimLeft(line, " ")
		}

		ls := strings.Split(line, " ")

		if len(ls) > 1 {

		}

		fmt.Println("当前是：", line, isUnorderedlist)

	}
}

// 处理get
func Home(c *gin.Context) {
	c.HTML(http.StatusOK, "index.html", gin.H{
		"title": "hello a lzw",
	})
}
