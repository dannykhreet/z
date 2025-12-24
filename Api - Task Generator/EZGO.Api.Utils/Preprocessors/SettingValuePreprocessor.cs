using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Preprocessors
{
    public static class SettingValuePreprocessor
    {
        public static string PreprocessSettingValue(this string incomingValue)
        {
            var updatedValue = incomingValue;

            if (!string.IsNullOrEmpty(incomingValue) && incomingValue != "ALL")
            {
                var settingValues = incomingValue.Split(",");
                var companyIds = new List<int>();

                bool nonNumericSettingFound = false;

                foreach (var settingValue in settingValues)
                {
                    if (!nonNumericSettingFound && int.TryParse(settingValue, out var settingCompanyId))
                    {
                        companyIds.Add(settingCompanyId);
                    }
                    else
                    {
                        nonNumericSettingFound = true;
                        break;
                    }
                }
                if (!nonNumericSettingFound)
                {
                    companyIds = companyIds.Distinct().OrderBy(c => c).ToList();
                    updatedValue = string.Join(",", companyIds);
                }
            }

            return updatedValue;
        }
    }
}
