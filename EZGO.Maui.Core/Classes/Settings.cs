using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Enumerations;
using NodaTime;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using EZGO.Maui.Core.Classes.SettingsPreferences.WorkAreaSettings;
using EZGO.Maui.Core.Classes.SettingsPreferences.AWSSettings;
using EZGO.Maui.Core.Classes.SettingsPreferences.AppSettings;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes
{
    public static class Settings
    {
        private const string tokenKey = "token";
        private const string workAreaIdKey = "workAreaId";
        private const string workAreaNameKey = "workAreaName";
        private const string reportWorkAreaIdKey = "reportWorkAreaId";
        private const string assessmentsWorkAreaIdKey = "assessmentsWorkAreaId";
        private const string allTaskWorkAreaIdKey = "allTaskWorkAreaId";
        private const string menuLocationKey = "menuLocation";
        private const string listViewLayoutKey = "listViewLayout";
        private const string downloadMediaKey = "downloadMedia";
        private const string lastUpdateCheckKey = "lastUpdateCheck";
        private const string lastChatUpdateCheckKey = "lastChatUpdateCheck";
        private const string lastTasksUpdateCheckKey = "lastTasksUpdateCheck";
        private const string lastAssessmentsUpdateCheckKey = "lastAssessmentsUpdateCheck";
        private const string lastTaskCommentsUpdateCheckKey = "lastTaskCommentsUpdateCheck";
        private const string lastPropertyValuesUpdateCheckKey = "lastPropertyValuesUpdateCheck";
        private const string lastEzFeedUpdateCheckKey = "lastEzFeedUpdateCheckKey";
        private const string lastEzFeedCommentsUpdateCheckKey = "lastEzFeedCommentsUpdateCheckKey";
        private const string reportIntervalKey = "reportInterval";
        private const string tasksTimestampKey = "tasksTimestamp";
        private const string tasksOverviewTimestampKey = "tasksOverviewTimestamp";
        private const string tasksStatussesTimestamp = "tasksStatussesTimestamp";
        private const string tasksExtendedTimestamp = "tasksExtendedTimestamp";
        private const string reportSubpageKey = "reportSubpage";
        private const string taskSubpageKey = "taskSubpage";
        private const string actionSubpageKey = "actionSubpageKey";
        private const string reportAuditKey = "reportAuditId";
        private const string completedChecklistsTimestamp = "completedChecklistsTimestamp";
        private const string completedAuditsTimestamp = "completedAuditsTimestamp";
        private const string actionsTimestamp = "actionsTimestamp";
        private const string lastCheckedShiftId = "lastCheckedShiftId";
        private const string hasApplicationCrashed = "hasApplicationCrashed";
        private const string currentLanguageTag = "currentLanguageTag";
        private const string tabIndex = "tabIndex";
        private const string msalAccessToken = "msalAccessToken";
        private const string availableLanguages = "availableLanguages";



        public static IWorkAreaSettings AreaSettings { get; set; } = WorkAreaSettings.Instance();
        public static IAWSSettings AWSSettings { get; set; } = SettingsPreferences.AWSSettings.AWSSettings.Instance();
        public static IAppSettings AppSettings { get; set; } = SettingsPreferences.AppSettings.AppSettings.Instance();



        public static List<string> AvailableLanguages
        {
            get
            {
                var deserializedList = JsonSerializer.Deserialize<List<string>>(Preferences.Get(availableLanguages, string.Empty));
                return deserializedList ?? new List<string>();
            }
            set
            {
                var serializedList = JsonSerializer.Serialize(value);
                Preferences.Set(availableLanguages, serializedList);
            }
        }


        public static string Token
        {
            get { return Preferences.Get(tokenKey, string.Empty); }
            set { Preferences.Set(tokenKey, value); }
        }

        public static bool HasCrashed
        {
            get { return Preferences.Get(hasApplicationCrashed, false); }
            set { Preferences.Set(hasApplicationCrashed, value); }
        }

        public static int WorkAreaId
        {
            get { return Preferences.Get(workAreaIdKey, 0); }
            set { Preferences.Set(workAreaIdKey, value); }
        }

        public static bool IsWorkAreaSelected => WorkAreaId > 0;

        public static string WorkAreaName
        {
            get { return Preferences.Get(workAreaNameKey, string.Empty); }
            set { Preferences.Set(workAreaNameKey, value); }
        }

        public static int ReportWorkAreaId
        {
            get { return Preferences.Get(reportWorkAreaIdKey, 0); }
            set { Preferences.Set(reportWorkAreaIdKey, value); }
        }

        public static int AssessmentsWorkAreaId
        {
            get { return Preferences.Get(assessmentsWorkAreaIdKey, 0); }
            set { Preferences.Set(assessmentsWorkAreaIdKey, value); }
        }

        public static int AllTaskWorkAreaId
        {
            get { return Preferences.Get(allTaskWorkAreaIdKey, 0); }
            set { Preferences.Set(allTaskWorkAreaIdKey, value); }
        }

        public static TimespanTypeEnum ReportInterval
        {
            get { return (TimespanTypeEnum)Preferences.Get(reportIntervalKey, (int)TimespanTypeEnum.LastTwelveDays); }
            set { Preferences.Set(reportIntervalKey, (int)value); }
        }

        public static MenuLocation MenuLocation
        {
            get { return (MenuLocation)Preferences.Get(menuLocationKey, (int)MenuLocation.None); }
            set { Preferences.Set(menuLocationKey, (int)value); }
        }

        public static MenuLocation SubpageReporting
        {
            get { return (MenuLocation)Preferences.Get(reportSubpageKey, (int)MenuLocation.Report); }
            set { Preferences.Set(reportSubpageKey, (int)value); }
        }

        public static MenuLocation SubpageTasks
        {
            get { return (MenuLocation)Preferences.Get(taskSubpageKey, (int)MenuLocation.Tasks); }
            set { Preferences.Set(taskSubpageKey, (int)value); }
        }

        public static MenuLocation SubpageActions
        {
            get { return (MenuLocation)Preferences.Get(actionSubpageKey, (int)MenuLocation.Actions); }
            set { Preferences.Set(actionSubpageKey, (int)value); }
        }

        public static int ReportAuditId
        {
            get { return Preferences.Get(reportAuditKey, 0); }
            set { Preferences.Set(reportAuditKey, value); }
        }

        public static ListViewLayout ListViewLayout
        {
            get { return (ListViewLayout)Preferences.Get(listViewLayoutKey, (int)ListViewLayout.Grid); }
            set
            {
                Preferences.Set(listViewLayoutKey, (int)value);
            }
        }

        public static bool DownloadMedia
        {
            get { return Preferences.Get(downloadMediaKey, false); }
            set { Preferences.Set(downloadMediaKey, value); }
        }

        public static LocalDateTime LastUpdateCheck
        {
            get
            {
                return ConvertDateTimeToLocal(Preferences.Get(lastUpdateCheckKey, DateTime.UtcNow));
            }
            set { Preferences.Set(lastUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastChatUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastChatUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastChatUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastTasksUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastTasksUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastTasksUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastAssessmentsUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastAssessmentsUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastAssessmentsUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastEzFeedUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastEzFeedUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastEzFeedUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastTaskCommentUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastTaskCommentsUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastTaskCommentsUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastEzFeedCommentsUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastEzFeedCommentsUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastEzFeedCommentsUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime LastPropertyValuesUpdateCheck
        {
            get { return ConvertDateTimeToLocal(Preferences.Get(lastPropertyValuesUpdateCheckKey, DateTime.UtcNow)); }
            set { Preferences.Set(lastPropertyValuesUpdateCheckKey, value.ToDateTimeUnspecified()); }
        }

        public static LocalDateTime TasksTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksTimestampKey, DateTime.Now));
            set => Preferences.Set(tasksTimestampKey, value.ToDateTimeUnspecified());
        }

        public static LocalDateTime TasksOverviewTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksOverviewTimestampKey, DateTime.Now));
            set => Preferences.Set(tasksOverviewTimestampKey, value.ToDateTimeUnspecified());
        }

        public static LocalDateTime TasksStatussesTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksStatussesTimestamp, DateTime.Now));
            set => Preferences.Set(tasksStatussesTimestamp, value.ToDateTimeUnspecified());
        }

        public static LocalDateTime TasksExtendedDataTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksExtendedTimestamp, DateTime.Now));
            set => Preferences.Set(tasksExtendedTimestamp, value.ToDateTimeUnspecified());
        }

        public static LocalDateTime CompletedChecklistsTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(completedChecklistsTimestamp, DateTime.Now));
            set => Preferences.Set(completedChecklistsTimestamp, value.ToDateTimeUnspecified());
        }

        public static LocalDateTime ActionsTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(actionsTimestamp, DateTime.Now));
            set => Preferences.Set(actionsTimestamp, value.ToDateTimeUnspecified());
        }

        public static LocalDateTime CompletedAudtisTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(completedAuditsTimestamp, DateTime.Now));
            set => Preferences.Set(completedAuditsTimestamp, value.ToDateTimeUnspecified());
        }

        public static int LastCheckedShiftId
        {
            get => Preferences.Get(lastCheckedShiftId, -1);
            set => Preferences.Set(lastCheckedShiftId, value);
        }

        public static string CurrentLanguageTag
        {
            get => Preferences.Get(currentLanguageTag, string.Empty);
            set => Preferences.Set(currentLanguageTag, value);
        }

        public static bool IsRightToLeftLanguage
        {
            get => System.Globalization.CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;
        }


        public static string DefaultDeviceLanguageTag { get; set; } = "en-gb";

        public static async Task SetMsalAccessTokenAsync(string value)
        {
            await SecureStorage.SetAsync(msalAccessToken, value);
        }

        public static async Task<string> GetMsalAccessTokenAsync()
        {
            return await SecureStorage.GetAsync(msalAccessToken);
        }

        public static void RemoveMsalAccessToken()
        {
            SecureStorage.Remove(msalAccessToken);
        }

        public static int TabIndex
        {
            get => Preferences.Get(tabIndex, 1);
            set => Preferences.Set(tabIndex, value);
        }

        public static void UpdateDates()
        {
            LastUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastChatUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastTasksUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastAssessmentsUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastEzFeedUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastTaskCommentUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastPropertyValuesUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            LastEzFeedCommentsUpdateCheck = ConvertDateTimeToLocal(DateTime.UtcNow);
            TasksTimestamp = ConvertDateTimeToLocal(DateTime.Now);
            TasksOverviewTimestamp = ConvertDateTimeToLocal(DateTime.Now);
            TasksStatussesTimestamp = ConvertDateTimeToLocal(DateTime.Now);
            TasksExtendedDataTimestamp = ConvertDateTimeToLocal(DateTime.Now);
            CompletedChecklistsTimestamp = ConvertDateTimeToLocal(DateTime.Now);
            ActionsTimestamp = ConvertDateTimeToLocal(DateTime.Now);
            CompletedAudtisTimestamp = ConvertDateTimeToLocal(DateTime.Now);
        }

        public static LocalDateTime ConvertDateTimeToLocal(DateTime date)
        {
            return LocalDateTime.FromDateTime(date, CalendarSystem.Iso);
        }

        public static LocalDateTime? ConvertDateTimeToLocal(DateTime? date)
        {
            try
            {
                return ConvertDateTimeToLocal(date.Value);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
