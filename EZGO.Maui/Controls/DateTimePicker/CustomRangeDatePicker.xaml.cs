using NodaTime;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Controls.DateTimePicker;

public partial class CustomRangeDatePicker : ContentView, IDisposable
{
    public static readonly BindableProperty FromDateProperty = BindableProperty.Create(nameof(FromDateProperty), typeof(LocalDateTime), typeof(CustomRangeDatePicker), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnFromDatePropertyChanged);

    public static readonly BindableProperty ToDateProperty = BindableProperty.Create(nameof(ToDateProperty), typeof(LocalDateTime), typeof(CustomRangeDatePicker), defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnToDatePropertyChanged);

    private static void OnToDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomRangeDatePicker;
        obj.toDate.Date = obj.ToDate.ToDateTimeUnspecified();
        obj.toDateEntry.Text = obj.ToDate.ToString("dd-MM-yyyy", null);
    }

    private static void OnFromDatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as CustomRangeDatePicker;
        obj.fromDate.Date = obj.FromDate.ToDateTimeUnspecified();
        obj.fromDateEntry.Text = obj.FromDate.ToString("dd-MM-yyyy", null);
    }

    private void UpdateDateRanges()
    {
        fromDate.MaximumDate = ToDate.ToDateTimeUnspecified().AddDays(-1);
        toDate.MinimumDate = FromDate.ToDateTimeUnspecified().AddDays(1);
    }

    public CustomRangeDatePicker()
    {
        InitializeComponent();

        fromDate.Date = DateTime.Now.AddDays(-2);
        toDate.Date = DateTime.Now.AddDays(-1);
        fromDate.MaximumDate = fromDate.Date;
        fromDate.MinimumDate = DateTime.Now.AddYears(-10);
        toDate.MaximumDate = DateTime.Now.AddDays(-1);
        toDate.MinimumDate = fromDate.MaximumDate.AddDays(1);
    }

    public LocalDateTime FromDate
    {
        get => (LocalDateTime)GetValue(FromDateProperty);
        set
        {
            SetValue(FromDateProperty, value);
            OnPropertyChanged();
        }
    }

    public LocalDateTime ToDate
    {
        get => (LocalDateTime)GetValue(ToDateProperty);
        set
        {
            SetValue(ToDateProperty, value);
            OnPropertyChanged();
        }
    }

    void toDate_DateSelected(System.Object sender, DateChangedEventArgs e)
    {
        var date = sender as DatePicker;
        ToDate = Settings.ConvertDateTimeToLocal(date.Date);
    }

    void fromDate_DateSelected(System.Object sender, DateChangedEventArgs e)
    {
        var date = sender as DatePicker;
        FromDate = Settings.ConvertDateTimeToLocal(date.Date);
    }

    private void fromDate_Tapped(object sender, EventArgs e) => FocusPicker(fromDate);

    private void toDate_Tapped(object sender, EventArgs e) => FocusPicker(toDate);

    private void FocusPicker(DatePicker datePicker)
    {
        UpdateDateRanges();
        datePicker.Focus();
    }

    public void HookEvents()
    {
        toDate.DateSelected += toDate_DateSelected;
        fromDate.DateSelected += fromDate_DateSelected;
    }

    public void Dispose()
    {
        toDate.DateSelected -= toDate_DateSelected;
        fromDate.DateSelected -= fromDate_DateSelected;
    }
}
