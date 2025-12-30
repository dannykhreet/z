using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Settings
{
    public static class RawSettings
    {
        public static string[] RawReferences { get; } = { "tasks", "checklists", "audits", "assessments", "actions", "comments", "shifts", "log_flattener", "log_processor", "log_auditing", "log_logging", "areas", "log_generation", "log_security", "log_sap_pm", "external_relations", "logging_external_requestresponse", "synchronisation_data_warehouse", "log_data_warehouse", "log_data_warehouse_security", "log_app", "log_grouped_app", "actions_max_resolved_totals_per_month", "actions_resolved_totals_per_month", "tasks_deeplink_linked_audit", "tasks_deeplink_linked_checklist", "audits_deeplink_linked_task", "checklists_deeplink_linked_task", "log_usage_app", "log_export", "log_provisioner","systemusers_overview", "serviceusers_overview", "tasktemplate_versions", "checklisttemplate_versions", "audittemplate_versions", "assessmenttemplate_versions", "workinstructiontemplate_versions" };

    }
   
}
