using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic.Interfaces;
using WebApp.Models.Language;

namespace WebApp.Logic.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IApiConnector _connector;
        private readonly IMemoryCache _memoryCache;
        public LanguageService(IApiConnector connector, IMemoryCache memoryCache)
        {
            _connector = connector;
            _memoryCache = memoryCache;
        }

        public async Task PrimeLanguages()
        {
            // list of basic languages, always present
            List<String> basicLanguages = new List<string> { "en-US", "nl-NL" };
            List<Task> tasks = new List<Task>();

            foreach (string locale in basicLanguages)
            {
                await GetLanguageDictionaryAsync(locale);
            }

        }

        private async Task<LanguageModel> GetLanguageAsync(string locale)
        {
            LanguageModel language = null;

            var endpoint = string.Format(Logic.Constants.CmsLanguage.GetLanguageKeys, locale.ToLowerInvariant());
            var result = await _connector.GetCall(endpoint);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                language = JsonConvert.DeserializeObject<LanguageModel>(result.Message);
            }

            return language;
        }

        public async Task<Dictionary<string, string>> GetLanguageDictionaryAsync(string locale)
        {
            try
            {
                var cacheKey = string.Concat("language_cache_", locale);
                var found = _memoryCache.TryGetValue(cacheKey, out _);
                Dictionary<string, string> language = null;
                Statics.Languages ??= new Dictionary<string, Dictionary<string, string>>();

                if (Statics.Languages != null && found && Statics.Languages.TryGetValue(locale, out language))
                {
                    return language;
                }
                else
                {
                    LanguageModel result = await GetLanguageAsync(locale);
                    if (result != null)
                    {
                        language = new();
                        foreach (var item in result.ResourceItems)
                        {
                            language[item.ResourceKey] = item.ResourceValue;
                        }

                        if (Statics.Languages.ContainsKey(locale))
                        {
                            Statics.Languages[locale] = language;
                        } else
                        {
                            Statics.Languages.Add(locale, language);
                        }

                        _memoryCache.Set(cacheKey, DateTime.Now, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600) });

                        return language;
                    }
                    return Statics.Languages.FirstOrDefault().Value;
                }
            }
            catch {
                return Statics.Languages.FirstOrDefault().Value;
            }
            finally
            {

            }
        }

        public async Task<List<SelectListItem>> GetLanguageSelectorItems()
        {
            var possibleActiveLanguages = await GetActiveLanguages();
            var result = new List<LanguageModel>();
            if (possibleActiveLanguages != null && possibleActiveLanguages.Count > 0)
            {
                foreach (var code in possibleActiveLanguages)
                {
                    var isoCode = code.Replace("_", "-");
                    var culture = CultureInfo.GetCultureInfo(isoCode);
                    if (culture != null)
                    {
                        var languageName = culture.NativeName != null ? culture.NativeName : culture.EnglishName;
                        languageName = char.ToUpper(languageName.First()) + languageName.Substring(1).ToLower();
                        languageName = languageName.Split("(")[0];
                        if(isoCode.ToLower() == "en-us") {
                            languageName = "English (US)"; //display US in language
                        }
                        if (isoCode.ToLower() == "en-gb")
                        {
                            languageName = "English (UK)"; //display UK in language
                        }
                        result.Add(new LanguageModel { Language = languageName, LanguageCulture = isoCode });
                        //code, string.Format("{0} ({1})", culture.NativeName != null ? culture.NativeName : culture.EnglishName, culture.Name));

                    }

                }
            } else
            {
                result.Add(new LanguageModel { Language = "English (US)", LanguageCulture = "en-US" });
                result.Add(new LanguageModel { Language = "English (UK)", LanguageCulture = "en-GB" });
                result.Add(new LanguageModel { Language = "Nederlands", LanguageCulture = "nl-NL" });
                result.Add(new LanguageModel { Language = "Français", LanguageCulture = "fr-FR" });
                result.Add(new LanguageModel { Language = "Deutsch", LanguageCulture = "de-DE" });
                result.Add(new LanguageModel { Language = "Español", LanguageCulture = "es-ES" });
                result.Add(new LanguageModel { Language = "Português", LanguageCulture = "pt-PT" });
            }


            if (result != null)
            {
                SetStaticAvailableLanguages(result);

                result = result.Distinct(new LanguageModelComparer()).ToList();
                return result.Select(x => new SelectListItem { Text = x.Language, Value = x.LanguageCulture }).ToList();
            }
            return null;
        }

        public async Task<List<string>> GetActiveLanguages()
        {
            var languages = new Languages();
            var languageKeys = new List<string>();

            try
            {
                var endpoint = "v1/tools/resources/language/cultures";
                var result = await _connector.GetCall(endpoint);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    languages = JsonConvert.DeserializeObject<Languages>(result.Message);
                }

                if (languages.SupportedLanguages != null && languages.SupportedLanguages.Count > 0)
                {
                    foreach (var item in languages.SupportedLanguages) 
                    {
                        languageKeys.Add(item.Key);
                    }
                }
#pragma warning disable CS0168 // Variable is declared but never used
            } catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {

            }

            return languageKeys;
        }

        /// <summary>
        /// GetLanguageItems; Get language items (names, iso codes, etc. for dispay)
        /// </summary>
        /// <returns></returns>
        public async Task<List<LanguageModel>> GetLanguageItems()
        {
            var possibleActiveLanguages = await GetActiveLanguages();

            var result = new List<LanguageModel>();
            if (possibleActiveLanguages != null && possibleActiveLanguages.Count > 0)
            {
                foreach (var code in possibleActiveLanguages)
                {
                    var isoCode = code.Replace("_", "-");
                    var culture = CultureInfo.GetCultureInfo(isoCode);
                    if (culture != null)
                    {
                        var languageName = culture.NativeName != null ? culture.NativeName : culture.EnglishName;
                        languageName = char.ToUpper(languageName.First()) + languageName.Substring(1).ToLower();
                        languageName = languageName.Split("(")[0];
                        result.Add(new LanguageModel { Language = languageName, LanguageEnglishName = culture.EnglishName, LanguageCulture = isoCode, LanguageIso = culture.TwoLetterISOLanguageName });
                        //code, string.Format("{0} ({1})", culture.NativeName != null ? culture.NativeName : culture.EnglishName, culture.Name));

                    }

                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="languages"></param>
        /// <returns></returns>
        private bool SetStaticAvailableLanguages(List<LanguageModel> languages)
        {
            try
            {
                if (Statics.AvailableLanguages == null || Statics.AvailableLanguages.Count < 0)
                {
                    Statics.AvailableLanguages = languages.Select(x => x.LanguageCulture.ToLower()).ToList();
                    return true;
                } else
                {
                    foreach(var lang in languages.Select(x => x.LanguageCulture.ToLower()))
                    {
                        if (!Statics.AvailableLanguages.Contains(lang)) {
                            Statics.AvailableLanguages.Add(lang);
                        }
                    }
                }
            }
#pragma warning disable CS0168 // Do not catch general exception types
            catch (Exception ex) {

            }
#pragma warning restore CS0168 // Do not catch general exception types

            return false;
        }

        /// <summary>
        /// private support class for displaying language arrays of items that can be used.
        /// </summary>
        public class Languages
        {
            public SortedList<string, string> SupportedLanguages { get; set; }
            public SortedList<string, string> TechnicalPossibleSupportedLanguages { get; set; }
        }

    }
}
