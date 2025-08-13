using System.Windows.Input;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Controls.TaskProperties;

public partial class TaskPropertyPopupListView : ContentView
{
    public const int NumberOfItemsVisible = 6;

    public static readonly BindableProperty OpenPopupCommandProperty = BindableProperty.Create(nameof(OpenPopupCommandProperty), typeof(ICommand), typeof(TaskPropertyPopupListView));
    public static readonly BindableProperty PropertiesSourceProperty = BindableProperty.Create(nameof(PropertiesSourceProperty), typeof(IReadOnlyList<BasicTaskPropertyModel>), typeof(TaskPropertyPopupListView), propertyChanged: SourcePropertyChanged, defaultBindingMode: BindingMode.OneWay);

    /// <summary>
    /// Gets or sets the command to be exectued when a property box is clicked
    /// </summary>
    public ICommand OpenPopupCommand
    {
        get => (ICommand)GetValue(OpenPopupCommandProperty);
        set => SetValue(OpenPopupCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets item source for this control
    /// </summary>
    public IReadOnlyList<BasicTaskPropertyModel> PropertiesSource
    {
        get => (IReadOnlyList<BasicTaskPropertyModel>)GetValue(PropertiesSourceProperty);
        set => SetValue(PropertiesSourceProperty, value);
    }

    private static void SourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TaskPropertyPopupListView instance)
        {
            var newValueList = (IReadOnlyList<BasicTaskPropertyModel>)newValue;

            // If we don't have enough items
            if (newValueList != null && newValueList.Count() < NumberOfItemsVisible)
            {
                // We need at least the NumberOfItemsVisible in the list
                // Fill the missing parts with nulls
                var newItemSource = newValueList.Concat(Enumerable.Repeat<BasicTaskPropertyModel>(null, NumberOfItemsVisible - newValueList.Count()));
                instance.listView.ItemsSource = newItemSource;

            }
            // We have enough items in the list
            else
            {
                instance.listView.ItemsSource = newValueList;
            }

        }
    }

    public TaskPropertyPopupListView()
    {
        InitializeComponent();
    }
}
