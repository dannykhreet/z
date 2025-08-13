using System.Windows.Input;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popups
{
    public partial class EditDeletePopupView : SfPopup
    {
        public static readonly BindableProperty EditButtonTextProperty = BindableProperty.Create(nameof(EditButtonText), typeof(string), typeof(EditDeletePopupView));

        public string EditButtonText
        {
            get => (string)GetValue(EditButtonTextProperty);
            set
            {
                SetValue(EditButtonTextProperty, value);
                OnPropertyChanged();
            }
        }


        public static readonly BindableProperty DeleteButtonTextProperty = BindableProperty.Create(nameof(DeleteButtonText), typeof(string), typeof(EditDeletePopupView));

        public string DeleteButtonText
        {
            get => (string)GetValue(DeleteButtonTextProperty);
            set
            {
                SetValue(DeleteButtonTextProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty EditButtonCommandProperty = BindableProperty.Create(nameof(EditButtonCommand), typeof(ICommand), typeof(EditDeletePopupView));

        public ICommand EditButtonCommand
        {
            get => (ICommand)GetValue(EditButtonCommandProperty);
            set
            {
                SetValue(EditButtonCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty DeleteButtonCommandProperty = BindableProperty.Create(nameof(DeleteButtonCommand), typeof(ICommand), typeof(EditDeletePopupView));

        public ICommand DeleteButtonCommand
        {
            get => (ICommand)GetValue(DeleteButtonCommandProperty);
            set
            {
                SetValue(DeleteButtonCommandProperty, value);
                OnPropertyChanged();
            }
        }

        public EditDeletePopupView()
        {
            InitializeComponent();
        }
    }
}
