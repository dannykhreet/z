using System.Windows.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;

namespace EZGO.Maui.Controls.ViewCells;

public partial class TaskCarouselViewCell : ViewCell
{
    public static readonly BindableProperty DetailCommandProperty = BindableProperty.Create(nameof(DetailCommandProperty), typeof(ICommand), typeof(TaskCarouselViewCell), propertyChanged: OnDetailCommandPropertyChanged);

    private static void OnDetailCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as TaskCarouselViewCell;
        obj.ImageGesture.Command = obj.DetailCommand;
    }

    public ICommand DetailCommand
    {
        get => (ICommand)GetValue(DetailCommandProperty);
        set
        {
            SetValue(DetailCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty PropertyCommandProperty = BindableProperty.Create(nameof(PropertyCommandProperty), typeof(ICommand), typeof(TaskCarouselViewCell), propertyChanged: OnPropertyCommandPropertyChanged);

    private static void OnPropertyCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as TaskCarouselViewCell;
        obj.PropertyPopupList.OpenPopupCommand = obj.PropertyCommand;
    }

    public ICommand PropertyCommand
    {
        get => (ICommand)GetValue(PropertyCommandProperty);
        set
        {
            SetValue(PropertyCommandProperty, value);
            OnPropertyChanged();
        }
    }

    public static readonly BindableProperty CardWidthProperty = BindableProperty.Create(nameof(CardWidth), typeof(double), typeof(TaskCarouselViewCell), defaultValue: 700d);

    public double CardWidth
    {
        get => (double)GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    public static readonly BindableProperty CardPropertyWidthProperty = BindableProperty.Create(nameof(CardPropertyWidth), typeof(double), typeof(TaskCarouselViewCell), defaultValue: 85d);

    public double CardPropertyWidth
    {
        get => (double)GetValue(CardPropertyWidthProperty);
        set => SetValue(CardPropertyWidthProperty, value);
    }

    public static readonly BindableProperty FullCardWidthProperty = BindableProperty.Create(nameof(FullCardWidth), typeof(double), typeof(TaskCarouselViewCell), defaultValue: 785d);

    public double FullCardWidth
    {
        get => (double)GetValue(FullCardWidthProperty);
        set => SetValue(FullCardWidthProperty, value);
    }

    public static readonly BindableProperty IsExtraInfoVisibleProperty = BindableProperty.Create(nameof(IsExtraInfoVisible), typeof(bool), typeof(TaskCarouselViewCell), defaultValue: true);

    public bool IsExtraInfoVisible
    {
        get => (bool)GetValue(IsExtraInfoVisibleProperty);
        set => SetValue(IsExtraInfoVisibleProperty, value);
    }

    public TaskCarouselViewCell()
    {
        InitializeComponent();
    }
}
