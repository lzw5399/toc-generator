using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using TocGenerator.Extensions;

namespace TocGenerator.Controllers
{
    public class TocController : Controller
    {
        private readonly object _locker = new object();

        // GET
        public IActionResult Converter()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Converter([FromBody] string text)
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