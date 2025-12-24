using System;

namespace EZGO.Api.Settings
{
    /// <summary>
    /// CacheSettings; Contains cache (and cachekeys) settings related to the API caching mechanism.
    /// </summary>
    public static class CacheSettings
    {
        /// <summary>
        /// CacheKeyHammerProtection; cache key used for hammer protection. When using in-memory debugging and or cache debugging use _HAMMER_ to search for the hammer cache
        /// </summary>
        public static string CacheKeyHammerProtection { get { return "_HAMMER_"; } }
        /// <summary>
        /// CacheKeyCompany; cache key used for company information.
        /// </summary>
        public static string CacheKeyCompany { get { return "_COMPANY"; } }
        /// <summary>
        /// CacheKeyActionParent; cache key used for actionparents
        /// </summary>
        public static string CacheKeyActionParent { get { return "_ACTIONPARENT"; } }
        public static string CacheKeyTasksWeekly { get { return "_TASKSWEEKLY"; } }
        public static string CacheKeyTasksFromTo { get { return "_TASKSFROMTO"; } }
        public static string CacheKeyTasksYesterday { get { return "_TASKSYESTERDAY"; } }
        public static string CacheKeyRequestCheck { get { return "_REQUESTCHECK"; } }
        public static string CacheKeyCompanyStatisticCheck { get { return "_COMPANYSTAT"; } }
        public static string CacheKeyCompanyAllStatisticCheck { get { return "_COMPANYSTATALL"; } }
        public static string CacheKeyHoldingStatisticCheck { get { return "_HOLDINGSTAT"; } }
        /// <summary>
        /// CacheTimeDefaultInSeconds; Default cache-time used for caching data information.
        /// </summary>
        public static int CacheTimeDefaultInSeconds { get { return 3600; } }
        /// <summary>
        /// CacheTimeFluentDataInSeconds; Cache time used for data that is more or less fluent but still can benefit from some caching.
        /// </summary>
        public static int CacheTimeFluentDataInSeconds { get { return 60; } }
        /// <summary>
        /// CacheTimeSemiStaticDataInSeconds; Cache time for data that is more is less static and only changed from management tooling.
        /// </summary>
        public static int CacheTimeSemiStaticDataInSeconds { get { return 600; } }
        /// <summary>
        /// CacheTimeDefaultLongInSeconds; (10u)
        /// </summary>
        public static int CacheTimeDefaultLongInSeconds { get { return 14400; } }

        #region - legacy cache keys -
        //cache keys
        public static string CacheKeyLegacyCompanyList { get { return "_LEGACY_COMPANIES"; } }
        public static string CacheKeyLegacyActionCommentsList { get { return "_LEGACY_ACTION_COMMENTS"; } }

        public static string CacheKeyLegacyActionCommentsByActionIdList { get { return "_LEGACY_ACTION_COMMENTS_BY_ACTION_ID"; } }

        public static string CacheKeyLegacyActionList { get { return "_LEGACY_ACTIONS"; } }

        public static string CacheKeyLegacyAreaList { get { return "_LEGACY_AREAS"; } }

        public static string CacheKeyLegacyAuditList { get { return "_LEGACY_AUDITS"; } }

        public static string CacheKeyLegacyAuditTemplateList { get { return "_LEGACY_AUDIT_TEMPLATES"; } }

        public static string CacheKeyLegacyAuditTaskTemplateList { get { return "_LEGACY_AUDIT_TASKTEMPLATES"; } }

        public static string CacheKeyLegacyChecklistList { get { return "_LEGACY_CHECKLISTS"; } }

        public static string CacheKeyLegacyChecklistTemplateList { get { return "_LEGACY_CHECKLIST_TEMPLATES"; } }

        public static string CacheKeyLegacyShiftList { get { return "_LEGACY_SHIFTS"; } }

        public static string CacheKeyLegacyTaskTemplateStepsList { get { return "_LEGACY_TASK_TEMPLATE_STEPS"; } }

        public static string CacheKeyLegacyTasksForCurrentShiftList { get { return "_LEGACY_TASKS_FOR_CURRENT_SHIFT"; } }

        public static string CacheKeyLegacyTaskTemplateList { get { return "_LEGACY_TASK_TEMPLATES"; } }

        public static string CacheKeyLegacyTaskList { get { return "_LEGACY_TASKS"; } }

        public static string CacheKeyLegacyTemplateRecurrencyBuTemplateIdList { get { return "_LEGACY_TEMPLATE_RECURRENCY_BY_TEMPLATE_ID"; } }

        public static string CacheKeyLegacyGetTaskTemplateTagsList { get { return "_LEGACY_TASK_TEMPLATE_TAGS"; } }

        #endregion

    }
}
