using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Mappers
{
    public static class LanguageMapper
    {
        /// <summary>
        /// Map different cultures to cultures that are enabled on our platform.
        /// </summary>
        /// <param name="culture">Culture (e.g. nl-NL)</param>
        /// <returns>return corrected culture.</returns>
        public static string FromCulture(string culture)
        {
            string output = culture;
            if (!string.IsNullOrEmpty(output))
            {
                switch(LanguageMapper.CleanCultureForDatabase(output))
                {
                    case "nl_be" : output = "nl-nl"; break;
                    case "es_us" : case "es_cl" : output = "es-es"; break;
                    case "en_ca" : case "en_in" : case "en_ie": case "en_nz": output = "en-us"; break;
                    case "de_ch" : output = "de-de"; break;
                    case "pt_bt" : output = "pt-pt"; break;
                    case "ar_eg" : output = "ar-eg"; break;
                    case "undefined" : output = "en-us"; break;
                }

                return output;
            } else
            {
                return "en-us";
            }
        }

        /// <summary>
        /// CleanCultureForDatabase; Clean culture;
        /// </summary>
        /// <param name="culture">Culture (e.g. nl-NL)</param>
        /// <returns>return cleaned culture.</returns>
        public static string CleanCultureForDatabase(string culture)
        {
            if(!string.IsNullOrEmpty(culture))
            {
                culture = culture.Replace("-", "_").ToLower();
            }
            return culture;
        }


    }
}
