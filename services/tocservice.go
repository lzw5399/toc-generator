package services

import (
	"bufio"
	"fmt"
	"github.com/ahmetb/go-linq/v3"
	"strings"
)

var unorderedList = [3]string{"- ", "+ ", "* "}
var headlineMap = map[string]int{
	"#":     0,
	"##":    1,
	"###":   2,
	"####":  3,
	"#####": 4,
}
var indentMap = map[int]int{
	0: -1,
	1: -1,
	2: -1,
	3: -1,
	4: -1,
	5: -1,
	6: -1,
}

func HandleContent(content string) string {
	var insertStr string
	var orgStr string
	lastStatus := -1
	currentStatus := -1
	headlineCounter := 0
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
		existHeadline := linq.From(headlineMap).AnyWith(func(i interface{}) bool {
			return i.(linq.KeyValue).Value == ls[0]
		})
		if len(ls) > 1 && existHeadline && !isCode && !isUnorderedlist {
			headlineCounter += 1
			currentStatus = headlineMap[ls[0]]
			// find first rank headline
			if lastStatus == -1 || currentStatus == 0 || indentMap[currentStatus] == 0 {
				// init indent
				for k := range indentMap {
					indentMap[k] = -1
				}

				indentMap[currentStatus] = 0
			} else if currentStatus > lastStatus {
				indentMap[currentStatus] = indentMap[lastStatus] + 1
			}

			// update headline text
			headtext := strings.Join(ls[1:len(ls)-1], " ")
			if ls[len(ls)-1] == "\n" || ls[len(ls)-1] == "\r\n" {
				headtext += " \n"
			} else {
				headtext += " " + ls[len(ls)-1]
			}

			headid := fmt.Sprintf("head%d", headlineCounter)
			headline := fmt.Sprintf("%s <span id=\"%s\">%s</span>\n", ls[0], headid, headtext)
			orgStr += headline

			jumpStr := fmt.Sprintf("- [%s](#head%d)", headtext, headlineCounter)

			tempp := ""
			if indentMap[currentStatus] >= 0 {
				for i := 0; i < indentMap[currentStatus]; i++ {
					tempp += "\t"
				}
			}
			insertStr += tempp + jumpStr + "\n"

			lastStatus = currentStatus
		} else {
			orgStr += line
		}

	}

	return insertStr + orgStr
}