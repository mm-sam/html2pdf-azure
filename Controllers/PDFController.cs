using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net;
using System.IO;
using Microsoft.Extensions.Logging;

namespace code.Controllers
{


    [ApiController]
    public class PDFController : ControllerBase
    {
        protected Helper helper;

        public PDFController(ILogger<PDFController> logger)
        {
            helper = new Helper(logger);
        }

        [HttpGet("/")]
        public IActionResult Hello()
        {
            return Redirect("/index.html");
        }

        /// <summary>
        ///  合并URL地址为PDF
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("/link/combine")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> LinkCombine([FromForm] string[] file)
        {
            var stream = await helper.GetCombineFile(file);

            return File(stream, "application/pdf", Helper.GetUUID() + ".pdf");
        }

        /// <summary>
        ///  合并URL地址为PDF, `/link/combine` 的别名
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("/combine")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Combine([FromForm] string[] file)
        {
            var stream = await helper.GetCombineFile(file);

            return File(stream, "application/pdf", Helper.GetUUID() + ".pdf");
        }

        /// <summary>
        ///  将网址转换成PDF
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        [HttpPost("/linkpdf")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> LinkPdf([FromForm] string link)
        {
            var list = new string[] { link };
            var stream = await helper.GetCombineFile(list);

            return File(stream, "application/pdf", Helper.GetUUID() + ".pdf");
        }

        /// <summary>
        /// 将HTML内容转换成PDF
        /// </summary>
        /// <param name="upload"></param>
        /// <returns></returns>
        [HttpPost("/htmlpdf")]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> HtmlPdf([FromForm] string upload)
        {
            var stream = await helper.GetCombineFromHtml(upload);

            return File(stream, "application/pdf", Helper.GetUUID() + ".pdf");
        }
    }
}