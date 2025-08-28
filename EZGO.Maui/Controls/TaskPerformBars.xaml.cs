using EZGO.Maui.Core.Models.Reports;

namespace EZGO.Maui.Controls;

public partial class TaskPerformBars : ContentView
{
    public static BindableProperty CountsProperty = BindableProperty.Create(nameof(Counts), typeof(ReportsCount), typeof(TaskPerformBars), propertyChanged: ReportChanged);
    public ReportsCount Counts
    {
        get => (ReportsCount)GetValue(CountsProperty);
        set => SetValue(CountsProperty, value);
    }

    public TaskPerformBars()
    {
        InitializeComponent();
    }

    private int skipped;
    public int Skipped
    {
        get => skipped;
        set
        {
            skipped = value;
            OnPropertyChanged();
        }
    }

    private double skippedPercentage;
    public double SkippedPercentage
    {
        get => skippedPercentage;
        set
        {
            skippedPercentage = value;
            OnPropertyChanged();
        }
    }

    private int notOk;
    public int NotOk
    {
        get => notOk;
        set
        {
            notOk = value;
            OnPropertyChanged();
        }
    }

    private double notOkPercentage;
    public double NotOkPercentage
    {
        get => notOkPercentage;
        set
        {
            notOkPercentage = value;
            OnPropertyChanged();
        }
    }

    private int ok;
    public int Ok
    {
        get => ok;
        set
        {
            ok = value;
            OnPropertyChanged();
        }
    }

    private double okPercentage;
    public double OkPercentage
    {
        get => okPercentage;
        set
        {
            okPercentage = value;
            OnPropertyChanged();
        }
    }


    private string subscript;
    public string Subscript
    {
        get => subscript;
        set
        {
            subscript = value;
            OnPropertyChanged();
        }
    }

    private bool barVisible;
    public bool BarVisible
    {
        get => barVisible;
        set
        {
            barVisible = value;
            OnPropertyChanged();
        }
    }

    private static void ReportChanged(BindableObject bindable, object oldvalue, object newvalue)
    {
        TaskPerformBars bar = (TaskPerformBars)bindable;

        if (bar.Counts != null)
        {
            var count = bar.Counts.NrSkipped + bar.Counts.NrNotOk + bar.Counts.NrOk;
            if (count > 0)
            {
                bar.Skipped = bar.Counts.NrSkipped;
                bar.NotOk = bar.Counts.NrNotOk;
                bar.Ok = bar.Counts.NrOk;

                bar.SkippedPercentage = Math.Round(((double)bar.Counts.NrSkipped / count) * 100, 2);
                bar.NotOkPercentage = Math.Round(((double)bar.Counts.NrNotOk / count) * 100, 2);
                bar.OkPercentage = Math.Round(((double)bar.Counts.NrOk / count) * 100, 2);
                var test = bar.Counts.PercentageOk;

                bar.BarVisible = true;
            }
            else
            {
                bar.BarVisible = false;
                bar.Skipped = 10;
            }

            bar.Subscript = bar.Counts.Subscript;
        }
    }
}
