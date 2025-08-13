using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ShiftTypeNameConverter : IValueConverter
    {
        private string _month;
        private string month
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_month))
                {
                    _month = TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftTypeMonth);
                }
                return _month;
            }
            set
            {
                _month = value;
            }
        }

        private string _once;
        private string once
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_once))
                {
                    _once = TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftTypeOnce);
                }
                return _once;
            }
            set
            {
                _once = value;
            }
        }

        private string _shift;
        private string shift
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_shift))
                {
                    _shift = TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftTypeShift);
                }
                return _shift;
            }
            set
            {
                _shift = value;
            }
        }

        private string _week;
        private string week
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_week))
                {
                    _week = TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftTypeWeek);
                }
                return _week;
            }
            set
            {
                _week = value;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string shiftType)
            {
                switch (shiftType)
                {
                    case "month":
                        if (month.IsNullOrWhiteSpace()) month = shiftType;
                        return month;
                    case "week":
                        if (week.IsNullOrWhiteSpace()) week = shiftType;
                        return week;
                    case "norecurrency":
                    case "no recurrency":
                        if (once.IsNullOrWhiteSpace()) once = shiftType;
                        return once;
                    case "shifts":
                        if (shift.IsNullOrWhiteSpace()) shift = shiftType;
                        return shift;
                    case "periodday":
                        return TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftTypeDailyInterval);
                    case "dynamicday":
                        return TranslateExtension.GetValueFromDictionary(LanguageConstants.shiftTypeDynamicDailyInterval);
                }
                return shiftType;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
