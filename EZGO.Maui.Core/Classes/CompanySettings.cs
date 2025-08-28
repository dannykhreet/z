using EZGO.Api.Models.Settings;
using EZGO.Maui.Core.Classes.CompanyFeaturesPreferences;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Storage;

namespace EZGO.Maui.Core.Classes
{
    public class CompanySettings : ApplicationSettings
    {
        public MediaLocations MediaLocations { get; set; }

        public void ToSettings()
        {
            CompanyFeatures.PasswordValidationRegEx = PasswordValidationRegEx;
            CompanyFeatures.CompanyFeatSettings.ActionsEnabled = Features.ActionsEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.ActionOnTheSpotEnabled = Features.ActionOnTheSportEnabled ?? false;
            CompanyFeatures.ActionCommentsEnabled = Features.ActionCommentsEnabled ?? false;
            CompanyFeatures.ActionPropertyEnabled = Features.ActionPropertyEnabled ?? false;
            CompanyFeatures.ActionPropertyValueRegistrationEnabled = Features.ActionPropertyValueRegistrationEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.AuditsEnabled = Features.AuditsEnabled ?? false;
            CompanyFeatures.AuditsPropertyValueRegistrationEnabled = Features.AuditsPropertyValueRegistrationEnabled ?? false;
            CompanyFeatures.AuditsPropertyEnabled = Features.AuditsPropertyEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.ChecklistsEnabled = Features.ChecklistsEnabled ?? false;
            CompanyFeatures.ChecklistsConditionalEnabled = Features.ChecklistsConditionalEnabled ?? false;
            CompanyFeatures.ChecklistsPropertyEnabled = Features.ChecklistsPropertyEnabled ?? false;
            CompanyFeatures.EasyCommentsEnabled = Features.EasyCommentsEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.FactoryFeedEnabled = Features.FactoryFeedEnabled ?? false;
            CompanyFeatures.RequiredProof = Features.RequiredProof ?? false;
            CompanyFeatures.CompanyFeatSettings.TasksEnabled = Features.TasksEnabled ?? false;
            CompanyFeatures.TasksPropertyValueRegistrationEnabled = Features.TasksPropertyValueRegistrationEnabled ?? false;
            CompanyFeatures.TasksPropertyEnabled = Features.TasksPropertyEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.ReportsEnabled = Features.ReportsEnabled ?? false;
            CompanyFeatures.UserProfileManagementEnabled = Features.UserProfileManagementEnabled ?? false;
            CompanyFeatures.ImageStorageBaseUrl = MediaLocations.ImageMediaBaseUri;
            CompanyFeatures.VideoStorageBaseUrl = MediaLocations.VideoMediaBaseUri;
            CompanyFeatures.CompanyFeatSettings.SkillAssessmentsEnabled = Features.SkillAssessments ?? false;
            CompanyFeatures.CompanyFeatSettings.WorkInstructionsEnabled = Features.WorkInstructions ?? false;
            CompanyFeatures.TaskMultiskipEnabled = Features.TaskMultiskipEnabled ?? false;
            CompanyFeatures.QRCode = Features.QRCode ?? false;
            CompanyFeatures.CompanyFeatSettings.ChecklistsPropertyValueRegistrationEnabled = Features.ChecklistsPropertyValueRegistrationEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.TagsEnabled = Features.TagsEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.MarketUltimoEnabled = Features.MarketUltimoEnabled ?? false;
            CompanyFeatures.CompanyFeatSettings.ChecklistsTransferableEnabled = Features.ChecklistsTransferableEnabled ?? false;
        }
    }

    public static class CompanyFeatures
    {
        public static ICompanyFeaturesSettings CompanyFeatSettings { get; set; } = CompanyFeaturesSettings.Instance();


        private const string passwordValidationRegEx = "passwordValidationRegEx";
        public static string PasswordValidationRegEx
        {
            get { return Preferences.Get(passwordValidationRegEx, string.Empty); }
            set { Preferences.Set(passwordValidationRegEx, value); }
        }

        private const string imageStorageBaseUrl = nameof(imageStorageBaseUrl);
        public static string ImageStorageBaseUrl
        {
            get { return Preferences.Get(imageStorageBaseUrl, string.Empty); }
            set { Preferences.Set(imageStorageBaseUrl, value); }
        }

        private const string videoStorageBaseUrl = nameof(videoStorageBaseUrl);
        public static string VideoStorageBaseUrl
        {
            get { return Preferences.Get(videoStorageBaseUrl, string.Empty); }
            set { Preferences.Set(videoStorageBaseUrl, value); }
        }


        #region features        

        #region actions
        private const string actionCommentsEnabled = "actionCommentsEnabled";
        public static bool ActionCommentsEnabled
        {
            get { return Preferences.Get(actionCommentsEnabled, false); }
            set { Preferences.Set(actionCommentsEnabled, value); }
        }

        private const string actionPropertyEnabled = "actionPropertyEnabled";
        public static bool ActionPropertyEnabled
        {
            get { return Preferences.Get(actionPropertyEnabled, false); }
            set { Preferences.Set(actionPropertyEnabled, value); }
        }

        private const string actionPropertyValueRegistrationEnabled = "actionPropertyValueRegistrationEnabled";
        public static bool ActionPropertyValueRegistrationEnabled
        {
            get { return Preferences.Get(actionPropertyValueRegistrationEnabled, false); }
            set { Preferences.Set(actionPropertyValueRegistrationEnabled, value); }
        }
        #endregion

        #region assessments
        private const string skillAssessmentsEnabled = "skillAssessmentsEnabled";
        public static bool SkillAssessmentsEnabled
        {
            get { return Preferences.Get(skillAssessmentsEnabled, false); }
            set { Preferences.Set(skillAssessmentsEnabled, value); }
        }

        #endregion

        #region audits
        private const string auditsPropertyValueRegistrationEnabled = "auditsPropertyValueRegistrationEnabled";
        public static bool AuditsPropertyValueRegistrationEnabled
        {
            get { return Preferences.Get(auditsPropertyValueRegistrationEnabled, false); }
            set { Preferences.Set(auditsPropertyValueRegistrationEnabled, value); }
        }

        private const string auditsPropertyEnabled = "auditsPropertyEnabled";
        public static bool AuditsPropertyEnabled
        {
            get { return Preferences.Get(auditsPropertyEnabled, false); }
            set { Preferences.Set(auditsPropertyEnabled, value); }
        }

        #endregion


        #region checklists       

        private const string checklistsConditionalEnabled = "checklistsConditionalEnabled";
        public static bool ChecklistsConditionalEnabled
        {
            get { return Preferences.Get(checklistsConditionalEnabled, false); }
            set { Preferences.Set(checklistsConditionalEnabled, value); }
        }

        private const string checklistsPropertyEnabled = "checklistsPropertyEnabled";
        public static bool ChecklistsPropertyEnabled
        {
            get { return Preferences.Get(checklistsPropertyEnabled, false); }
            set { Preferences.Set(checklistsPropertyEnabled, value); }
        }

        #endregion

        private const string easyCommentsEnabled = "easyCommentsEnabled";
        public static bool EasyCommentsEnabled
        {
            get { return Preferences.Get(easyCommentsEnabled, false); }
            set { Preferences.Set(easyCommentsEnabled, value); }
        }



        private const string requiredProof = "requiredProof";
        public static bool RequiredProof
        {
            get { return Preferences.Get(requiredProof, false); }
            set { Preferences.Set(requiredProof, value); }
        }


        #region tasks
        private const string tasksPropertyValueRegistrationEnabled = "tasksPropertyValueRegistrationEnabled";
        public static bool TasksPropertyValueRegistrationEnabled
        {
            get { return Preferences.Get(tasksPropertyValueRegistrationEnabled, false); }
            set { Preferences.Set(tasksPropertyValueRegistrationEnabled, value); }
        }

        private const string tasksPropertyEnabled = "tasksPropertyEnabled";
        public static bool TasksPropertyEnabled
        {
            get { return Preferences.Get(tasksPropertyEnabled, false); }
            set { Preferences.Set(tasksPropertyEnabled, value); }
        }

        private const string taskMultiskipEnabled = "taskMultiskipEnabled";
        public static bool TaskMultiskipEnabled
        {
            get { return Preferences.Get(taskMultiskipEnabled, false); }
            set { Preferences.Set(taskMultiskipEnabled, value); }
        }
        #endregion

        private const string userProfileManagementEnabled = "userProfileManagementEnabled";
        public static bool UserProfileManagementEnabled
        {
            get { return Preferences.Get(userProfileManagementEnabled, false); }
            set { Preferences.Set(userProfileManagementEnabled, value); }
        }

        #region qr code
        private const string qRCode = "qRCode";
        public static bool QRCode
        {
            get { return Preferences.Get(qRCode, false); }
            set { Preferences.Set(qRCode, value); }
        }
        #endregion

        public static bool IsActionButtonVisible => CompanyFeatSettings.ActionsEnabled || EasyCommentsEnabled;

        //public bool? TasksPropertyEnabled { get; set; }
        //public bool? TasksPropertyValueRegistrationEnabled { get; set; }
        //public bool? TasksEnabled { get; set; }
        //public bool? ExportEnabled { get; set; }
        //public bool? ChecklistsPropertyEnabled { get; set; }
        //public bool? ChecklistsPropertyValueRegistrationEnabled { get; set; }
        //public bool? ChecklistsConditionalEnabled { get; set; }
        //public bool? ChecklistsEnabled { get; set; }
        //public bool? AuditsPropertyEnabled { get; set; }
        //public bool? AuditsPropertyValueRegistrationEnabled { get; set; }
        //public bool? AuditsEnabled { get; set; }
        //public bool? EasyCommentsEnabled { get; set; }
        //public bool? ActionPropertyValueRegistrationEnabled { get; set; }
        //public bool? ActionPropertyEnabled { get; set; }
        //public bool? ActionCommentsEnabled { get; set; }
        //public bool? ActionOnTheSportEnabled { get; set; }
        //public bool? ActionsEnabled { get; set; }
        //public bool? ReportsEnabled { get; set; }
        //public bool? UserProfileManagementEnabled { get; set; }

        #endregion
    }
}
