using System;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Java.Interop;
using Java.IO;
using Microsoft.Maui.Controls;
using System.Reflection.Metadata;
using File = Java.IO.File;
using WebView = Android.Webkit.WebView;
using EZGO.Maui.Core.Interfaces.File;

namespace EZGO.Maui.Platforms.Android.Services
{
    public class PdfService : IPdfService
    {
        public string SaveHtmlToPdf(string html, string pdfFilename, Action callbackAction)
        {
            File directory = new File(FileSystem.Current.AppDataDirectory + "/pdf/");
            File file = new File(directory + "/" + pdfFilename + ".pdf");

            File tempHtml = File.CreateTempFile(pdfFilename, ".html");
            FileWriter writer = new FileWriter(tempHtml);
            writer.Write(html);
            writer.Close();

            if (!directory.Exists())
                directory.Mkdirs();

            int x = 0;
            while (file.Exists())
            {
                x++;
                file = new File(directory + "/" + pdfFilename + "(" + x + ").pdf");
            }


            file.CreateNewFile();
            string filename = file.ToString();

            WebView webView = new WebView(global::Android.App.Application.Context)
            {
                DrawingCacheEnabled = true
            };

            var settings = webView.Settings;
            settings.AllowFileAccess = true;
            settings.AllowFileAccessFromFileURLs = true;
            settings.AllowUniversalAccessFromFileURLs = true;
            settings.MixedContentMode = MixedContentHandling.AlwaysAllow;
            settings.DomStorageEnabled = true;
            settings.LoadsImagesAutomatically = true;

            webView.SetPadding(0, 0, 0, 0);
            webView.SetClipToPadding(true);
            webView.SetLayerType(LayerType.Software, null);
            webView.Layout(0, 0, 512, 1024);
            webView.LoadUrl("file://" + tempHtml.Path);
            webView.SetWebViewClient(new WebViewCallBack(filename, callbackAction));
            return filename;
        }

        class WebViewCallBack : WebViewClient
        {
            bool _complete;
            private readonly string filename;
            private readonly Action action;

            public WebViewCallBack(string filename, Action action)
            {
                this.filename = filename;
                this.action = action;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                if (!_complete)
                {
                    _complete = true;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPageLoaded(view);
                    });
                }
            }
            public override void OnLoadResource(WebView view, string url)
            {
                base.OnLoadResource(view, url);
                Device.StartTimer(TimeSpan.FromSeconds(10), () =>
                {
                    if (!_complete)
                        OnPageFinished(view, url);
                    return false;
                });
            }

            internal void OnPageLoaded(WebView webView)
            {
                try
                {
                    PrintAttributes.Builder builder = new PrintAttributes.Builder();
                    builder.SetMediaSize(PrintAttributes.MediaSize.IsoA4);
                    builder.SetMinMargins(PrintAttributes.Margins.NoMargins);
                    builder.SetResolution(new PrintAttributes.Resolution("pdf", "pdf", 300, 300));
                    PrintAttributes attributes = builder.Build();
                    PrintDocumentAdapter adapter = webView.CreatePrintDocumentAdapter(filename);
                    PdfLayoutResultCallback layoutResultCallback = new PdfLayoutResultCallback
                    {
                        Adapter = adapter,
                        Filename = filename,
                        Action = action
                    };
                    adapter.OnLayout(null, attributes, null, layoutResultCallback, null);
                }
                catch
                {
                }
            }
        }

        [Register("android/print/PdfLayoutResultCallback")]
        public class PdfLayoutResultCallback : PrintDocumentAdapter.LayoutResultCallback
        {
            public string Filename { get; set; }

            public Action Action { get; set; }

            public PrintDocumentAdapter Adapter { get; set; }

            public PdfLayoutResultCallback(IntPtr javaReference, JniHandleOwnership transfer)
                : base(javaReference, transfer) { }

            public PdfLayoutResultCallback() : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
            {
                if (!(Handle != IntPtr.Zero))
                {
                    unsafe
                    {
                        JniObjectReference val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                        SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                        JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                    }
                }

            }

            public override void OnLayoutFinished(PrintDocumentInfo info, bool changed)
            {
                try
                {
                    File file = new File(Filename);
                    ParcelFileDescriptor fileDescriptor = ParcelFileDescriptor.Open(file, ParcelFileMode.ReadWrite);
                    PdfWriteResultCallback writeResultCallback = new PdfWriteResultCallback(Action);
                    Adapter.OnWrite(new[] { PageRange.AllPages }, fileDescriptor, new CancellationSignal(), writeResultCallback);
                }
                catch
                {

                }

                base.OnLayoutFinished(info, changed);
            }
        }

        [Register("android/print/PdfWriteResult")]
        public class PdfWriteResultCallback : PrintDocumentAdapter.WriteResultCallback
        {
            private readonly Action action;

            public PdfWriteResultCallback(Action action, IntPtr javaReference, JniHandleOwnership transfer) : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
            {
                this.action = action;
            }

            public PdfWriteResultCallback(Action action) : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
            {
                if (!(Handle != IntPtr.Zero))
                {
                    unsafe
                    {
                        JniObjectReference val = JniPeerMembers.InstanceMethods.StartCreateInstance("()V", GetType(), null);
                        SetHandle(val.Handle, JniHandleOwnership.TransferLocalRef);
                        JniPeerMembers.InstanceMethods.FinishCreateInstance("()V", this, null);
                    }
                }

                this.action = action;
            }

            public override void OnWriteFinished(PageRange[] pages)
            {
                base.OnWriteFinished(pages);

                action.Invoke();
            }
        }
    }
}

