using System;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes.CompanyFeaturesPreferences
{

    public class CompanyFeaturesSettings : ICompanyFeaturesSettings
    {
        static ICompanyFeaturesSettings companyFeaturesSettings;

        private const string checklistsPropertyValueRegistrationEnabledKey = "checklistsPropertyValueRegistrationEnabled";
        private const string tasksPropertyValueRegistrationEnabled = "checklistsPropertyValueRegistrationEnabled";
        private const string requiredProof = "requiredProof";
        private const string tagsEnabled = "tagsEnabled";
        private const string marketUltimoEnabled = "marketUltimoEnabled";
        private const string skillAssessmentsEnabled = "skillAssessmentsEnabled";
        private const string workInstructionsEnabled = "instructionsEnabled";
        private const string checklistsEnabled = "checklistsEnabled";
        private const string tasksEnabled = "tasksEnabled";
        private const string auditsEnabled = "auditsEnabled";
        private const string actionsEnabled = "actionsEnabled";
        private const string reportsEnabled = "reportsEnabled";
        private const string factoryFeedEnabled = "factoryFeedEnabled";
        private const string actionOnTheSpotEnabled = "actionOnTheSportEnabled";
        private const string checklistsTransferableEnabled = "checklistsTransferableEnabled";

        private CompanyFeaturesSettings()
        {
        }

        public static ICompanyFeaturesSettings Instance()
        {
            if (companyFeaturesSettings == null)
            {
                companyFeaturesSettings = new CompanyFeaturesSettings();
            }

            return companyFeaturesSettings;
        }

        public bool ChecklistsPropertyValueRegistrationEnabled
        {
            get { return Preferences.Get(checklistsPropertyValueRegistrationEnabledKey, false); }
            set { Preferences.Set(checklistsPropertyValueRegistrationEnabledKey, value); }
        }

        public bool TasksPropertyValueRegistrationEnabled
        {
            get { return Preferences.Get(tasksPropertyValueRegistrationEnabled, false); }
            set { Preferences.Set(tasksPropertyValueRegistrationEnabled, value); }
        }

        public bool RequiredProof
        {
            get { return Preferences.Get(requiredProof, false); }
            set { Preferences.Set(requiredProof, value); }
        }

        public bool TagsEnabled
        {
            get { return Preferences.Get(tagsEnabled, false); }
            set { Preferences.Set(tagsEnabled, value); }
        }

        public bool MarketUltimoEnabled
        {
            get { return Preferences.Get(marketUltimoEnabled, false); }
            set { Preferences.Set(marketUltimoEnabled, value); }
        }

        public bool SkillAssessmentsEnabled
        {
            get { return Preferences.Get(skillAssessmentsEnabled, false); }
            set { Preferences.Set(skillAssessmentsEnabled, value); }
        }

        public bool WorkInstructionsEnabled
        {
            get { return Preferences.Get(workInstructionsEnabled, false); }
            set { Preferences.Set(workInstructionsEnabled, value); }
        }

        public bool ChecklistsEnabled
        {
            get { return Preferences.Get(checklistsEnabled, false); }
            set { Preferences.Set(checklistsEnabled, value); }
        }

        public bool TasksEnabled
        {
            get { return Preferences.Get(tasksEnabled, false); }
            set { Preferences.Set(tasksEnabled, value); }
        }

        public bool AuditsEnabled
        {
            get { return Preferences.Get(auditsEnabled, false); }
            set { Preferences.Set(auditsEnabled, value); }
        }

        public bool ActionsEnabled
        {
            get { return Preferences.Get(actionsEnabled, false); }
            set { Preferences.Set(actionsEnabled, value); }
        }

        public bool ReportsEnabled
        {
            get { return Preferences.Get(reportsEnabled, false); }
            set { Preferences.Set(reportsEnabled, value); }
        }

        public bool FactoryFeedEnabled
        {
            get { return Preferences.Get(factoryFeedEnabled, false); }
            set { Preferences.Set(factoryFeedEnabled, value); }
        }

        public bool ActionOnTheSpotEnabled
        {
            get { return Preferences.Get(actionOnTheSpotEnabled, false); }
            set { Preferences.Set(actionOnTheSpotEnabled, value); }
        }

        public bool ChecklistsTransferableEnabled
        {
            get { return Preferences.Get(checklistsTransferableEnabled, false); }
            set { Preferences.Set(checklistsTransferableEnabled, value); }
        }

    }

}
