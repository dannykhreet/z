namespace EZGO.Api.Models.Settings
{
    /// <summary>
    /// Feature contains several toggles to enable certain functionalities within the apps, cms and or api it self. 
    /// Depending on implementation this can be an entire full feature (audits, actions etc) or just a specific functionality (exports etc).
    /// Depending on upcoming functionalities, this class can be extended with new fields.
    /// Please make sure to create null able items.
    /// </summary>
    public class Features
    {
        #region - action related -
        /// <summary>
        /// ActionsEnabled; Actions enabled or not enabled. 
        /// </summary>
        public bool? ActionsEnabled { get; set; }
        /// <summary>
        /// ActionOnTheSportEnabled; Action on the spot (actions without direct connection to tasks, checklists or audits)  enabled or not enabled. 
        /// </summary>
        public bool? ActionOnTheSportEnabled { get; set; }
        /// <summary>
        /// ActionCommentsEnabled; Action comments, 'the action chat' enabled or not enabled.  
        /// </summary>
        public bool? ActionCommentsEnabled { get; set; }
        /// <summary>
        /// ActionPropertyEnabled; Action Properties enabled or not enabled (not implemented currently)
        /// </summary>
        public bool? ActionPropertyEnabled { get; set; }
        /// <summary>
        /// ActionPropertyValueRegistrationEnabled; Action Properties enabled or not enabled (not implemented currently)
        /// </summary>
        public bool? ActionPropertyValueRegistrationEnabled { get; set; }
        /// <summary>
        /// EasyCommentsEnabled; Comments (more or less same as actions without the limitation of due dates and certain field) enabled or not enabled
        /// </summary>
        public bool? EasyCommentsEnabled { get; set; }
        #endregion

        #region - audits -
        /// <summary>
        /// AuditsEnabled; Audits enabled or not enabled
        /// </summary>
        public bool? AuditsEnabled { get; set; }
        /// <summary>
        /// AuditsPropertyValueRegistrationEnabled; Audits property value registration enabled or not enabled (Properties with audit items);
        /// </summary>
        public bool? AuditsPropertyValueRegistrationEnabled { get; set; }
        /// <summary>
        /// AuditsPropertyEnabled; Audit properties enabled or not enabled (Properties with audits it self, e.g. open fields)
        /// </summary>
        public bool? AuditsPropertyEnabled { get; set; }
        #endregion

        #region - checklists -
        /// <summary>
        /// ChecklistsEnabled; Checklists enabled or not enabled
        /// </summary>
        public bool? ChecklistsEnabled { get; set; }
        /// <summary>
        /// ChecklistsConditionalEnabled; Checklists conditional requirements enabled or not enabled (not yet implemented)
        /// </summary>
        public bool? ChecklistsConditionalEnabled { get; set; }
        /// <summary>
        /// ChecklistsPropertyValueRegistrationEnabled; Checklist property value registration enabled or not enabled (Properties with checklist items);
        /// </summary>
        public bool? ChecklistsPropertyValueRegistrationEnabled { get; set; }
        /// <summary>
        /// ChecklistsPropertyEnabled; Checklist properties enabled or not enabled (Properties with checklists it self, e.g. open fields)
        /// </summary>
        public bool? ChecklistsPropertyEnabled { get; set; }
        /// <summary>
        /// ChecklistsTransferableEnabled; Checklist being transferable enabled or not enabled
        /// </summary>
        public bool? ChecklistsTransferableEnabled { get; set; }
        #endregion

        #region - exports -
        /// <summary>
        /// ExportEnabled; Basic exports enabled;
        /// </summary>
        public bool? ExportEnabled { get; set; }
        /// <summary>
        /// ExportAdvancedEnabled; Advanced exports enabled.
        /// </summary>
        public bool? ExportAdvancedEnabled { get; set; }
        /// <summary>
        /// ExportTaskProperties; Export Task Properties (specific export)
        /// </summary>
        public bool? ExportTaskProperties { get; set; } //for audit properties, checklist properties and task properties
        /// <summary>
        /// ExportMatrixSkillUserEnabled; matrix user skill export enabled or not.
        /// </summary>
        public bool? ExportMatrixSkillUserEnabled { get; set; }
        #endregion

        #region - general-
        /// <summary>
        /// FactoryFeedEnabled; Factory feed enabled, this is a functionality to display a feed containing several information blocks. Depending on the kind of information these will be auto generated or based on user input. 
        /// </summary>
        public bool? FactoryFeedEnabled { get; set; }
        /// <summary>
        /// RequiredProof; Required proof functionality enabled or not enabled (not yet fully implemented) is used for forcing a checklist item / task to have a required photo or photos connected to that item based on the comment structure.
        /// </summary>
        public bool? RequiredProof { get; set; }
        /// <summary>
        /// MarketEnabled; Marketplace enabled (CMS) for use enabling external / 3rd party coupled systems for use on the ease go platform
        /// </summary>
        public bool? MarketEnabled { get; set; }
        /// <summary>
        /// MarketUltimoEnabled; Enable Ultimo connector to map EZ-GO Action data to Technical Jobs in Ultimo
        /// </summary>
        public bool? MarketUltimoEnabled { get; set; }
        /// <summary>
        /// MarketSapEnabled; Enable SAP connector
        /// </summary>
        public bool? MarketSapEnabled { get; set; }
        /// <summary>
        /// QRCode; Enable QR code structure for display and retrieval.
        /// </summary>
        public bool? QRCode { get; set; }
        /// <summary>
        /// AdvancedSearch; Enable / Disable advanced search.
        /// </summary>
        public bool? AdvancedSearchEnabled { get; set; }
        /// <summary>
        /// AnalyticsEnabled; Enable / Disable analytics in clients for company.
        /// </summary>
        public bool? AnalyticsEnabled { get; set; }
        /// <summary>
        /// TemplateSharingEnabled; Enable / Disable template sharing.
        /// </summary>
        public bool? TemplateSharingEnabled { get; set; }
        /// <summary>
        /// AuditTrailDetailsEnabled; Enable / Disable Details view of audit trail.
        /// </summary>
        public bool? AuditTrailDetailsEnabled { get; set; }
        /// <summary>
        /// FlattenDataEnabled; Enable / Disable flattening of historic data.
        /// </summary>
        public bool? FlattenDataEnabled { get; set; }
        /// <summary>
        /// FlattenDataFallbackEnabled; Enable / Disable the fallback mechanic when version is unknown for flattened data.
        /// </summary>
        public bool? FlattenDataFallbackEnabled { get; set; }
        /// <summary>
        /// FlattenDataSearchEnabled; Enable / Disable the ability to search the flattened data.
        /// </summary>
        public bool? FlattenDataSearchEnabled { get; set; }
        #endregion

        #region - skills -
        /// <summary>
        /// WorkInstructions; WorkInstructions, basically checklists without input. Can be viewed by a user and coupled to tasks, checklists or audit items. (not yet implemented)
        /// </summary>
        public bool? WorkInstructions { get; set; }
        /// <summary>
        /// SkillAssessments; Skill Assessments, basically a collection of work instructions that can be scored like audits. Data will be used within the matrixes and reports.
        /// </summary>
        public bool? SkillAssessments { get; set; }
        /// <summary>
        /// SkillMatrix; Skillmatrix, a user performance matrix (cms) that can be used for advanced reporting. Will make use of assessments and general data from the system.
        /// </summary>
        public bool? SkillMatrix { get; set; }
        /// <summary>
        /// SkillMatrixMandatorySkills; Skillmatrix, a section of the user performance matrix (cms) that can be used for advanced reporting. Will make use of assessments and general data from the system.
        /// </summary>
        public bool? SkillMatrixMandatorySkills { get; set; }
        /// <summary>
        /// SkillMatrixOperationalSkills; Skillmatrix, a section of the user performance matrix (cms) that can be used for advanced reporting. Will make use of assessments and general data from the system.
        /// </summary>
        public bool? SkillMatrixOperationalSkills { get; set; }
        /// <summary>
        /// SkillMatrixOperationalBehaviour; Skillmatrix, a section of the user performance matrix (cms) that can be used for advanced reporting. Will make use of assessments and general data from the system.
        /// </summary>
        public bool? SkillMatrixOperationalBehaviour { get; set; }
        /// <summary>
        /// SkillAssessmentsRunningInCms; Running skillassessments in the CMS, a functionality that is implemented in the CMS where a user can run an assessment.
        /// </summary>
        public bool? SkillAssessmentsRunningInCms { get; set; }
        /// <summary>
        /// WorkInstructionItemAttachmentPdf; Attaching a pdf to a workinstruction item, a functionality where you can attach a pdf (or a link if other feature is enabled) to a workinstruction item.
        /// </summary>
        public bool? WorkInstructionItemAttachmentPdf { get; set; }
        /// <summary>
        /// WorkInstructionItemAttachmentLink; Attaching a link to a workinstruction item, a functionality where you can attach a link (or a pdf if other feature is enabled) to a workinstruction item.
        /// </summary>
        public bool? WorkInstructionItemAttachmentLink { get; set; }
        /// <summary>
        /// WorkInstructionsChangedNotifications; When workinstructions are changed, the user can choose to inform the end users of the work instructions.
        /// </summary>
        public bool? WorkInstructionsChangedNotificationsEnabled { get; set; }
        /// <summary>
        /// ChecklistStagesEnabled; Enables the use of stages in checklists.
        /// </summary>
        public bool? ChecklistStagesEnabled { get; set; }

        /// <summary>
        /// EnableModificationOwnCompanySettingsWorkInstruction; Enable/Disable the possibility for a manager to modify the WorkInstruction company setting(s)
        /// </summary>
        public bool? EnableModificationOwnCompanySettingsWorkInstruction { get; set; }
        /// <summary>
        /// EnableModificationOwnCompanySettingsAssessment; Enable/Disable the possibility for a manager to modify the Assessment company setting(s)
        /// </summary>
        public bool? EnableModificationOwnCompanySettingsAssessment { get; set; }
        /// <summary>
        /// EnableModificationOwnCompanySettingsMatrix; Enable/Disable the possibility for a manager to modify the Matrix company setting(s)
        /// </summary>
        public bool? EnableModificationOwnCompanySettingsMatrix { get; set; }
        /// <summary>
        /// EnableMatrixScoreStandard; Enable/Disable the possibility for a manager to change the Matrix company score to standard
        /// </summary>
        public bool? EnableMatrixScoreStandard { get; set; }

        #endregion

        #region - sort order functions -
        /// <summary>
        /// OrderAudits; Sorting mechanism for sorting audits; (CMS only)
        /// </summary>
        public bool? OrderAudits { get; set; }
        /// <summary>
        /// OrderChecklists; Sorting mechanism for sorting checklists; (CMS only)
        /// </summary>
        public bool? OrderChecklists { get; set; }
        /// <summary>
        /// OrderTasks; Sorting mechanism for sorting tasks; (CMS only)
        /// </summary>
        public bool? OrderTasks { get; set; }

        #endregion

        #region - tags -
        /// <summary>
        /// TagsEnabled; Enable the use of tags.
        /// </summary>
        public bool? TagsEnabled { get; set; }
        #endregion

        #region - tasks -
        /// <summary>
        /// TasksEnabled; Enabled the main task feature (e.g. shift tasks, week tasks, month tasks, one time only tasks)
        /// </summary>
        public bool? TasksEnabled { get; set; }
        /// <summary>
        /// TasksPropertyValueRegistrationEnabled; Enable the property value registration with tasks. 
        /// </summary>
        public bool? TasksPropertyValueRegistrationEnabled { get; set; }
        /// <summary>
        /// TasksPropertyEnabled; Enable other properties with tasks (currently implemented as value registration, but will be used if specific custom properties will be added to task templates). 
        /// </summary>
        public bool? TasksPropertyEnabled { get; set; }
        #endregion

        #region - reports -
        /// <summary>
        /// ReportsEnabled; Reports enabled or not. Basic reporting in app and other tooling.
        /// </summary>
        public bool? ReportsEnabled { get; set; }
        #endregion

        #region - technical -
        /// <summary>
        /// FirebaseLoggingCMSEnabled; Firebase logging enabled (CMS only)
        /// </summary>
        public bool? FirebaseLoggingCMSEnabled { get; set; }
        /// <summary>
        /// UseStaticAuditStorage; Technical feature for generating static audit storage; currently handled through configuration or direct query
        /// </summary>
        public bool? UseStaticAuditStorage { get; set; }
        /// <summary>
        /// UseStaticChecklistStorage; Technical feature for generating static checklist storage; currently handled through configuration or direct query
        /// </summary>
        public bool? UseStaticChecklistStorage { get; set; }

        #endregion

        #region - user -
        /// <summary>
        /// UserProfileManagementEnabled; User profile management enabled or not
        /// </summary>
        public bool? UserProfileManagementEnabled { get; set; }
        public bool? UserExtendedDetailsEnabled { get; set; }
        #endregion

        #region - support -
        /// <summary>
        /// Enable support chat method (currenlty for CMS only)
        /// </summary>
        public bool? EnableSupportChat { get; set; }
        #endregion

        #region - task generation types -
        public bool? TaskTypeNoRecurrencyEnabled { get; set; }

        public bool? TaskTypeShiftEnabled { get; set; }

        public bool? TaskTypeMonthEnabled { get; set; }

        public bool? TaskTypeWeekEnabled { get; set; }

        public bool? TaskTypePeriodDayEnabled { get; set; }

        public bool? TaskTypePeriodHourEnabled { get; set; }

        public bool? TaskTypePeriodMinuteEnabled { get; set; }

        public bool? TaskTypeDynamicDayEnabled { get; set; }

        public bool? TaskTypeDynamicHourEnabled { get; set; }

        public bool? TaskTypeDynamicMinuteEnabled { get; set; }
        #endregion

        #region - other -
        public bool? TaskMultiskipEnabled { get; set; }

        public bool? TaskGenerationOptions { get; set; }

        public bool? RoleManagementEnabled { get; set; }

        public bool? AtossProvisioningEnabled { get; set; }
        #endregion

        #region - tiers -
        public bool? TierEssentials { get; set; }
        public bool? TierAdvanced { get; set; }
        public bool? TierPremium { get; set; }
        #endregion

    }
}
