using System.Windows.Input;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Models.Tasks;

namespace EZGO.Maui.Controls;

public partial class TaskGauges : ContentView
{
    #region Bindable Properties
    public static BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), declaringType: typeof(TaskGauges));
    public static BindableProperty ReportItemsProperty = BindableProperty.Create(nameof(ReportItems), typeof(List<TaskOverviewReportItemModel>), typeof(TaskGauges), propertyChanged: ReportItemsChanged);
    public static BindableProperty TapCommandProperty = BindableProperty.Create(nameof(TapCommand), typeof(ICommand), declaringType: typeof(TaskGauges));
    public static BindableProperty TapCommandParameterProperty = BindableProperty.Create(nameof(TapCommandParameter), typeof(object), declaringType: typeof(TaskGauges));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    public object TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }
    #endregion


    private int notOkRangeStart;
    public int NotOkRangeStart
    {
        get => notOkRangeStart;
        set
        {
            notOkRangeStart = value;

            OnPropertyChanged();
        }
    }

    private int skippedRangeStart;
    public int SkippedRangeStart
    {
        get => skippedRangeStart;
        set
        {
            skippedRangeStart = value;

            OnPropertyChanged();
        }
    }

    private int todoRangeStart;
    public int TodoRangeStart
    {
        get => todoRangeStart;
        set
        {
            todoRangeStart = value;

            OnPropertyChanged();
        }
    }

    private int completedTaskPercentage;
    public int CompletedTaskPercentage
    {
        get => completedTaskPercentage;
        set
        {
            completedTaskPercentage = value;

            OnPropertyChanged();
        }
    }

    private int totalTaskAmount = 10;
    public int TotalTaskAmount
    {
        get => totalTaskAmount;
        set
        {
            totalTaskAmount = value;

            OnPropertyChanged();
        }
    }

    private int skippedTasksAmount;
    public int SkippedTasksAmount
    {
        get => skippedTasksAmount;
        set
        {
            skippedTasksAmount = value;

            OnPropertyChanged();
        }
    }

    private int notOkTasksAmount;
    public int NotOkTasksAmount
    {
        get => notOkTasksAmount;
        set
        {
            notOkTasksAmount = value;

            OnPropertyChanged();
        }
    }

    private int okTasksAmount;
    public int OkTasksAmount
    {
        get => okTasksAmount;
        set
        {
            okTasksAmount = value;

            OnPropertyChanged();
        }
    }

    public TaskGauges()
    {
        InitializeComponent();
    }

    private static void ReportItemsChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        TaskGauges taskGauge = (TaskGauges)bindable;

        taskGauge.TotalTaskAmount = 10;
        taskGauge.SkippedTasksAmount = 0;
        taskGauge.NotOkTasksAmount = 0;
        taskGauge.OkTasksAmount = 0;

        taskGauge.NotOkRangeStart = 0;
        taskGauge.SkippedRangeStart = 0;
        taskGauge.TodoRangeStart = 0;

        taskGauge.CompletedTaskPercentage = 0;

        if (taskGauge.ReportItems != null && taskGauge.ReportItems.Any())
        {
            taskGauge.TotalTaskAmount = taskGauge.ReportItems.Sum(x => x.NrOfItems);

            foreach (TaskOverviewReportItemModel reportItem in taskGauge.ReportItems)
            {
                switch (reportItem.TaskStatus)
                {
                    case TaskStatusEnum.Skipped:
                        taskGauge.SkippedTasksAmount = reportItem.NrOfItems;
                        break;
                    case TaskStatusEnum.NotOk:
                        taskGauge.NotOkTasksAmount = reportItem.NrOfItems;
                        break;
                    case TaskStatusEnum.Ok:
                        taskGauge.OkTasksAmount = reportItem.NrOfItems;
                        break;
                    case TaskStatusEnum.Todo:
                    default:
                        break;
                }
            }

            taskGauge.NotOkRangeStart = taskGauge.OkTasksAmount;
            taskGauge.SkippedRangeStart = taskGauge.NotOkRangeStart + taskGauge.NotOkTasksAmount;
            taskGauge.TodoRangeStart = taskGauge.SkippedRangeStart + taskGauge.SkippedTasksAmount;

            if (taskGauge.TotalTaskAmount > 0)
                taskGauge.CompletedTaskPercentage = (int)Math.Round((double)(100 * taskGauge.TodoRangeStart) / taskGauge.TotalTaskAmount);
        }
    }
}
