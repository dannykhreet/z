using EZGO.Maui.Core.Converter;
using System.Diagnostics;
using System.Globalization;

namespace EZGO.Maui.Controls.ProgressBar;

public partial class CustomLinearProgressBar : ContentView
{
    private readonly ScoreToColorConverter scoreToColorConverter;

    public CustomLinearProgressBar()
    {
        InitializeComponent();
        scoreToColorConverter = new ScoreToColorConverter();
        // To fill bar with default 0 value
        HandleScoreChanged();
    }

    public static readonly BindableProperty BarColorProperty = BindableProperty.Create(nameof(BarColorProperty), typeof(Color), typeof(CustomLinearProgressBar), propertyChanged: OnScorePropertyChanged, defaultValue: null);

    public Color BarColor
    {
        get => (Color)GetValue(BarColorProperty);
        set
        {
            SetValue(BarColorProperty, value);
            OnPropertyChanged(nameof(BarColor));
        }
    }

    public static readonly BindableProperty ScoreProperty = BindableProperty.Create(nameof(ScoreProperty), typeof(int), typeof(CustomLinearProgressBar), propertyChanged: OnScorePropertyChanged);

    public int Score
    {
        get => (int)GetValue(ScoreProperty);
        set
        {
            // Prevent invalid scores
            if (value < 0 || value > 100)
            {
                Debug.WriteLine($"Invalid Score value {value}. Must be between 0 and 100.");
                value = Math.Clamp(value, 0, 100);
            }

            SetValue(ScoreProperty, value);
            OnPropertyChanged(nameof(Score));
        }
    }

    public static readonly BindableProperty IsLabelVisibleProperty =
        BindableProperty.Create(
            nameof(IsLabelVisible),
            typeof(bool),
            typeof(CustomLinearProgressBar),
            propertyChanged: OnIsLabelVisibleProperty);

    public bool IsLabelVisible
    {
        get => (bool)GetValue(IsLabelVisibleProperty);
        set => SetValue(IsLabelVisibleProperty, value);
    }

    public static readonly BindableProperty BarHeightProperty = BindableProperty.Create(nameof(BarHeight), typeof(double), typeof(CustomLinearProgressBar), defaultValue: 20d);

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    private void HandleScoreChanged()
    {
        double rawScore = Score;

        double safeScore = double.IsNaN(rawScore) || double.IsInfinity(rawScore)
            ? 0
            : Math.Clamp(rawScore, 0, 100);

        scoreHalf.Width = new GridLength(safeScore, GridUnitType.Star);
        noScoreHalf.Width = new GridLength(100 - safeScore, GridUnitType.Star);

        string formattedScore = FormatScore((int)Math.Round(safeScore));

        labelScore.Text = formattedScore;
        labelLayout.IsVisible = IsLabelVisible;
        labelBar.IsVisible = safeScore == 0;
        labelBar.Text = formattedScore;

        var bgColor = BarColor ?? (Color)scoreToColorConverter.Convert(safeScore, typeof(Color), null, CultureInfo.CurrentCulture);
        borderColor.BackgroundColor = bgColor;

        scoreLabel.Text = formattedScore;
    }

    private static void OnIsLabelVisibleProperty(BindableObject bindable, object oldValue, object newValue)
    {
        var progressBar = bindable as CustomLinearProgressBar;
        progressBar.labelLayout.IsVisible = progressBar.IsLabelVisible;
    }

    private static void OnScorePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var progressBar = bindable as CustomLinearProgressBar;
        Debug.WriteLine($"Progress Bar value chagned: OldValue: {oldValue}, NewValue: {newValue}");
        progressBar.HandleScoreChanged();
    }

    public static string FormatScore(int score)
    {
        return $"{score}%";
    }
}
