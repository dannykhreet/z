using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    //TODO: move all setting resources to own manager

    /// <summary>
    /// IGeneralManager, Interface for use with the GeneralManager.
    /// This interface is needed for .NetCore3.1 services and possible tests.
    /// </summary>
    public interface IGeneralManager
    {
        Task<MainMenu> GetMainMenuAsync(int companyId, DateTime timestamp, int? areaId = null, int? userId = null);
        Task<MainMenuItem> GetMenuItemAsync(int companyId, MenuTypeEnum menuType, DateTime? timestamp = null, bool useStatistics = false, int? areaId = null, int? userId = null);
        Task<List<StatsItem>> GetStatisticsAsync(int companyId, DateTime timestamp, int? areaId = null, int? userId = null);
        Task<ApplicationSettings> GetApplicationSettings(int companyid, int? userid = null);
        Task<List<UpdateCheckItem>> GetUpdateChangesAsync(int companyId, int? userId = null, int? areaId = null, DateTime? timestamp = null);
        Task<List<UpdateCheckItem>> GetUpdateChangesFeedAsync(int companyId, int? userId = null, int? areaId = null, DateTime? timestamp = null);
        Task<Features> GetFeatures(int companyId, int? userId);
        Task<bool> GetHasAccessToFeatureByCompany(int companyId, string featurekey);
        Task<bool> GetIsSetInSetting(string value, string settingkey);
        Task<LanguageResource> GetLanguageResource(string culture, ResourceLanguageTypeEnum resourceType = ResourceLanguageTypeEnum.APP);
        Task<List<StatsItem>> GetLanguageResourceStatistics();
        Task<List<SettingResource>> GetSettingResources(int companyid, string include = null);
        Task<List<SettingResource>> GetFeatureSettingResources();
        Task<Features> GetFeatures(List<SettingResource> resourceSettings, int companyId);
        Task<SettingResource> GetSettingResourceByKey(string resourceKey);
        Task<Tuple<bool, List<string>>> TryUpdateCompanySetting(int companyid, int userId, SettingResourceItemTrueFalse resourceSettingToUpdate);
        Task<List<SettingResource>> GetSettingResourcesForSettings(int companyid);
        Task<List<Announcement>> GetAnnouncements(int companyId, AnnouncementTypeEnum announcementType = AnnouncementTypeEnum.ReleaseNotes,int limit = 0, int offset = 0);
        Task<(DateTime? start, DateTime? end)> GetCreatedPeriodByRangeAsync(int companyId, string rangePeriod);
        Task<int> AddAnnouncement(Announcement announcement);
        Task<bool> ChangeAnnouncement(int announcementId, Announcement announcement);
        Task<bool> SetAnnouncementActiveAsync(int companyId, int userId, int announcementId, bool isActive = true);
        Task<int> AddSettingResourceCompany(int companyid, SettingResourceItem setting);
        Task<bool> ChangeSettingResourceCompany(int companyid, SettingResourceItem setting, bool encryptValue = false);
        Task<bool> ChangeSettingResourceHolding(int holdingId, SettingResourceItem setting, bool encryptValue = false);
        Task<bool> ChangeSettingResource(int companyId, int userId, int id, string value);
        Task<bool> UpdateLanguageKeyAsync(string key, string culture, string value, int companyId, int userId);
        Task<bool> UpdateDescriptionKeyAsync(string key, string description);
        Task<bool> AddLanguageKeyAsync(string key, string description);
        Task<bool> AddLanguageKeyAsync(string key, string description, string culture, string value);
        Task<List<SettingResourceItem>> GetSettingResourceItemForCompany(int companyid);
        Task<string> GetSettingValueForCompanyByResourceId(int companyid, int resourcesettingid);
        Task<string> GetSettingValueForCompanyOrHoldingByResourceId(int companyid, int resourcesettingid, bool decryptValue = false);
        Task<bool> DoTestCall(bool useError);
        List<Exception> GetPossibleExceptions();
    }
}

//TODO Move language related resources to own manager or logic parts.