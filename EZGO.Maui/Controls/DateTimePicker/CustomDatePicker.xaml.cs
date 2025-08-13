using EZGO.Maui.Core.Extensions;
using NodaTime;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Controls.DateTimePicker;

public partial class CustomDatePicker : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(TextProperty), typeof(string), typeof(CustomDatePicker), propertyChanged: OnTextPropertyChanged);
    public static readonly BindableProperty PickerHeightProperty = BindableProperty.Create(nameof(PickerHeightProperty), typeof(double), typeof(CustomDatePicker), defaultValue: 20.0, propertyChanged: OnPickerHeightPropertyChanged);

    private static void OnPickerHeightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CustomDatePicker datePicker)
        {
            datePicker.entry.HeightRequest = datePicker.PickerHeight;
        }
    }

    public static readonly BindableProperty FormatStringProperty = BindableProperty.Create(nameof(FormatStringProperty), typeof(string), typeof(CustomDatePicker), defaultValue: "dd-MM-yyyy");

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomDatePicker;
        obj.label.Text = obj.Text;
    }

    public static readonly BindableProperty PickedDateProperty = BindableProperty.Create(nameof(PickedDateProperty), typeof(LocalDateTime), typeof(CustomDatePicker), propertyChanged: OnPickedDatePropertyChanged, defaultBindingMode: BindingMode.TwoWay);
    public static readonly BindableProperty MinimumDateProperty = BindableProperty.Create(nameof(MinimumDateProperty), typeof(DateTime), typeof(CustomDatePicker), propertyChanged: OnMinimumDatePropertyChanged, defaultValue: DateTime.Now.AddYears(-10));
    public static readonly BindableProperty MaximumDateProperty = BindableProperty.Create(nameof(MaximumDateProperty), typeof(DateTime), typeof(CustomDatePicker), propertyChanged: OnMaximumDatePropertyChanged, defaultValue: DateTime.Now.AddDays(-1));

    private static void OnMaximumDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomDatePicker;
        obj.picker.MaximumDate = obj.MaximumDate;
    }

    private static void OnMinimumDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomDatePicker;
        if (obj.MinimumDate == DateTime.Today)
        {
            obj.picker.MaximumDate = obj.MinimumDate.AddYears(10);
        }
        obj.picker.MinimumDate = obj.MinimumDate;
    }

    private static void OnPickedDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomDatePicker;
        obj.entry.Text = obj.PickedDate.ToString(obj.FormatString, null);
        obj.picker.Date = obj.PickedDate.ToDateTimeUnspecified();
    }

    #endregion

    public CustomDatePicker()
    {
        InitializeComponent();
        picker.MinimumDate = DateTime.Now.AddYears(-10);
        picker.MaximumDate = DateTime.Now.AddDays(-1);
    }

    #region Properties

    public double PickerHeight
    {
        get => (double)GetValue(PickerHeightProperty);
        set
        {
            SetValue(PickerHeightProperty, value);
            OnPropertyChanged();
        }
    }

    public string FormatString
    {
        get => (string)GetValue(FormatStringProperty);
        set
        {
            SetValue(FormatStringProperty, value);
            OnPropertyChanged();
        }
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            OnPropertyChanged();
        }
    }

    private DateTime Date { get; set; }

    public LocalDateTime PickedDate
    {
        get => (LocalDateTime)GetValue(PickedDateProperty);
        set
        {
            SetValue(PickedDateProperty, value);
            OnPropertyChanged();
        }
    }

    public DateTime MinimumDate
    {
        get => (DateTime)GetValue(MinimumDateProperty);
        set
        {
            SetValue(MinimumDateProperty, value);
            OnPropertyChanged();
        }
    }

    public DateTime MaximumDate
    {
        get => (DateTime)GetValue(MaximumDateProperty);
        set
        {
            SetValue(MaximumDateProperty, value);
            OnPropertyChanged();
        }
    }

    #endregion

    #region Events

    private void TapGestureRecognizer_Tapped(System.Object sender, System.EventArgs e) => picker.Focus();

    private void Picker_DateSelected(object sender, DateChangedEventArgs e)
    {
        if (sender is DatePicker datePicker && PickedDate != DateTimeHelper.MinValue)
        {
            Date = datePicker.Date;
            OnPropertyChanged(nameof(Date));
            PickedDate = Settings.ConvertDateTimeToLocal(Date);
            entry.Text = PickedDate.ToString(FormatString, null);
        }
    }

    #endregion
}
