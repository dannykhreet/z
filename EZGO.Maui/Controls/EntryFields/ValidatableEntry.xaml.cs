using EZGO.Maui.Core.Extensions;

namespace EZGO.Maui.Controls.EntryFields;

public partial class ValidatableEntry : ContentView
{
    public readonly static BindableProperty TitleProperty = BindableProperty.Create(nameof(TitleProperty), typeof(string), typeof(ValidatableEntry), propertyChanged: OnTitlePropertyChanged);
    public readonly static BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPasswordProperty), typeof(bool), typeof(ValidatableEntry), defaultValue: false, propertyChanged: OnIsPasswordPropertyChanged);

    private static void OnTitlePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ValidatableEntry;
        obj.fieldLabel.Text = obj.Title;
    }

    private static void OnIsPasswordPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ValidatableEntry;
        obj.input.IsPassword = obj.IsPassword;
    }

    public readonly static BindableProperty IsLiveValidationOnProperty = BindableProperty.Create(nameof(IsLiveValidationOnProperty), typeof(bool), typeof(ValidatableEntry), defaultValue: false);
    public readonly static BindableProperty ValidateProperty = BindableProperty.Create(nameof(ValidateProperty), typeof(Func<bool>), typeof(ValidatableEntry));
    public readonly static BindableProperty InputTextProperty = BindableProperty.Create(nameof(InputTextProperty), typeof(string), typeof(ValidatableEntry), propertyChanged: OnInputTextPropertyChanged, defaultBindingMode: BindingMode.TwoWay);
    public readonly static BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(PlaceholderProperty), typeof(string), typeof(ValidatableEntry));

    private static void OnInputTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ValidatableEntry;
        obj.input.Text = obj.InputText;
    }

    public readonly static BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValidProperty), typeof(bool), typeof(ValidatableEntry), propertyChanged: OnIsValidPropertyChanged);

    private static void OnIsValidPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ValidatableEntry;
        obj.beh.IsValid = obj.IsValid;
        obj.fieldLabel.TextColor = obj.IsValid ? Colors.Black : Colors.Red;
        obj.inputLayout.HasError = !obj.IsValid;
    }

    public readonly static BindableProperty ErrorsProperty = BindableProperty.Create(nameof(ErrorsProperty), typeof(List<string>), typeof(ValidatableEntry), propertyChanged: OnErrorsPropertyChanged);

    private static void OnErrorsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ValidatableEntry;
        obj.inputLayout.ErrorText = obj.Errors.IsNullOrEmpty() ? null : obj.Errors.First();
    }

    public ValidatableEntry()
    {
        InitializeComponent();
    }

    public Func<bool> Validate
    {
        get => (Func<bool>)GetValue(ValidateProperty);
        set
        {
            SetValue(ValidateProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set
        {
            SetValue(IsPasswordProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsLiveValidationOn
    {
        get => (bool)GetValue(IsLiveValidationOnProperty);
        set
        {
            SetValue(IsLiveValidationOnProperty, value);
            OnPropertyChanged();
        }
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set
        {
            SetValue(TitleProperty, value);
            OnPropertyChanged();
        }
    }

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set
        {
            SetValue(InputTextProperty, value);
            OnPropertyChanged();
        }
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set
        {
            SetValue(PlaceholderProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set
        {
            SetValue(IsValidProperty, value);
            OnPropertyChanged();
        }
    }

    public List<string> Errors
    {
        get => (List<string>)GetValue(ErrorsProperty);
        set
        {
            SetValue(ErrorsProperty, value);
            OnPropertyChanged();
        }
    }

    void input_TextChanged(System.Object sender, TextChangedEventArgs e)
    {
        if (IsLiveValidationOn && Validate != null)
            Validate();
    }
}
