using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Classes
{
    public static class ValidationHelper
    {
        private static Page GetCurrentPage()
        {
            Page page = null;

            if (Application.Current.MainPage?.Navigation.NavigationStack != null)
                page = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

            return page;
        }

        #region Validation Popups
        private static async Task DisplayValidationPopup(string message)
        {
            Page page = GetCurrentPage();
            string cancel = TranslateExtension.GetValueFromDictionary(LanguageConstants.baseTextCancel);
            await page.DisplayActionSheet(message, null, cancel);
        }

        public static async Task DisplayActionChatValidationPopup()
        {
            var translated = TranslateExtension.GetValueFromDictionary(LanguageConstants.maxNumberOfAttachments);
            await DisplayValidationPopup(translated);
        }
        #endregion

        public static async Task DisplayGeneralValidationPopup(string message)
        {
            await DisplayValidationPopup(message);
        }
    }
}

