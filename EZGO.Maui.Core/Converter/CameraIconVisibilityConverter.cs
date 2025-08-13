using System;
using System.Globalization;
using EZGO.Api.Models.Enumerations;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class CameraIconVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool hasPictureProof = (bool?)values[1] ?? false;

                if (!hasPictureProof)
                    return false;

                var bindedScoreResult = Enum.TryParse(typeof(ScoreTypeEnum), values[0]?.ToString(), out var bindedScoreType);
                bindedScoreType = bindedScoreResult ? bindedScoreType : ScoreTypeEnum.Thumbs;

                var result = Enum.TryParse(typeof(ScoreTypeEnum), values[2]?.ToString(), out var typedScoreType);
                typedScoreType = result ? typedScoreType : ScoreTypeEnum.Thumbs;

                return bindedScoreType.ToString() == typedScoreType.ToString();
            }
            catch (Exception e)
            {
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
