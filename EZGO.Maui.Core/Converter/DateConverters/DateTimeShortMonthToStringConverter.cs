using System;
using System.Globalization;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using NodaTime;

namespace EZGO.Maui.Core.Converter;

public class DateTimeShortMonthToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string result = string.Empty;

        if (value != null)
        {
            LocalDateTime dateTime;

            if (value is DateTime dt)
                dateTime = Settings.ConvertDateTimeToLocal(dt.ToLocalTime());
            else
                dateTime = (LocalDateTime)value;

            result = dateTime.ToString(BaseDateFormats.DateTimeMonthShortNameFormat, CultureInfo.CurrentUICulture);
        }

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
