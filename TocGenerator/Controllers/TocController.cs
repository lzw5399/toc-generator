using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TocGenerator.Controllers
{
    public class TocController : Controller
    {
        private static Guid uuid = Guid.NewGuid();
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
        private readonly string[] _unorderedList = new string[] { "- ", "+ ", "* " };

        public IActionResult Converter()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Converter([FromBody] string text)
        {
            var insertStr = new StringBuilder();
            var orgStr = new StringBuilder();
            var lastStatus = -1;
            var currentStatus = -1;
            var headlineCounter = 0;
            var iscode = false;
            var isUnorderedlist = false;
            
            using (var sr = new StringReader(text))
            {
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (line.Trim() == "[TOC]")
                        line = string.Empty;

                    if (line.Length >= 3 && line.Substring(0, 3) == "```")
                        iscode = !iscode;

                    if (line.TrimStart().Length >= 2 && _unorderedList.Contains(line.TrimStart().Substring(0, 2)))
                        isUnorderedlist = true;

                    if (!iscode && !isUnorderedlist)
                        line = line.TrimStart();

                    var ls = line.Split(' ');

                    if (ls.Length > 1 && _headlineDic.Keys.Contains(ls[0]) && !iscode && !isUnorderedlist)
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
                        orgStr.Append(headline);

                        var jumpStr = $"- [{headtext}](#head{headlineCounter})";

                        var tempp = string.Empty;
                        if (_indentDic[currentStatus] >= 0)
                        {
                            for (int i = 0; i < _indentDic[currentStatus]; i++)
                            {
                                tempp += "\t";
                            }
                        }

                        insertStr.Append(tempp + jumpStr + "\n");

                        lastStatus = currentStatus;
                    }
                    else
                    {
                        orgStr.AppendLine(line);
                    }

                    isUnorderedlist = false;
                }
            }

            return Json(insertStr.ToString() + orgStr.ToString());
        }

        [HttpGet("version")]
        public ActionResult Version()
        {
            var buildNumber = Environment.GetEnvironmentVariable("BUILD_NUMBER") ?? "no build number avaliable";
            var content = $"buildNumber: {buildNumber}{Environment.NewLine}uuid: {uuid}";

            return Content(content);
        }
    }
}