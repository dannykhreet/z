using System.ComponentModel;
using System.Windows.Input;

namespace EZGO.Maui.Controls.ViewCells.Templates;

public partial class SignButtonTemplate : Grid, INotifyPropertyChanged
{
    public static readonly BindableProperty TasksDoneProperty = BindableProperty.Create(nameof(TasksDone), typeof(bool), typeof(SignButtonTemplate));

    public bool TasksDone
    {
        get => (bool)GetValue(TasksDoneProperty);
        set => SetValue(TasksDoneProperty, value);
    }

    public static readonly BindableProperty IsSyncingProperty = BindableProperty.Create(nameof(IsSyncing), typeof(bool), typeof(SignButtonTemplate));

    public bool IsSyncing
    {
        get => (bool)GetValue(IsSyncingProperty);
        set => SetValue(IsSyncingProperty, value);
    }

    public static readonly BindableProperty SignCommandProperty = BindableProperty.Create(nameof(SignCommand), typeof(ICommand), typeof(SignButtonTemplate));

    public ICommand SignCommand
    {
        get => (ICommand)GetValue(SignCommandProperty);
        set => SetValue(SignCommandProperty, value);
    }

    public static readonly BindableProperty IsSignatureRequiredProperty = BindableProperty.Create(nameof(IsSignatureRequired), typeof(bool), typeof(SignButtonTemplate), propertyChanged: OnIsSignatureRequiredPropertyChanged);

    private static void OnIsSignatureRequiredPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as SignButtonTemplate;
        obj.Text = obj.IsSignatureRequired ? obj.SignText : obj.CompleteText;
    }

    public bool IsSignatureRequired
    {
        get => (bool)GetValue(IsSignatureRequiredProperty);
        set => SetValue(IsSignatureRequiredProperty, value);
    }

    public static readonly BindableProperty TextHorizontalOptionProperty = BindableProperty.Create(nameof(TextHorizontalOption), typeof(LayoutOptions), typeof(SignButtonTemplate));

    public LayoutOptions TextHorizontalOption
    {
        get => (LayoutOptions)GetValue(TextHorizontalOptionProperty);
        set => SetValue(TextHorizontalOptionProperty, value);
    }

    public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(int), typeof(SignButtonTemplate), defaultValue: 22);

    public int FontSize
    {
        get => (int)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly BindableProperty SignTextProperty = BindableProperty.Create(nameof(SignText), typeof(string), typeof(SignButtonTemplate), propertyChanged: OnIsSignatureRequiredPropertyChanged);

    public string SignText
    {
        get => (string)GetValue(SignTextProperty);
        set => SetValue(SignTextProperty, value);
    }

    public static readonly BindableProperty CompleteTextProperty = BindableProperty.Create(nameof(CompleteText), typeof(string), typeof(SignButtonTemplate), propertyChanged: OnIsSignatureRequiredPropertyChanged);

    public string CompleteText
    {
        get => (string)GetValue(CompleteTextProperty);
        set => SetValue(CompleteTextProperty, value);
    }

    private static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SignButtonTemplate));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public SignButtonTemplate()
    {
        InitializeComponent();
    }
}
