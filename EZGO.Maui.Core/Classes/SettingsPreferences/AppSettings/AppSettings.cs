using System;
using EZGO.Maui.Core.Classes.SettingsPreferences.WorkAreaSettings;
using EZGO.Maui.Core.Enumerations;
using NodaTime;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes.SettingsPreferences.AppSettings
{
    public class AppSettings : IAppSettings
    {
        static IAppSettings appSettings;

        private const string listViewLayoutKey = "listViewLayout";
        private const string taskSubpageKey = "taskSubpage";
        private const string menuLocationKey = "menuLocation";
        private const string actionSubpageKey = "actionSubpageKey";
        private const string completedAuditsTimestamp = "completedAuditsTimestamp";
        private const string completedChecklistsTimestamp = "completedChecklistsTimestamp";
        private const string tasksStatussesTimestamp = "tasksStatussesTimestamp";
        private const string tasksExtendedTimestamp = "tasksExtendedTimestamp";
        private const string tasksTimestampKey = "tasksTimestamp";

        private AppSettings()
        {
        }

        public static IAppSettings Instance()
        {
            if (appSettings == null)
            {
                appSettings = new AppSettings();
            }

            return appSettings;
        }

        public ListViewLayout ListViewLayout
        {
            get { return (ListViewLayout)Preferences.Get(listViewLayoutKey, (int)ListViewLayout.Grid); }
            set
            {
                Preferences.Set(listViewLayoutKey, (int)value);
            }
        }


        public MenuLocation SubpageTasks
        {
            get { return (MenuLocation)Preferences.Get(taskSubpageKey, (int)MenuLocation.Tasks); }
            set { Preferences.Set(taskSubpageKey, (int)value); }
        }


        public MenuLocation SubpageActions
        {
            get { return (MenuLocation)Preferences.Get(actionSubpageKey, (int)MenuLocation.Actions); }
            set { Preferences.Set(actionSubpageKey, (int)value); }
        }

        public MenuLocation MenuLocation
        {
            get { return (MenuLocation)Preferences.Get(menuLocationKey, (int)MenuLocation.None); }
            set { Preferences.Set(menuLocationKey, (int)value); }
        }

        public LocalDateTime CompletedChecklistsTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(completedChecklistsTimestamp, DateTime.Now));
            set => Preferences.Set(completedChecklistsTimestamp, value.ToDateTimeUnspecified());
        }

        public LocalDateTime CompletedAudtisTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(completedAuditsTimestamp, DateTime.Now));
            set => Preferences.Set(completedAuditsTimestamp, value.ToDateTimeUnspecified());
        }

        public LocalDateTime TasksStatussesTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksStatussesTimestamp, DateTime.Now));
            set => Preferences.Set(tasksStatussesTimestamp, value.ToDateTimeUnspecified());
        }

        public LocalDateTime TasksExtendedDataTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksExtendedTimestamp, DateTime.Now));
            set => Preferences.Set(tasksExtendedTimestamp, value.ToDateTimeUnspecified());
        }

        public LocalDateTime TasksTimestamp
        {
            get => ConvertDateTimeToLocal(Preferences.Get(tasksTimestampKey, DateTime.Now));
            set => Preferences.Set(tasksTimestampKey, value.ToDateTimeUnspecified());
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

