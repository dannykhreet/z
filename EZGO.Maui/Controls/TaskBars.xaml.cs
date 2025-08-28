using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Models.Tasks;
using System.ComponentModel;
using System.Windows.Input;

namespace EZGO.Maui.Controls;

public partial class TaskBars : ContentView, INotifyPropertyChanged
{
    public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), declaringType: typeof(TaskBars));
    public static BindableProperty BarIsVisibleProperty = BindableProperty.Create(nameof(BarIsVisible), typeof(bool), declaringType: typeof(TaskBars));
    public static BindableProperty EmptyColorProperty = BindableProperty.Create(nameof(EmptyColor), typeof(Color), declaringType: typeof(TaskBars));
    public static BindableProperty ReportItemsProperty = BindableProperty.Create(nameof(ReportItems), typeof(List<TaskOverviewReportItemModel>), typeof(TaskBars), propertyChanged: ReportItemsChanged);
    public static BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand), typeof(ICommand), declaringType: typeof(TaskBars));
    public static BindableProperty TapCommandParamaterProperty = BindableProperty.Create(nameof(TapCommandParamater), typeof(TaskPeriod), declaringType: typeof(TaskBars));
    public static readonly BindableProperty GaugeMarginProperty = BindableProperty.Create(nameof(GaugeMargin), typeof(Thickness), typeof(TaskBars), defaultValue: new Thickness(0, 0, 0, 15));

    public Thickness GaugeMargin
    {
        get => (Thickness)GetValue(GaugeMarginProperty);
        set => SetValue(GaugeMarginProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool BarIsVisible
    {
        get => (bool)GetValue(BarIsVisibleProperty);
        set => SetValue(BarIsVisibleProperty, value);
    }

    public TaskPeriod TapCommandParamater
    {
        get => (TaskPeriod)GetValue(TapCommandParamaterProperty);
        set => SetValue(TapCommandParamaterProperty, value);
    }

    public Color EmptyColor
    {
        get
        {
            var result = (Color)GetValue(EmptyColorProperty);
            return result != null ? result : (Color)Application.Current.Resources["GreyColor"];
        }
        set => SetValue(EmptyColorProperty, value);
    }

    public List<TaskOverviewReportItemModel> ReportItems
    {
        get => (List<TaskOverviewReportItemModel>)GetValue(ReportItemsProperty);
        set => SetValue(ReportItemsProperty, value);
    }

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    private int totalTaskAmount;
    public int TotalTaskAmount
    {
        get => totalTaskAmount;
        set
        {
            totalTaskAmount = value;

            OnPropertyChanged();
        }
    }

    private TaskOverviewReportItemModel _OkReport = new TaskOverviewReportItemModel { TaskStatus = TaskStatusEnum.Ok, NrOfItems = 0 };
    public TaskOverviewReportItemModel OkReport
    {
        get => _OkReport;
        set
        {
            _OkReport = value;
            OnPropertyChanged();
        }
    }

    private TaskOverviewReportItemModel _NotOkReport = new TaskOverviewReportItemModel { TaskStatus = TaskStatusEnum.NotOk, NrOfItems = 0 };
    public TaskOverviewReportItemModel NotOkReport
    {
        get => _NotOkReport;
        set
        {
            _NotOkReport = value;
            OnPropertyChanged();
        }
    }

    private TaskOverviewReportItemModel _SkippedReport = new TaskOverviewReportItemModel { TaskStatus = TaskStatusEnum.Skipped, NrOfItems = 0 };
    public TaskOverviewReportItemModel SkippedReport
    {
        get => _SkippedReport;
        set
        {
            _SkippedReport = value;
            OnPropertyChanged();
        }
    }

    private TaskOverviewReportItemModel _TodoReport = new TaskOverviewReportItemModel { TaskStatus = TaskStatusEnum.Todo, NrOfItems = 10 };
    public TaskOverviewReportItemModel TodoReport
    {
        get => _TodoReport;
        set
        {
            _TodoReport = value;
            OnPropertyChanged();
        }
    }

    public TaskBars()
    {
        InitializeComponent();
    }

    private static void ReportItemsChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        TaskBars taskBar = (TaskBars)bindable;

        if (taskBar.ReportItems != null && taskBar.ReportItems.Any())
        {
            taskBar.TotalTaskAmount = taskBar.ReportItems.Sum(x => x.NrOfItems);

            foreach (TaskOverviewReportItemModel reportItem in taskBar.ReportItems)
            {
                switch (reportItem.TaskStatus)
                {
                    case TaskStatusEnum.Skipped:
                        taskBar.SkippedReport = reportItem;
                        break;
                    case TaskStatusEnum.NotOk:
                        taskBar.NotOkReport = reportItem;
                        break;
                    case TaskStatusEnum.Ok:
                        taskBar.OkReport = reportItem;
                        break;
                    case TaskStatusEnum.Todo:
                        taskBar.TodoReport = reportItem;
                        break;
                }
            }
        }
        else
        {
            taskBar.TotalTaskAmount = 0;
        }
    }

    public static readonly BindableProperty IsLoadingProperty =
    BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(TaskBars), false, propertyChanged: OnIsLoadingChanged);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static void OnIsLoadingChanged(BindableObject bindable, object oldValue, object newValue)
    {
    }
}
