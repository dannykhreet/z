using System.Windows.Input;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Controls;

public partial class ImageInput : ContentView
{
    public static string AddPhotoText = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.addPhotoTitle);
    public static string ChangePhotoText = Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.changePhotoTitle);

    public static readonly BindableProperty CurrentMediaItemProperty = BindableProperty.Create(
        nameof(CurrentMediaItem),
        typeof(MediaItem),
        typeof(ImageInput), null,
        defaultBindingMode: BindingMode.TwoWay);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(ImageInput),
        null);

    public static readonly BindableProperty ShowMediaOverlayProperty = BindableProperty.Create(
        nameof(ShowMediaOverlay),
        typeof(bool),
        typeof(ImageInput),
        true);

    public MediaItem CurrentMediaItem
    {
        get => (MediaItem)GetValue(CurrentMediaItemProperty);
        set
        {
            SetValue(CurrentMediaItemProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set
        {
            SetValue(CommandProperty, value);
            OnPropertyChanged();
        }
    }

    public bool ShowMediaOverlay
    {
        get => (bool)GetValue(ShowMediaOverlayProperty);
        set
        {
            SetValue(ShowMediaOverlayProperty, value);
            OnPropertyChanged();
        }
    }

    public ImageInput()
    {
        InitializeComponent();
    }
}
