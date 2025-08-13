using System.Windows.Input;

namespace EZGO.Maui.Controls.ViewCells;

public partial class AllTaskTemplateViewCell : ContentView
{
    public AllTaskTemplateViewCell()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty NavigateToAllTasksSlideCommandProperty = BindableProperty.Create(nameof(NavigateToAllTasksSlideCommand), typeof(ICommand), typeof(AllTaskTemplateViewCell), propertyChanged: OnNavigateToTaskEditCommandPropertyChanged);

    private static void OnNavigateToTaskEditCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as AllTaskTemplateViewCell;
        if (!obj.IsNavigationEnabled)
            obj.ContentGrid.GestureRecognizers.Clear();

        //obj.TapGesture.Command = obj.NavigateToTaskEditCommand;
    }

    public ICommand NavigateToAllTasksSlideCommand
    {
        get => (ICommand)GetValue(NavigateToAllTasksSlideCommandProperty);
        set => SetValue(NavigateToAllTasksSlideCommandProperty, value);
    }

    public static readonly BindableProperty IsNavigationEnabledProperty = BindableProperty.Create(nameof(IsNavigationEnabled), typeof(bool), typeof(AllTaskTemplateViewCell), defaultValue: true, propertyChanged: OnNavigateToTaskEditCommandPropertyChanged);

    public bool IsNavigationEnabled
    {
        get => (bool)GetValue(IsNavigationEnabledProperty);
        set => SetValue(IsNavigationEnabledProperty, value);
    }
}
