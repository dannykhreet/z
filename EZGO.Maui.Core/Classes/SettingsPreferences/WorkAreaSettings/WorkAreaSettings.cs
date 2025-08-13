using System;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes.SettingsPreferences.WorkAreaSettings
{
    public class WorkAreaSettings : IWorkAreaSettings
    {
        static IWorkAreaSettings areaSettings;

        private const string workAreaIdKey = "workAreaId";
        private const string workAreaNameKey = "workAreaName";
        private const string reportWorkAreaIdKey = "reportWorkAreaId";
        private const string assessmentsWorkAreaIdKey = "assessmentsWorkAreaId";

        private WorkAreaSettings()
        {
        }

        public static IWorkAreaSettings Instance()
        {
            if (areaSettings == null)
            {
                areaSettings = new WorkAreaSettings();
            }

            return areaSettings;
        }

        public string WorkAreaName { get => Preferences.Get(workAreaNameKey, string.Empty); set => Preferences.Set(workAreaNameKey, value); }

        public int WorkAreaId { get => Preferences.Get(workAreaIdKey, 0); set => Preferences.Set(workAreaIdKey, value); }

        public int ReportWorkAreaId { get => Preferences.Get(reportWorkAreaIdKey, 0); set => Preferences.Set(reportWorkAreaIdKey, value); }

        public int AssessmentsWorkAreaId
        {
            get { return Preferences.Get(assessmentsWorkAreaIdKey, 0); }
            set { Preferences.Set(assessmentsWorkAreaIdKey, value); }
        }
    }
}
