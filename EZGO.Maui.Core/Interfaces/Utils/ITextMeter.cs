using System;
namespace EZGO.Maui.Core.Interfaces.Utils
{
    public interface ITextMeter
    {
        Tuple<double, double> MeasureTextSize(string text, double fontSize, string fontName = null);
        Tuple<double, double> MeasureTextSize(string text, double width, double fontSize, string fontName = null);

    }
}
