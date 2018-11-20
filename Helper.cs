using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using PdfSharp.Pdf.IO;
using SelectPdf;
using Microsoft.Extensions.Logging;

namespace code
{

    public class DownloadTask
    {
        public string Url;
        public string FileName;
        public int Index;
        public bool IsPDF;
        public Stream Content;
    }

    public class Helper
    {

        protected ILogger logger;

        public Helper(ILogger _logger)
        {
            logger = _logger;
        }

        public async Task<Stream> GetCombineFromHtml(string HTMLString)
        {
            var job = new DownloadTask()
            {
                Url = "",
                FileName = GetUUID() + ".pdf",
                IsPDF = true,
                Index = 0,
                Content = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(HTMLString))
            };

            job = await ConvertPdfFromHTML(job);

            return job.Content;
        }

        public async Task<Stream> GetCombineFile(string[] urlList)
        {
            var taskList = new List<DownloadTask>();


            logger.LogDebug("begin assign job queue");

            for (var i = 0; i < urlList.Count(); i++)
            {
                taskList.Add(new DownloadTask()
                {
                    Url = urlList[i],
                    FileName = GetUUID() + ".pdf",
                    IsPDF = IsPdf(urlList[i]),
                    Index = i
                });
            }

            logger.LogDebug("begin handle job queue");
            await HandleFileList(taskList);

            logger.LogDebug("begin combine pdf");

            if (taskList.Count > 0)
            {
                var outPdf = PdfReader.Open(taskList[0].Content, PdfDocumentOpenMode.Import);

                if (taskList.Count == 1)
                {
                    logger.LogDebug("begin output pdf stream");
                    var st = taskList[0].Content;
                    st.Position = 0;
                    return st;
                }

                for (var i = 1; i < taskList.Count; i++)
                {
                    var doc = PdfReader.Open(taskList[i].Content, PdfDocumentOpenMode.Import);
                    for (var j = 0; j < doc.PageCount; j++)
                    {
                        outPdf.AddPage(doc.Pages[j]);
                    }
                }

                logger.LogDebug("begin output pdf stream");

                var stream = new MemoryStream();
                outPdf.Save(stream);

                return stream;
            }

            return new MemoryStream();
        }

        public async Task HandleFileList(List<DownloadTask> lists)
        {
            await Task.WhenAll(lists.Select(job =>
            {
                if (job.IsPDF)
                {
                    logger.LogDebug("begin download remote pdf[{index}]", job.Index);
                    return DownloadPdf(job);
                }

                logger.LogDebug("begin convert url to pdf[{index}]", job.Index);
                return ConvertPdf(job);
            }));
        }

        //Convert Pdf from URL
        public async Task<DownloadTask> ConvertPdf(DownloadTask job)
        {
            GlobalProperties.EnableFallbackToRestrictedRenderingEngine = true;
            GlobalProperties.EnableRestrictedRenderingEngine = true;

            var converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.WebPageWidth = 1240;

            // create a new pdf document converting an url
            var doc = converter.ConvertUrl(job.Url);
            var tmp = doc.Save();
            job.Content = new MemoryStream(tmp);

            logger.LogDebug("convert url to pdf[job index:{index}] complete", job.Index);

            return job;
        }

        //Convert Pdf from HTMLString
        public async Task<DownloadTask> ConvertPdfFromHTML(DownloadTask job)
        {
            GlobalProperties.EnableFallbackToRestrictedRenderingEngine = true;
            GlobalProperties.EnableRestrictedRenderingEngine = true;

            var converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.WebPageWidth = 1240;

            // create a new pdf document converting an url
            var reader = new StreamReader(job.Content);
            var doc = converter.ConvertHtmlString(reader.ReadToEnd());
            var tmp = doc.Save();
            job.Content = new MemoryStream(tmp);

            return job;
        }

        //Download Pdf from URL
        public async Task<DownloadTask> DownloadPdf(DownloadTask job)
        {
            using (var httpClient = new WebClient())
            {
                httpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                byte[] tmp = await httpClient.DownloadDataTaskAsync(job.Url);
                job.Content = new MemoryStream(tmp);
                logger.LogDebug("download pdf[job index:{index}] complete", job.Index);
                return job;
            }
        }

        public static string ToLocalFileURL(string path)
        {
            var uri = new Uri(path);
            return uri.AbsoluteUri;
        }

        public static bool IsLocal(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static bool IsPdf(string path)
        {
            var uri = new Uri(path);
            var ext = Path.GetExtension(uri.LocalPath).ToLower();
            return ext.Equals(".pdf");
        }

        public static string GetUUID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}