using Syncfusion.Maui.Core;
using Syncfusion.Maui.Inputs;
using Syncfusion.Maui.Popup;

namespace EZGO.Maui.Controls.Popup;

public partial class TaskPropertyPopupView : SfPopup
{
    #region Bindable Properties

    public static BindableProperty ContentBindingContextProperty = BindableProperty.Create(
        nameof(ContentBindingContext),
        typeof(object),
        typeof(TaskPropertyPopupView),
        propertyChanged: ContentBindingContextPropertyChanged);

    #endregion

    #region Properties 

    /// <summary>
    /// The binding context for the content of this popup
    /// </summary>
    public object ContentBindingContext
    {
        get => GetValue(ContentBindingContextProperty);
        set => SetValue(ContentBindingContextProperty, value);
    }

    /// <summary>
    /// Content template selector
    /// </summary>
    private readonly DataTemplateSelector ContentTemplateSelector;

    #endregion

    public TaskPropertyPopupView()
    {
        InitializeComponent();
        ContentTemplateSelector = Resources["InputTemplateSelector"] as DataTemplateSelector;
    }

    private static void ContentBindingContextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        // Get the control
        var control = (TaskPropertyPopupView)bindable;
        if (control.ContentTemplateSelector != null)
        {
            // Get the associated template
            var newTemplate = control.ContentTemplateSelector.SelectTemplate(newValue, control);

            // Set that template
            control.ContentTemplate = newTemplate;
        }
    }

    void EntryOnFocused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry)
        {
            Dispatcher.DispatchAsync(() =>
            {
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text != null ? entry.Text.Length : 0;
            });
        }
    }

    void SfTextInputLayout_LayoutChanged(object sender, System.EventArgs e)
    {
        SfTextInputLayout sfTextInputLayout = (SfTextInputLayout)sender;
        Dispatcher?.DispatchAsync(() =>
        {
            if (sfTextInputLayout.Content is StackLayout sl)
            {
                var entry = sl.FindByName<DatePicker>("datePicker");
                entry?.Focus();
            }
            else
                sfTextInputLayout.Content?.Focus();
        });
    }

    void datePicker_Unfocused(object sender, FocusEventArgs e)
    {
        var parent = Parent as SfPopup;
        if (sender is DatePicker datePicker)
        {
            Dispatcher?.DispatchAsync(() =>
            {
                var entry = datePicker.Parent.FindByName<TimePicker>("timePicker");
                if (parent.IsOpen && entry != null)
                    entry.Focus();
            });
        }
    }

    void SfNumericTextBox_ValueChanged(System.Object sender, NumericEntryValueChangedEventArgs e)
    {
        SfNumericEntry sfNumericTextBox = (SfNumericEntry)sender;
        if ((sfNumericTextBox.Value.ToString().Length == 9 && !sfNumericTextBox.Value.ToString().Contains('-'))
            || (sfNumericTextBox.Value.ToString().Length == 10 && sfNumericTextBox.Value.ToString().Contains('-'))) sfNumericTextBox.Value = e.OldValue;
    }
}
