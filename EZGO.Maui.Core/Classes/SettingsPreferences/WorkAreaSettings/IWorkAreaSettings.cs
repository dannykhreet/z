namespace EZGO.Maui.Core.Classes.SettingsPreferences.WorkAreaSettings
{
    public interface IWorkAreaSettings
    {
        int WorkAreaId { get; set; }
        string WorkAreaName { get; set; }
        int ReportWorkAreaId { get; set; }
        int AssessmentsWorkAreaId { get; set; }
    }
}
