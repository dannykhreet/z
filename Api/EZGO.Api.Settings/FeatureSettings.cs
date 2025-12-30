using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    /// <summary>
    /// FeatureSettings; Contains settings specific to certain features and or for use with the feature mechanism
    /// </summary>
    public static class FeatureSettings
    {
        public const string FEATURE_EXPORTTASKTEMPLATES_CONFIG_KEY = "Features:ExportTaskTemplates";
        public const string FEATURE_EXPORTAUDITSCHECKLISTS_CONFIG_KEY = "Features:ExportAuditsChecklists";
        public const string FEATURE_EXPORTAUDITTEMPLATESCHECKLISTTEMPLATES_CONFIG_KEY = "Features:ExportAuditTemplatesChecklistTemplates";
        public const string FEATURE_VALUE_REGISTRATION_TASKS_CONFIG_KEY = "Features:ValueRegistrationTasks";

        public const string FEATURE_AUDITS = "FEATURE_AUDITS";
        public const string FEATURE_CHECKLISTS = "FEATURE_CHECKLISTS";
        public const string FEATURE_TASKS = "FEATURE_TASKS";
        public const string FEATURE_ACTIONS = "FEATURE_ACTIONS";
        public const string FEATURE_TASKTEMPLATEPROPERTIES = "FEATURE_TASKTEMPLATEPROPERTIES";
        public const string FEATURE_TASKTEMPLATEPROPERTIES_CHECKLISTS = "FEATURE_TASKTEMPLATEPROPERTIES_CHECKLISTS";
        public const string FEATURE_TASKTEMPLATEPROPERTIES_AUDITS = "FEATURE_TASKTEMPLATEPROPERTIES_AUDITS";
        public const string FEATURE_CHECKLISTTEMPLATEPROPERTIES = "FEATURE_CHECKLISTTEMPLATEPROPERTIES";
        public const string FEATURE_AUDITTEMPLATEPROPERTIES = "FEATURE_AUDITTEMPLATEPROPERTIES";
        public const string FEATURE_ACTIONONTHESPOT = "FEATURE_ACTIONONTHESPOT";
        public const string FEATURE_COMMENT = "FEATURE_COMMENT";
        public const string FEATURE_REQUIREPROOF = "FEATURE_REQUIREPROOF";
        public const string FEATURE_FACTORY_FEED = "FEATURE_FACTORY_FEED";

        public const string TECH_FEATURE_USE_STATIC_AUDIT_STORAGE = "TECH_USE_STATIC_AUDIT_STORAGE";
        public const string TECH_FEATURE_USE_STATIC_CHECKLIST_STORAGE = "TECH_USE_STATIC_CHECKLIST_STORAGE";

        public const string TECH_FEATURE_USE_STATIC_ASSESSMENT_STORAGE = "TECH_USE_STATIC_ASSESSMENT_STORAGE";

    }
}
