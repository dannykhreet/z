using System.Windows.Input;
using EZGO.Maui.Core.Interfaces.Utils;

namespace EZGO.Maui.Controls;

public partial class TreeDropdownFilter : Border
{
    #region Bindable Properties      
    public static readonly BindableProperty ItemsListProperty = BindableProperty.Create(nameof(ItemsList), typeof(List<ITreeDropdownFilterItem>), typeof(TreeDropdownFilter), propertyChanged: OnItemListChanged);
    public static readonly BindableProperty IsDropdownOpenProperty = BindableProperty.Create(nameof(IsDropdownOpen), typeof(bool), typeof(TreeDropdownFilter), defaultValue: false);
    public static readonly BindableProperty DropdownTapCommandProperty = BindableProperty.Create(nameof(DropdownTapCommand), typeof(ICommand), declaringType: typeof(TreeDropdownFilter));

    public bool IsDropdownOpen
    {
        get => (bool)GetValue(IsDropdownOpenProperty);
        set => SetValue(IsDropdownOpenProperty, value);
    }

    public List<ITreeDropdownFilterItem> ItemsList
    {
        get => (List<ITreeDropdownFilterItem>)GetValue(ItemsListProperty);
        set { SetValue(ItemsListProperty, value); OnPropertyChanged(); }
    }

    public ICommand DropdownTapCommand
    {
        get => (ICommand)GetValue(DropdownTapCommandProperty);
        set => SetValue(DropdownTapCommandProperty, value);
    }
    #endregion

    public TreeDropdownFilter()
    {
        InitializeComponent();
    }


    private static void OnItemListChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        var item = bindable as TreeDropdownFilter;
        item.treeControl.ItemsSource = item.ItemsList;
    }
}
