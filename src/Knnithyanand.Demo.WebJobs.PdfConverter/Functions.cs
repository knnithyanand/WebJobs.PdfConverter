using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Knnithyanand.Demo.WebJobs.PdfConverter
{
    public class Functions
    {
        private static int desired_x_dpi = 300;
        private static int desired_y_dpi = 300;

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessPdfFiles(
            [QueueTrigger("workitems")] string pdfFilename,
            [Blob("pdf-files/{queueTrigger}.pdf", FileAccess.Read)] Stream pdfBlob,
            IBinder binder,
            ILogger logger)
        {
            logger.LogInformation($"Start Processing: {pdfFilename}");

            GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(@"gsdll32.dll");
            using (GhostscriptRasterizer _rasterizer = new GhostscriptRasterizer())
            {
                logger.LogInformation($"Open: {pdfFilename}");
                _rasterizer.Open(pdfBlob, gvi, true);
                for (int pageNumber = 1; pageNumber <= _rasterizer.PageCount; pageNumber++)
                {
                    string pngFilename = $"{pdfFilename}-page-{pageNumber.ToString()}.png".ToLower();
                    logger.LogInformation($"pageFilePath: {pngFilename}");

                    BlobAttribute blobAttribute = new BlobAttribute($"png-files/{pngFilename}");
                    CloudBlockBlob pngBlob = binder.Bind<CloudBlockBlob>(blobAttribute);

                    logger.LogInformation($"GetPage: {pageNumber}");
                    Image img = _rasterizer.GetPage(desired_x_dpi, desired_y_dpi, pageNumber);
                    logger.LogInformation($"Save: {pngFilename}");

                    MemoryStream ms = new MemoryStream();                    
                    img.Save(ms, ImageFormat.Png);
                    ms.Flush();
                    ms.Position = 0;
                    pngBlob.UploadFromStream(ms);
                }
            }

            logger.LogInformation($"Finish Processing: {pdfFilename}");
        }
    }
}
