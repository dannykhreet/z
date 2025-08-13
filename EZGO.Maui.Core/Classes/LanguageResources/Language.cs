using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Interfaces.Data;
using EZGO.Maui.Core.Services;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Classes.LanguageResources
{
    public class Language
    {
        private const string defaultLanguageTag = "en-gb";

        public Language()
        {

        }

        /// <summary>
        /// Get resources from api based on culture set in device
        /// </summary>
        public async Task GetResourcesAsync(bool syncData = true)
        {
            try
            {
                var languageBox = new LanguageBox();
                var languageSetter = new LanguageSetter();

                string languageTag = CultureInfo.CurrentUICulture.IetfLanguageTag;

                switch (languageTag)
                {
                    case "pt-br":
                        languageTag = "pt-pt";
                        break;
                    default:
                        break;
                }

                LanguageResource language = await languageBox.ApiInsatnce.GetLanguageResource(languageTag).ConfigureAwait(false);

                if ((language?.ResourceItems?.Any() ?? false) == false)
                    language = await languageBox.ApiInsatnce.GetLanguageResource(defaultLanguageTag).ConfigureAwait(false);

                if ((language?.ResourceItems?.Any() ?? false) == false)
                    language = await languageBox.LocalInsatnce.GetLanguageResource(defaultLanguageTag).ConfigureAwait(false);

                if (language?.ResourceItems != null)
                    if (language.ResourceItems.Any())
                        languageSetter.SetLanguageDictionary(language);

                LanguageResource embeddedlanguage = languageBox.EmbeddedInsatnce.GetLanguageResource();
                if (embeddedlanguage?.ResourceItems != null)
                    languageSetter.SetDefaultLanguageResource(embeddedlanguage);

                // last resource, set the language to embedded
                if (language?.ResourceItems == null)
                    languageSetter.SetLanguageDictionary(embeddedlanguage);

                if (languageTag != Settings.CurrentLanguageTag && syncData)
                {
                    LanguageChangeService languageChange = new LanguageChangeService(languageTag);
                    await languageChange.PerformLanguageChange();
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error has occured during retriving resources {exception}", "[Languages]\n\t");
                //Crashes.TrackError(exception);
            }
        }

        /// <summary>
        /// Set Culture Info Based on Current Supported Languages if selected lanuage is not supported the default culture will be selected
        /// </summary>
        public static void SetCultureInfo()
        {
            try
            {
                if (Settings.CurrentLanguageTag != string.Empty && Settings.AvailableLanguages.Contains(Settings.CurrentLanguageTag.ToLower()))
                {
                    var culture = CultureInfo.GetCultureInfo(Settings.CurrentLanguageTag);
                    CultureInfo.CurrentUICulture = culture;
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                        System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                    });
                }
                else
                {
                    string baseCharacters = Settings.CurrentLanguageTag.Split("-")[0];

                    if (Settings.AvailableLanguages.Any(x => x.StartsWith(baseCharacters)))
                    {
                        var langTag = Settings.AvailableLanguages.FirstOrDefault(x => x.StartsWith(baseCharacters));
                        var culture = CultureInfo.GetCultureInfo(langTag);
                        CultureInfo.CurrentUICulture = culture;
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.DefaultThreadCurrentCulture = culture;
                        CultureInfo.DefaultThreadCurrentUICulture = culture;
                        Settings.CurrentLanguageTag = langTag;

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                        });
                    }
                    else
                    {
                        var culture = CultureInfo.GetCultureInfo(defaultLanguageTag);
                        CultureInfo.CurrentUICulture = culture;
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.DefaultThreadCurrentCulture = culture;
                        CultureInfo.DefaultThreadCurrentUICulture = culture;
                        Settings.CurrentLanguageTag = defaultLanguageTag;

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
                            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
                        });
                    }
                }
            }
            catch
            {
                Debug.WriteLine($"Current language tag: {Settings.CurrentLanguageTag} is not supported!", "[Language]:\n\t");
            }
        }

        public async Task SetDefaultDictionary()
        {
            using var scope = App.Container.CreateScope();
            var settingsService = scope.ServiceProvider.GetService<ISettingsService>();
            await settingsService.GetAvailableLanguagesAsync();

            Settings.CurrentLanguageTag = Settings.DefaultDeviceLanguageTag;
            SetCultureInfo();
            await GetResourcesAsync(false);
        }
    }
}
