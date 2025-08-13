using EZGO.Maui.Core.Models.Reports;

namespace EZGO.Maui.Controls;

public partial class ReportBars : ContentView
{
    public static BindableProperty StatsProperty = BindableProperty.Create(nameof(Stats), typeof(TaskStats), typeof(ReportBars), propertyChanged: StatsChanged);
    public TaskStats Stats
    {
        get => (TaskStats)GetValue(StatsProperty);
        set => SetValue(StatsProperty, value);
    }

    private string ok;
    public string Ok
    {
        get => ok;
        set
        {
            ok = value;
            OnPropertyChanged();
        }
    }

    private string notok;
    public string NotOk
    {
        get => notok;
        set
        {
            notok = value;
            OnPropertyChanged();
        }
    }

    private string skipped;
    public string Skipped
    {
        get => skipped;
        set
        {
            skipped = value;
            OnPropertyChanged();
        }
    }

    private string todo;
    public string Todo
    {
        get => todo;
        set
        {
            todo = value;
            OnPropertyChanged();
        }
    }

    private double percentageOk = 25;
    public double PercentageOk
    {
        get => percentageOk;
        set
        {
            percentageOk = value;
            OnPropertyChanged();
        }
    }

    private double percentageNotOk = 25;
    public double PercentageNotOk
    {
        get => percentageNotOk;
        set
        {
            percentageNotOk = value;
            OnPropertyChanged();
        }
    }

    private double percentageSkipped = 25;
    public double PercentageSkipped
    {
        get => percentageSkipped;
        set
        {
            percentageSkipped = value;
            OnPropertyChanged();
        }
    }

    private double percentagetodo = 25;
    public double PercentageTodo
    {
        get => percentagetodo;
        set
        {
            percentagetodo = value;
            OnPropertyChanged();
        }
    }

    public ReportBars()
    {
        InitializeComponent();
    }

    private static void StatsChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        ReportBars reportBar = (ReportBars)bindable;

        if (reportBar.Stats != null)
        {
            reportBar.Ok = reportBar.Stats.Ok.ToString();
            reportBar.NotOk = reportBar.Stats.NotOk.ToString();
            reportBar.Skipped = reportBar.Stats.Skipped.ToString();
            reportBar.Todo = reportBar.Stats.Todo.ToString();

            reportBar.PercentageOk = reportBar.Stats.PercentageOk;
            reportBar.PercentageNotOk = reportBar.Stats.PercentageNotOk;
            reportBar.PercentageSkipped = reportBar.Stats.PercentageSkipped;

            reportBar.PercentageTodo = 100 - (reportBar.Stats.PercentageOk + reportBar.Stats.PercentageNotOk + reportBar.Stats.PercentageSkipped);
        }
    }
}
