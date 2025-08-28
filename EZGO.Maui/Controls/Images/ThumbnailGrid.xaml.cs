using System.Diagnostics;
using EZGO.Maui.Core;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.Navigation;
using EZGO.Maui.Core.Interfaces.Pdf;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.ViewModels.Feed;
using EZGO.Maui.Core.ViewModels.Shared;
using EZGO.Maui.Views;

namespace EZGO.Maui.Controls.Images;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ThumbnailGrid : ContentView
{
    public ThumbnailGrid()
    {
        InitializeComponent();
    }

    public static BindableProperty ThumbnailsProperty = BindableProperty.Create(
        nameof(Thumbnails),
        typeof(List<MediaItem>),
        typeof(ThumbnailGrid),
        propertyChanged: ThumbnailsProperty_Changed);

    /// <summary>
    /// List of thumbnails to display in the view
    /// </summary>
    public List<MediaItem> Thumbnails
    {
        get => (List<MediaItem>)GetValue(ThumbnailsProperty);
        set => SetValue(ThumbnailsProperty, value);
    }

    public static BindableProperty IsDetailEnabledProperty = BindableProperty.Create(
        nameof(IsDetailEnabled),
        typeof(bool),
        typeof(ThumbnailGrid),
        propertyChanged: ThumbnailsProperty_Changed);

    public bool IsDetailEnabled
    {
        get => (bool)GetValue(IsDetailEnabledProperty);
        set => SetValue(IsDetailEnabledProperty, value);
    }

    private static async void ThumbnailsProperty_Changed(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ThumbnailGrid control)
        {
            if (control.mainGrid == null) return;

            if (!(newValue is List<MediaItem> newMedia) || newMedia == null || newMedia.Count == 0)
            {
                control.mainGrid.IsVisible = false;
                return;
            }

            else
            {
                var MediaCount = newMedia.Count;

                // Fill the images with the urls
                await control.FillImagesUrls();

                // We only need second row if we have more than 2 elements
                control.secondRowDef.Height = MediaCount > 2 ? GridLength.Star : 0;

                // We only need second column if we have more then 1 element
                control.secondColDef.Width = MediaCount > 1 ? GridLength.Star : 0;

                // When we have 3 items
                if (MediaCount == 3)
                {
                    // Set the first image to full height
                    Grid.SetRowSpan(control.image1, 2);
                    Grid.SetRowSpan(control.pdf1, 2);
                    Grid.SetRowSpan(control.grid1, 2);
                    // And move the third image to the second column
                    Grid.SetColumn(control.image3, 1);
                    Grid.SetColumn(control.pdf3, 1);
                    Grid.SetColumn(control.grid3, 1);

                    control.grid4.IsVisible = false;
                }
                // Otherwise
                else
                {
                    // Restore default placement
                    Grid.SetRowSpan(control.image1, 1);
                    Grid.SetRowSpan(control.pdf1, 1);
                    Grid.SetRowSpan(control.grid1, 1);
                    Grid.SetColumn(control.image3, 0);
                    Grid.SetColumn(control.pdf3, 0);
                    Grid.SetColumn(control.grid3, 0);
                }

                // Determine how many items are not showed
                var moreNumber = Math.Max(0, MediaCount - 4);

                // No more items
                if (moreNumber == 0)
                {
                    // Hide the text label
                    control.additionalPhotosLabel.IsVisible = false;
                }
                // Additional items
                else
                {
                    // Show the text label
                    control.additionalPhotosLabel.Text = $"+{moreNumber}";
                    control.additionalPhotosLabel.IsVisible = true;
                }

                // Sow the main grid
                control.mainGrid.IsVisible = true;
            }
        }
    }

    private async Task FillImagesUrls()
    {
        if (Thumbnails == null || Thumbnails.Count == 0)
            return;

        using var scope = App.Container.CreateScope();
        var pdfService = scope.ServiceProvider.GetService<IPdfService>();
        if (pdfService == null)
            return; // fail fast if service is missing

        static async Task SetItems(MediaItem item, CustomImage customImage, PdfViewerControl pdfViewerControl, IPdfService pdfService, Button playButton)
        {
            if (item == null)
            {
                customImage.IsVisible = false;
                pdfViewerControl.IsVisible = false;
                return;
            }

            if (item.PictureUrl?.ToLower().EndsWith(".pdf") ?? false)
            {
                var fullDocumentUri = string.Format(Core.Utils.Constants.MediaBaseUrl, item.PictureUrl);
                var stream = await pdfService.GetPfdAsync(fullDocumentUri);
                item.FileStream = stream;
                pdfViewerControl.PdfFileStream = stream;
                pdfViewerControl.IsVisible = true;
                customImage.IsVisible = false;
                playButton.IsVisible = false;
            }
            else
            {
                if (item.IsVideo)
                    playButton.IsVisible = true;
                else
                    playButton.IsVisible = false;


                customImage.IsLocalFile = item.IsLocalFile;
                customImage.ImageUrl = item.PictureUrl;
                customImage.IsVisible = true;
                pdfViewerControl.IsVisible = false;
            }
        }

        try
        {
            var item = Thumbnails.ElementAtOrDefault(0);
            await SetItems(item, image1, pdf1, pdfService, playBtn1);

            item = Thumbnails.ElementAtOrDefault(1);
            await SetItems(item, image2, pdf2, pdfService, playBtn2);

            item = Thumbnails.ElementAtOrDefault(2);
            await SetItems(item, image3, pdf3, pdfService, playBtn3);

            item = Thumbnails.ElementAtOrDefault(3);
            await SetItems(item, image4, pdf4, pdfService, playBtn4);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }

    }
    private async Task NavigateToDetailAsync(int index)
    {
        using var scope = App.Container.CreateScope();
        var itemsDetailViewModel = scope.ServiceProvider.GetService<ItemsDetailViewModel>();
        var navigationService = scope.ServiceProvider.GetService<INavigationService>();
        var items = new List<ThumbnailGridDetailModel>();
        Thumbnails.ForEach(async x =>
        {
            if (!x.IsVideo)
            {
                items.Add(new ThumbnailGridDetailModel
                {
                    Picture = x.PictureUrl,
                    PDFStream = x.PictureUrl?.ToLower().EndsWith(".pdf") ?? false ? await x.GetFileStream() : null,
                });
            }
            else
            {
                items.Add(new ThumbnailGridDetailModel
                {
                    Video = x.VideoUrl
                });
            }
        });
        itemsDetailViewModel.Items = new List<Core.Interfaces.Utils.IDetailItem>(items);
        itemsDetailViewModel.SenderClassName = nameof(FeedViewModel);
        itemsDetailViewModel.SelectedItem = items[index];
        await navigationService.NavigateAsync(viewModel: itemsDetailViewModel);
    }

    void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e)
    {
        if (IsDetailEnabled)
        {
            var index = ((TappedEventArgs)e).Parameter;

            _ = NavigateToDetailAsync(Convert.ToInt32(index));
        }
    }
}
