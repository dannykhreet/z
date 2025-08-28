using System;
using EZGO.Maui.Core.Enumerations;
using NodaTime;

namespace EZGO.Maui.Core.Classes.SettingsPreferences.AppSettings
{
    public interface IAppSettings
    {
        ListViewLayout ListViewLayout { get; set; }
        MenuLocation SubpageTasks { get; set; }
        MenuLocation SubpageActions { get; set; }
        MenuLocation MenuLocation { get; set; }
        LocalDateTime CompletedChecklistsTimestamp { get; set; }
        LocalDateTime CompletedAudtisTimestamp { get; set; }
        LocalDateTime TasksStatussesTimestamp { get; set; }
        LocalDateTime TasksExtendedDataTimestamp { get; set; }
        LocalDateTime TasksTimestamp { get; set; }
    }
}

