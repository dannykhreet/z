using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Maui.Core.Extensions
{
    public class TranslateExtension
    {
        public static string GetValueFromDictionary(string text)
        {
            if (text.IsNullOrEmpty())
                return "";

            Statics.LanguageDictionary.TryGetValue(text, out string result);

            if (string.IsNullOrEmpty(result))
            {
                Statics.DefaultLanguageDictionary.TryGetValue(text, out result);
            }

            return result ?? "";
        }
    }
}
