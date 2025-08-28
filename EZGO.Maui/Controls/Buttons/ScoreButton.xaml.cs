using System.Globalization;
using EZGO.Maui.Core.Converter.MultiBinding;
using EZGO.Maui.Core.Models.Audits;
using Syncfusion.Maui.Buttons;

namespace EZGO.Maui.Controls.Buttons;

public partial class ScoreButton : SfButton
{
    public static readonly BindableProperty ScoreProperty = BindableProperty.Create(nameof(Score), typeof(int?), typeof(ScoreButton), propertyChanged: OnScorePropertyChanged);

    private static void OnScorePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ScoreButton;
        obj.SetButtonColor();
    }

    public int? Score
    {
        get => (int?)GetValue(ScoreProperty);
        set => SetValue(ScoreProperty, value);
    }

    public static readonly BindableProperty ColorCalculatorProperty = BindableProperty.Create(nameof(ColorCalculator), typeof(IScoreColorCalculator), typeof(ScoreButton), propertyChanged: OnScoreColorCalculatorChanged);

    private static void OnScoreColorCalculatorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ScoreButton;
        obj.ColorCalculator = newValue as IScoreColorCalculator;
        obj.SetButtonColor();
    }

    public IScoreColorCalculator ColorCalculator
    {
        get => (IScoreColorCalculator)GetValue(ColorCalculatorProperty);
        set => SetValue(ColorCalculatorProperty, value);
    }

    public static readonly BindableProperty EmptyBackgroundColorProperty = BindableProperty.Create(nameof(EmptyBackgroundColor), typeof(Color), typeof(ScoreButton), propertyChanged: OnEmptyBackgroundColorChanged, defaultBindingMode: BindingMode.TwoWay);

    private static void OnEmptyBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var obj = bindable as ScoreButton;
        obj.EmptyBackgroundColor = (Color)newValue;
        obj.SetButtonColor();
    }

    public Color EmptyBackgroundColor
    {
        get => (Color)GetValue(EmptyBackgroundColorProperty);
        set => SetValue(EmptyBackgroundColorProperty, value);
    }

    public ScoreButton()
    {
        InitializeComponent();
    }

    public Color ScoreBackgroundColor { get; set; }

    public void SetButtonColor()
    {
        if (Score == null)
        {
            scoreButton.Background = Colors.Transparent;
            scoreButton.Stroke = EmptyBackgroundColor;
        }
        else
        {
            var converter = new AuditScoreColorConverter();
            var color = (Color)converter.Convert(new object[] { Score, ColorCalculator }, typeof(Color), null, CultureInfo.CurrentCulture);
            ScoreBackgroundColor = color;
            OnPropertyChanged(nameof(ScoreBackgroundColor));
        }
    }
}
