using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models.Audits;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class InstructionScoreToColorConverter: IValueConverter
    {
        private IScoreColorCalculator ScoreColorCalculator;

        public InstructionScoreToColorConverter()
        {
            ScoreColorCalculator = ScoreColorCalculatorFactory.Default(0, 5);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var score = int.Parse(value.ToString());            

            return ScoreColorCalculator.GetColor(score);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
