using System.Windows.Input;

namespace EZGO.Maui.Views;

public partial class PdfViewerControl : ContentView
{
    public PdfViewerControl()
    {
        InitializeComponent();
        viewer.ShowScrollHead = false;
        //viewer.LoadDocument(null);
    }

    public readonly static BindableProperty PdfFileStreamProperty = BindableProperty.Create(nameof(PdfFileStreamProperty), typeof(Stream), typeof(PdfViewerControl), propertyChanged: OnPdfStreamPropertyChanged);

    private async static void OnPdfStreamPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var pdfViewer = bindable as PdfViewerControl;
        if (pdfViewer.PdfFileStream != null)
        {
            MemoryStream stream = new();
            pdfViewer.PdfFileStream.Seek(0, SeekOrigin.Begin);
            await pdfViewer.PdfFileStream.CopyToAsync(stream);

            pdfViewer.viewer.ShowScrollHead = false;
            pdfViewer.viewer.LoadDocument(stream);
        }
    }

    public Stream PdfFileStream
    {
        get => (Stream)GetValue(PdfFileStreamProperty);
        set
        {
            SetValue(PdfFileStreamProperty, value);
            OnPropertyChanged();
        }
    }


    public readonly static BindableProperty ZoomProperty = BindableProperty.Create(nameof(Zoom), typeof(float), typeof(PdfViewerControl), defaultValue: 100.0f);
    public float Zoom
    {
        get => (float)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public static readonly BindableProperty SwipeLeftCommandProperty = BindableProperty.Create(nameof(SwipeLeftCommand), typeof(ICommand), typeof(PdfViewerControl), propertyChanged: OnSwipeLeftPropertyChanged);

    private static void OnSwipeLeftPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var pdfViewer = bindable as PdfViewerControl;
        pdfViewer.viewer.SwipeLeftCommand = (ICommand)newValue;
    }

    private static void OnSwipeRightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var pdfViewer = bindable as PdfViewerControl;
        pdfViewer.viewer.SwipeRightCommand = (ICommand)newValue;
    }

    public ICommand SwipeLeftCommand
    {
        get => (ICommand)GetValue(SwipeLeftCommandProperty);
        set => SetValue(SwipeLeftCommandProperty, value);
    }

    public static readonly BindableProperty SwipeRightCommandProperty = BindableProperty.Create(nameof(SwipeRightCommand), typeof(ICommand), typeof(PdfViewerControl), propertyChanged: OnSwipeRightPropertyChanged);

    public ICommand SwipeRightCommand
    {
        get => (ICommand)GetValue(SwipeRightCommandProperty);
        set => SetValue(SwipeRightCommandProperty, value);
    }
}
