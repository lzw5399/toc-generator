using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TocGenerator.Extensions;

namespace TocGenerator.Controllers
{
    public class TocController : Controller
    {
        private readonly object _locker = new object();
        private readonly Dictionary<string, int> _headlineDic = new Dictionary<string, int>
        {
            { "#", 0 },
            { "##", 1 },
            { "###", 2 },
            { "####", 3 },
            { "#####", 4 }
        };
        private Dictionary<int, int> _indentDic = new Dictionary<int, int>
        {
            { 0, -1 },
            { 1, -1 },
            { 2, -1 },
            { 3, -1 },
            { 4, -1 },
            { 5, -1 },
            { 6, -1 }
        };

        // GET
        public IActionResult Converter()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConverterPy([FromBody] string text)
        {
            try
            {
                // 删除toc
                if (text.StartsWith("[TOC]"))
                {
                    text = text.Substring(5);
                }

                var path = Path.Combine(AppContext.BaseDirectory, "Scripts");
                DeleteMarkdowns(path);

                using (var fs = new FileStream(Path.Combine(path, "temp.md"), FileMode.Create))
                {
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(text);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                    fs.Close();
                }

                "bash ./run.sh".Bash(path);

                string result;
                // 读取转换之后的
                using (var sr = new StreamReader(Path.Combine(path, "temp_with_toc.md")))
                {
                    result = sr.ReadToEnd();
                    sr.Close();
                }

                DeleteMarkdowns(path);

                return Json(result);
            }
            catch (Exception e)
            {
                return Json("转换失败，请检查输入markdown本身");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Converter([FromBody] string text)
        {
            var insertStr = string.Empty;
            var orgStr = string.Empty;
            var lastStatus = -1;
            var currentStatus = -1;
            var headlineCounter = 0;
            var iscode = false;

            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (line.Length >= 3 && line.Substring(0, 3) == "```")
                        iscode = !iscode;

                    if (!iscode)
                        line = line.TrimStart();

                    var ls = line.Split(' ');

                    if (ls.Length > 1 && _headlineDic.Keys.Contains(ls[0]) && !iscode)
                    {
                        headlineCounter += 1;
                        currentStatus = _headlineDic[ls[0]];
                        // find first rank headline
                        if (lastStatus == -1 || currentStatus == 0 || _indentDic[currentStatus] == 0)
                        {
                            // init indent
                            _indentDic = _indentDic.ToDictionary(it => it.Key, it => -1);

                            _indentDic[currentStatus] = 0;
                        }
                        else if (currentStatus > lastStatus)
                        {
                            _indentDic[currentStatus] = _indentDic[lastStatus] + 1;
                        }

                        // update headline text
                        var headtext = string.Join(' ', ls.Skip(1).SkipLast(1));
                        if (ls.LastOrDefault() == "\n" || ls.LastOrDefault() == "\r\n")
                        {
                            headtext += (" " + Environment.NewLine);
                        }
                        else
                        {
                            headtext += (" " + ls.LastOrDefault());
                        }

                        var headid = $"head{headlineCounter}";
                        var headline = $"{ls[0]} <span id=\"{headid}\">{headtext}</span>\n";
                        orgStr += headline;

                        var jumpStr = $"- [{headtext}](#head{headlineCounter})";

                        var tempp = string.Empty;
                        if (_indentDic[currentStatus] >= 0)
                        {
                            for (int i = 0; i < _indentDic[currentStatus]; i++)
                            {
                                tempp += "\t";
                            }
                        }

                        insertStr += (tempp + jumpStr + "\n");

                        lastStatus = currentStatus;
                    }
                    else
                    {
                        orgStr += line;
                    }
                }
            }

            return Json(insertStr + orgStr);
        }

        private void DeleteMarkdowns(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            var tempFiles = directoryInfo.GetFiles("*.md");

            if (!tempFiles.Any()) return;

            lock (_locker)
            {
                foreach (var file in tempFiles)
                {
                    file.Delete();
                }
            }
        }
    }
}