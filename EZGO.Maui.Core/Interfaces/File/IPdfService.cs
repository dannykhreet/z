using System;

namespace EZGO.Maui.Core.Interfaces.File
{
    public interface IPdfService
    {
        string SaveHtmlToPdf(string html, string pdfFilename, Action callbackAction);
    }
}
