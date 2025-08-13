using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes.UserSettingsPreferences;
using EZGO.Maui.Core.Utils;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes
{
    public static class UserSettings
    {
        public static IUserSettingsPrefs userSettingsPrefs { get; set; } = UserSettingsPrefs.Instance();

        private const string roleKey = "role";
        private const string roleTypeKey = "roleType";
        private const string companyNameKey = "companyNameKey";
        private const string companyLogoUrlKey = "companyLogoUrl";
        private const string userPictureUrlKey = "userPictureUrl";
        private const string fullnameKey = "fullname";
        private const string firstnameKey = "firstname";
        private const string lastnameKey = "lastname";
        private const string userEmailKey = "userEmail";
        private const string idKey = "id";
        private const string companyId = "company_id";
        private const string username = "username";
        private const string preferredLanguageKey = "preferredLanguage";

        public static string Role
        {
            get { return Preferences.Get(roleKey, string.Empty); }
            set { Preferences.Set(roleKey, value); }
        }

        public static RoleTypeEnum RoleType
        {
            get { return (RoleTypeEnum)Preferences.Get(roleTypeKey, (int)RoleTypeEnum.Basic); }
            set { Preferences.Set(roleTypeKey, (int)value); }
        }

        public static string CompanyLogoUrl
        {
            get { return Preferences.Get(companyLogoUrlKey, string.Empty); }
            set { Preferences.Set(companyLogoUrlKey, value); }
        }

        public static int CompanyId
        {
            get { return Preferences.Get(companyId, 0); }
            set { Preferences.Set(companyId, value); }
        }

        public static string CompanyName
        {
            get { return Preferences.Get(companyNameKey, string.Empty); }
            set { Preferences.Set(companyNameKey, value); }
        }

        public static string UserPictureUrl
        {
            get { return Preferences.Get(userPictureUrlKey, string.Empty); }
            set { Preferences.Set(userPictureUrlKey, value); }
        }

        public static string Fullname
        {
            get { return Preferences.Get(fullnameKey, string.Empty); }
            set { Preferences.Set(fullnameKey, value); }
        }

        public static string Firstname
        {
            get { return Preferences.Get(firstnameKey, string.Empty); }
            set { Preferences.Set(firstnameKey, value); }
        }

        public static string Lastname
        {
            get { return Preferences.Get(lastnameKey, string.Empty); }
            set { Preferences.Set(lastnameKey, value); }
        }

        public static string Email
        {
            get { return Preferences.Get(userEmailKey, string.Empty); }
            set { Preferences.Set(userEmailKey, value); }
        }

        public static int Id
        {
            get { return Preferences.Get(idKey, 0); }
            set { Preferences.Set(idKey, value); }
        }

        public static string PreferredLanguage
        {
            get { return Preferences.Get(preferredLanguageKey, string.Empty); }
            set { Preferences.Set(preferredLanguageKey, value); }
        }

        public static string Username
        {
            get => Preferences.Get(username, string.Empty);
            set => Preferences.Set(username, value);
        }
    }
}
