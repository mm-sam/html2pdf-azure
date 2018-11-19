using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SelectPdf;
using System.Net.Http;
using System.Net;

namespace code.Controllers
{

    [ApiController]
    public class PDFController : ControllerBase
    {
        [HttpGet("/api")]
        public ActionResult<String> Hello()
        {
            return "Hello";
        }

        [HttpGet("/api/pdf/fromlink")]
        public async Task<IActionResult> Link2PDF(string link)
        {
            GlobalProperties.EnableFallbackToRestrictedRenderingEngine = true;
            GlobalProperties.EnableRestrictedRenderingEngine = true;

            var converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.WebPageWidth = 1240;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertUrl(link);

            return File(doc.Save(), "application/pdf", "test.pdf");
        }
    }
}