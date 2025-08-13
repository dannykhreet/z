using System;
using EZGO.Api.Models.Enumerations;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes.UserSettingsPreferences
{
    public class UserSettingsPrefs : IUserSettingsPrefs
    {
        static IUserSettingsPrefs userSettingsPrefs;

        private const string roleKey = "role";
        private const string companyId = "company_id";
        private const string roleTypeKey = "roleType";
        private const string idKey = "id";
        private const string fullnameKey = "fullname";
        private const string userPictureUrlKey = "userPictureUrl";
        private const string companyLogoUrlKey = "companyLogoUrl";
        private const string companyNameKey = "companyNameKey";

        private UserSettingsPrefs()
        {
        }

        public static IUserSettingsPrefs Instance()
        {
            if (userSettingsPrefs == null)
            {
                userSettingsPrefs = new UserSettingsPrefs();
            }

            return userSettingsPrefs;
        }

        public string Role
        {
            get { return Preferences.Get(roleKey, string.Empty); }
            set { Preferences.Set(roleKey, value); }
        }

        public RoleTypeEnum RoleType
        {
            get { return (RoleTypeEnum)Preferences.Get(roleTypeKey, (int)RoleTypeEnum.Basic); }
            set { Preferences.Set(roleTypeKey, (int)value); }
        }

        public int CompanyId
        {
            get { return Preferences.Get(companyId, 0); }
            set { Preferences.Set(companyId, value); }
        }

        public int Id
        {
            get { return Preferences.Get(idKey, 0); }
            set { Preferences.Set(idKey, value); }
        }

        public string Fullname
        {
            get { return Preferences.Get(fullnameKey, string.Empty); }
            set { Preferences.Set(fullnameKey, value); }
        }

        public string UserPictureUrl
        {
            get { return Preferences.Get(userPictureUrlKey, string.Empty); }
            set { Preferences.Set(userPictureUrlKey, value); }
        }

        public string CompanyLogoUrl
        {
            get { return Preferences.Get(companyLogoUrlKey, string.Empty); }
            set { Preferences.Set(companyLogoUrlKey, value); }
        }

        public string CompanyName
        {
            get { return Preferences.Get(companyNameKey, string.Empty); }
            set { Preferences.Set(companyNameKey, value); }
        }
    }
}
