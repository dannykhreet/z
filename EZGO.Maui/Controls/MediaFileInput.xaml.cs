using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls;

public partial class MediaFileInput : SfButton
{
    public static string AddPhotoText = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.addPhotoTitle);
    public static string ChangePhotoText = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.changePhotoTitle);

    public static readonly BindableProperty CurrentMediaItemProperty = BindableProperty.Create(
        nameof(CurrentMediaItem),
        typeof(MediaItem),
        typeof(MediaFileInput), null,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty ShowMediaOverlayProperty = BindableProperty.Create(
        nameof(ShowMediaOverlay),
        typeof(bool),
        typeof(MediaFileInput),
        true);

    public static readonly BindableProperty PDFStreamProperty = BindableProperty.Create(
        nameof(ShowMediaOverlay),
        typeof(Stream),
        typeof(MediaFileInput),
        defaultValue: null);

    public Stream PDFStream
    {
        get => (Stream)GetValue(PDFStreamProperty);
        set
        {
            SetValue(PDFStreamProperty, value);
            OnPropertyChanged(nameof(PDFStream));
        }
    }

    public MediaItem CurrentMediaItem
    {
        get => (MediaItem)GetValue(CurrentMediaItemProperty);
        set
        {
            SetValue(CurrentMediaItemProperty, value);
            OnPropertyChanged(nameof(CurrentMediaItem));
        }
    }


    public bool ShowMediaOverlay
    {
        get => (bool)GetValue(ShowMediaOverlayProperty);
        set
        {
            SetValue(ShowMediaOverlayProperty, value);
            OnPropertyChanged(nameof(ShowMediaOverlay));
        }
    }

    public MediaFileInput()
    {
        InitializeComponent();
    }
}
