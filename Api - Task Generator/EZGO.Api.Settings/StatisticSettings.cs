using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    /// <summary>
    /// StatisticSettings; settings for use with statistics / reporting within the API
    /// </summary>
    public static class StatisticSettings
    {
        /// <summary>
        /// StatisticReferences; statistic reference list, which refers to an SP that may be executed when certain functionality is called.
        /// NOTE! in the near future this should be added to the general configation db so it can be changed on the fly.
        /// </summary>
        public static string[] StatisticReferences { get; } = { "actionscount_pastdue_per_assignedarea", "actionscount_pastdue_per_assigneduser", "actionscount_per_area", "actionscount_per_checklisttemplate", "actionscount_per_audittemplate", "actionscount_per_tasktemplate","actionscount_per_action_state", "actionscount_per_date", "actionscount_per_month", "actionscount_per_week", "actionscount_per_day", "actionscount_per_year","actionscount_per_user","actionscount_per_assigned_user",
                                                                "commentcount_per_action",
                                                                "auditscount_per_audit_state","auditscount_per_date","audititemscount_per_state","auditscount_per_day","auditscount_per_week","auditscount_per_year","auditscount_per_month","audititemscount_per_state_per_date","auditscount_per_user","actionscount_started_resolved_per_date",
                                                                "checklistscount_per_checklist_state","checklistscount_per_date", "checklistitemscount_per_state","checklistscount_per_day","checklistscount_per_week","checklistscount_per_month","checklistscount_per_year","checklistitemscount_per_state_per_date","checklistscount_per_user",
                                                                "taskscount_per_state","taskscount_per_state_per_date","taskscount_per_state_per_date_last_12_months","taskscount_per_state_per_date_last_6_months","taskscount_per_state_per_date_last_month",
                                                                "auditsaverage_per_state", "auditsaverage_per_month","auditsaverage_per_year","auditsaverage_per_week","auditsaverage_per_day","auditsaverage_per_date",
                                                                "my_actionscount_per_date", "my_actionscount_per_month", "my_actionscount_per_week", "my_actionscount_per_day", "my_actionscount_per_year",
                                                                "my_checklistscount_per_date", "my_checklistscount_per_month", "my_checklistscount_per_week", "my_checklistscount_per_day", "my_checklistscount_per_year",
                                                                "my_auditscount_per_date", "my_auditscount_per_month", "my_auditscount_per_week", "my_auditscount_per_day", "my_auditscount_per_year",
                                                                "my_auditsaverage_per_date", "my_auditsaverage_per_month", "my_auditsaverage_per_week", "my_auditsaverage_per_day", "my_auditsaverage_per_year",
                                                                "actionscount_created_per_month_year_by_range","actionscount_duedate_per_month_year_by_range","assessmentscount_per_month_year_by_range","auditscount_per_month_year_by_range","checklistscount_per_month_year_by_range","commentscount_created_per_month_year_by_range","taskscount_per_month_year_by_range","taskscount_ok_per_month_year_by_range","taskscount_notok_per_month_year_by_range","taskscount_skipped_per_month_year_by_range",
                                                                "tasktemplatecount_per_month_year_by_range","assessmenttemplatecount_month_year_by_range","audittemplatecount_per_month_year_by_range","checklisttemplatecount_month_year_by_range","witemplatecount_per_month_year_by_range"
        };

        public static string[] StatisticReferencesDatawarehouse { get; } = { "action", "area", "assessment", "audit", "checklist", "comment", "company", "pictureproof", "shift", "tag", "task" };

    }
}




