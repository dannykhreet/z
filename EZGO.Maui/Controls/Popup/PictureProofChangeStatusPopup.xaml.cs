using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class PictureProofChangeStatusPopup : SfPopup
{
	public PictureProofChangeStatusPopup()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty CanChangePhotosProperty = BindableProperty.Create(
          nameof(CanChangePhotos),
          typeof(bool),
          typeof(PictureProofChangeStatusPopup), defaultValue: true);

    public static readonly BindableProperty CancelButtonCommandProperty = BindableProperty.Create(
       nameof(CancelButtonCommand),
       typeof(ICommand),
       typeof(PictureProofChangeStatusPopup));

    public static readonly BindableProperty RemoveButtonCommandProperty = BindableProperty.Create(
        nameof(RemoveButtonCommand),
        typeof(ICommand),
        typeof(PictureProofChangeStatusPopup));

    public static readonly BindableProperty KeepButtonCommandProperty = BindableProperty.Create(
      nameof(KeepButtonCommand),
      typeof(ICommand),
      typeof(PictureProofChangeStatusPopup));

    public bool CanChangePhotos
    {
        get => (bool)GetValue(CanChangePhotosProperty);
        set
        {
            SetValue(CanChangePhotosProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand CancelButtonCommand
    {
        get => (ICommand)GetValue(CancelButtonCommandProperty);
        set
        {
            SetValue(CancelButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand RemoveButtonCommand
    {
        get => (ICommand)GetValue(RemoveButtonCommandProperty);
        set
        {
            SetValue(RemoveButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public ICommand KeepButtonCommand
    {
        get => (ICommand)GetValue(KeepButtonCommandProperty);
        set
        {
            SetValue(KeepButtonCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public string CancelButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_CANCEL"); }
    public string EditAndChangeButtonText { get => TranslateExtension.GetValueFromDictionary("PICTURE_PROOF_EDIT_CHANGE"); }
    public string KeepButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_OK"); }
}
