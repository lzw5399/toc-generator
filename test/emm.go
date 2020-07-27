package main

import (
	"fmt"
)

var indentMap = map[int]int{
	0: -1,
	1: -1,
	2: -1,
	3: -1,
	4: -1,
	5: -1,
	6: -1,
}

var arr = [5]int{0, 1, 2, 3, 4}

func main() {
	str := fmt.Sprintf("a:%v", "why")

	fmt.Println(str)
}
