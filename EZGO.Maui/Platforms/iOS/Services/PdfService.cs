using System;
using CoreGraphics;
using Foundation;
using System.Diagnostics;
using UIKit;
using WebKit;
using EZGO.Maui.Core.Interfaces.File;

namespace EZGO.Maui.Platforms.iOS.Services
{
    public class PdfService : IPdfService
    {
        private const string category = "[PdfService]:\n\t";

        public string SaveHtmlToPdf(string html, string pdfFilename, Action callbackAction)
        {
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "pdf");
            string filename = $"{pdfFilename}.pdf";
            Debug.WriteLine("Created Directory paths", category);
            if (!File.Exists(folder))
                Directory.CreateDirectory(folder);

            int index = 0;
            string filePath = Path.Combine(folder, filename);

            while (File.Exists(filePath))
            {
                index++;
                filename = $"{pdfFilename}({index}).pdf";
                filePath = Path.Combine(folder, filename);
                Debug.WriteLine($"While loop {index}", category);
            }
            WKWebView webView = new WKWebView(new CGRect(0, 0, 512, 1024), new WKWebViewConfiguration())
            {
                UserInteractionEnabled = false,
                BackgroundColor = UIColor.White,
                Hidden = true,
                NavigationDelegate = new WebViewCallBack(filePath, callbackAction)
            };

            Debug.WriteLine("Created Web View", category);

            webView.LoadHtmlString(html, NSBundle.MainBundle.BundleUrl);

            Debug.WriteLine("Finished Pdf Creation", category);
            return filePath;
        }

        private class WebViewCallBack : WKNavigationDelegate
        {
            private readonly string filename;
            private readonly Action action;

            public WebViewCallBack(string filename, Action action)
            {
                this.filename = filename;
                this.action = action;
            }

            /// <param name="webView">To be added.</param>
            /// <param name="navigation">To be added.</param>
            /// <summary>Method that is called when all the data is loaded.</summary>
            /// <remarks>To be added.</remarks>
            public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
            {
                const int padding = 0;

                UIEdgeInsets pageMargins = new UIEdgeInsets(padding, padding, padding, padding);
                webView.ViewPrintFormatter.ContentInsets = pageMargins;

                UIPrintPageRenderer renderer = new UIPrintPageRenderer();
                renderer.AddPrintFormatter(webView.ViewPrintFormatter, 0);

                CGSize pageSize = new CGSize(512, 1024);
                CGRect printableRect = new CGRect(padding, padding, pageSize.Width - (padding * 2), pageSize.Height - (padding * 2));
                CGRect paperRect = new CGRect(0, 0, 512, 1024);

                NSString paperRectString = new NSString("PaperRect");
                NSString printableRectString = new NSString("PrintableRect");

                renderer.SetValueForKey(FromObject(paperRect), paperRectString);
                renderer.SetValueForKey(FromObject(printableRect), printableRectString);

                NSData file = PrintToPdfWithRenderer(renderer, paperRect);
                File.WriteAllBytes(filename, file.ToArray());

                action.Invoke();
            }

            private static NSData PrintToPdfWithRenderer(UIPrintPageRenderer renderer, CGRect paperRect)
            {
                NSMutableData pdfData = new NSMutableData();

                UIGraphics.BeginPDFContext(pdfData, paperRect, null);
                renderer.PrepareForDrawingPages(new NSRange(0, renderer.NumberOfPages));

                for (int index = 0; index < renderer.NumberOfPages; index++)
                {
                    UIGraphics.BeginPDFPage();
                    renderer.DrawPage(index, paperRect);
                }

                UIGraphics.EndPDFContext();

                return pdfData;
            }
        }


    }
}

