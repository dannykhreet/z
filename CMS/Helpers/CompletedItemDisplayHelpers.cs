using EZGO.CMS.LIB.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Models.Checklist;
using WebApp.Models.Shared;

namespace WebApp.Helpers
{
    public static class CompletedItemDisplayHelpers
    {
        public static string GetPropertiesAsString(SharedTaskModel completedTask, TimeZoneInfo timezone, string locale)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < completedTask.Properties.Count(); i++)
            {
                var templateProperty = completedTask.Properties[i];
                var possibePropertyValue = completedTask.PropertyUserValues.Where(x => x.PropertyId == templateProperty.PropertyId && x.TemplatePropertyId == templateProperty.Id).FirstOrDefault();
                if (possibePropertyValue != null && possibePropertyValue.Id > 0)
                {
                    DateTime propertyDate = TimeZoneInfo.ConvertTime(possibePropertyValue.CreatedAt.ToLocalTime(), timezone);

                    if (possibePropertyValue.RegisteredAt != null)
                    {
                        propertyDate = TimeZoneInfo.ConvertTime(possibePropertyValue.RegisteredAt.Value.ToLocalTime(), timezone);
                    }

                    sb.AppendFormat("{0}: ", !String.IsNullOrEmpty(templateProperty.TitleDisplay) ? templateProperty.TitleDisplay : templateProperty.Property.ShortName);
                    sb.Append(possibePropertyValue.UserBoolValue);
                    sb.Append(possibePropertyValue.UserValueDate);
                    sb.Append(possibePropertyValue.UserValueDecimal / 1.000000000000000000000000000000000m);
                    sb.Append(possibePropertyValue.UserValueInt);
                    sb.Append(possibePropertyValue.UserValueString);
                    sb.Append(possibePropertyValue.UserValueTime);
                    if (!string.IsNullOrEmpty(templateProperty.PropertyValueDisplay))
                    {
                        sb.Append(templateProperty.PropertyValueDisplay);
                    }
                    else if (templateProperty.PropertyValue != null)
                    {
                        sb.Append(templateProperty.PropertyValue.ValueSymbol);
                    }
                    sb.AppendFormat(" on {0}", propertyDate.ToLocaleFullDateShortTimeString(locale));
                    sb.Append(templateProperty.IsRequired.HasValue && templateProperty.IsRequired.Value ? " *" : "");
                    sb.Append(", ");
                }
            }
            if (sb.Length > 0)
            {
                var output = sb.ToString().Trim(); //remove trailing spaces
                return output.Substring(0, output.Length - 1); //remove last ,
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetPropertiesAsString(EZGO.Api.Models.TasksTask completedTaskTask, TimeZoneInfo timezone, string locale)
        {
            return GetPropertiesAsString(completedTask: completedTaskTask.ToJsonFromObject().ToObjectFromJson<SharedTaskModel>(), timezone: timezone, locale: locale);
        }
    }
}