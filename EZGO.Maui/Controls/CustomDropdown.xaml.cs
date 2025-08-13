using System.Windows.Input;

namespace EZGO.Maui.Controls;

public partial class CustomDropdown : HorizontalStackLayout
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(CustomDropdown));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CustomDropdown));

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly BindableProperty IsExpandIconVisibleProperty = BindableProperty.Create(nameof(IsExpandIconVisible), typeof(bool), typeof(CustomDropdown), defaultValue: true);

    public bool IsExpandIconVisible
    {
        get => (bool)GetValue(IsExpandIconVisibleProperty);
        set => SetValue(IsExpandIconVisibleProperty, value);
    }

    public CustomDropdown()
    {
        InitializeComponent();
    }
}
