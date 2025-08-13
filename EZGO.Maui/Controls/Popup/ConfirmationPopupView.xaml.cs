using System.Windows.Input;
using EZGO.Maui.Core.Extensions;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popups
{
    public partial class ConfirmationPopupView : SfPopup
    {
        public string CancelButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_CANCEL"); }
        public string SubmitButtonText { get => TranslateExtension.GetValueFromDictionary("BASE_TEXT_OK"); }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(ConfirmationPopupView));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty CancelButtonCommandProperty = BindableProperty.Create(nameof(CancelButtonCommand), typeof(ICommand), typeof(ConfirmationPopupView));

        public ICommand CancelButtonCommand
        {
            get => (ICommand)GetValue(CancelButtonCommandProperty);
            set
            {
                SetValue(CancelButtonCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty OkButtonCommandProperty = BindableProperty.Create(nameof(OkButtonCommand), typeof(ICommand), typeof(ConfirmationPopupView));

        public ICommand OkButtonCommand
        {
            get => (ICommand)GetValue(OkButtonCommandProperty);
            set
            {
                SetValue(OkButtonCommandProperty, value);
                OnPropertyChanged();
            }
        }


        public ConfirmationPopupView()
        {
            InitializeComponent();
        }
    }
}
