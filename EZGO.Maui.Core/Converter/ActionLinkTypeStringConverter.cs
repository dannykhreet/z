using EZGO.Api.Models.Basic;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Converter
{
    public class ActionLinkTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            if (value is ActionParentBasic parent)
            {
                if (parent.AuditTemplateId.HasValue)
                    result = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionDetailScreenActionIsLinkedToAuditItem);
                else if (parent.ChecklistTemplateId.HasValue)
                    result = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionDetailScreenActionIsLinkedToChecklistItem);
                else
                    result = TranslateExtension.GetValueFromDictionary(LanguageConstants.actionDetailScreenActionIsLinkedToTask);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
