namespace EZGO.Maui.Core.Classes.CompanyFeaturesPreferences
{
    public interface ICompanyFeaturesSettings
    {
        bool ChecklistsPropertyValueRegistrationEnabled { get; set; }
        bool TasksPropertyValueRegistrationEnabled { get; set; }
        bool RequiredProof { get; set; }
        bool TagsEnabled { get; set; }
        bool SkillAssessmentsEnabled { get; set; }
        bool WorkInstructionsEnabled { get; set; }
        bool MarketUltimoEnabled { get; set; }
        bool ChecklistsEnabled { get; set; }
        bool TasksEnabled { get; set; }
        bool AuditsEnabled { get; set; }
        bool ActionsEnabled { get; set; }
        bool ReportsEnabled { get; set; }
        bool FactoryFeedEnabled { get; set; }
        bool ActionOnTheSpotEnabled { get; set; }
        bool ChecklistsTransferableEnabled { get; set; }
    }
}
