using System.Windows.Input;

namespace EZGO.Maui.Controls.DateTimePicker;

public partial class TranslatedDatePicker : DatePicker
{
    public static readonly BindableProperty OkClickedCommandProperty = BindableProperty.Create(nameof(OkClickedCommand), typeof(ICommand), typeof(TranslatedDatePicker));
    public ICommand OkClickedCommand
    {
        get => (ICommand)GetValue(OkClickedCommandProperty);
        set => SetValue(OkClickedCommandProperty, value);
    }

    public void OnPickerClosed()
    {
        OkClickedCommand?.Execute(Date);
    }

    public TranslatedDatePicker()
    {
        InitializeComponent();
    }
}
