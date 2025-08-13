using System.Windows.Input;
using EZGO.Maui.Classes;
using EZGO.Maui.Core.Enumerations;

namespace EZGO.Maui.Controls;

public partial class LayoutSwitcher : StackLayout
{
    private Color greenColor;
    private Color defaultColor;

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(LayoutSwitcher));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty ListLayoutProperty = BindableProperty.Create(nameof(ListLayoutProperty), typeof(ListViewLayout), typeof(LayoutSwitcher), propertyChanged: OnListLayoutPropertyChanged);

    private static void OnListLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as LayoutSwitcher;
        if (obj.ListLayout == ListViewLayout.Grid)
        {
            obj.GridButton.TextColor = obj.greenColor;
            obj.LinearButton.TextColor = obj.defaultColor;
        }
        else
        {
            obj.LinearButton.TextColor = obj.greenColor;
            obj.GridButton.TextColor = obj.defaultColor;
        }
    }

    public ListViewLayout ListLayout
    {
        get => (ListViewLayout)GetValue(ListLayoutProperty);
        set
        {
            SetValue(ListLayoutProperty, value);
            OnPropertyChanged();
        }
    }

    public LayoutSwitcher()
    {
        InitializeComponent();
        greenColor = ResourceHelper.GetValueFromResources<Color>("GreenColor");
        defaultColor = ResourceHelper.GetValueFromResources<Color>("GreyColor");
    }
}
