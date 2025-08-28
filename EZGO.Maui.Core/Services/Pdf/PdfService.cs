using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Services.Pdf
{
    public class PdfService : Interfaces.Pdf.IPdfService
    {
        private readonly HttpClient httpClient;
        private readonly IFileService fileService;
        private const string pdfDirectory = "pdf";

        public PdfService()
        {
            httpClient = Statics.AWSS3MediaHttpClient;
            httpClient.Timeout = TimeSpan.FromSeconds(100);
            fileService = DependencyService.Get<IFileService>();
        }

        public async Task<Stream> GetPfdAsync(string uri)
        {
            try
            {
                var uriParts = uri.Split("/");
                var filename = uriParts.Last();

                var file = await fileService.ReadFromInternalStorageAsStreamAsync(filename, pdfDirectory);
                if (file != null) return file;

                var response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();

                    fileService.SaveFileToInternalStorage(content, filename, pdfDirectory);

                    return new MemoryStream(content);
                }

                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        public void Dispose()
        {
            httpClient.Dispose();
            //fileService.
        }
    }
}
