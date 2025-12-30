using EZGO.Api.Models;
using EZGO.Api.Models.Settings;
using EZGO.Api.Models.Tags;
using System;
using System.Collections.Generic;
using WebApp.Logic;
using WebApp.Models.Action;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class FilterViewModel
    {
        public enum ApplicationModules
        {
            CHECKLISTS,
            COMPLETEDCHECKLISTS,
            TASKS,
            COMPLETEDTASKS,
            AUDITS,
            ACTIONS,
            USERS,
            LANGUAGES,
            APPLANGUAGES,
            CMSLANGUAGES,
            DASHBOARD,
            CONFIG,
            REPORT,
            ANNOUNCEMENTS,
            SETTINGS,
            COMPANIES,
            TASKINDICES,
            CHECKLISTINDICES,
            AUDITINDICES,
            WORKINSTRUCTIONS,
            SKILLASSESSMENTS,
            REPORTSKILLASSESSMENTS,
            SKILLSMATRIX,
            FACTORYFEED,
            MARKETPLACE,
            RAWVIEWER,
            RAWSCHEDULER,
            DATAWAREHOUSE,
            AUDITING,
            TAGS,
            EXTERNALLINK,
            VERSIONS,
            COMPLETEDAUDITS,
            CompanySetting,
            TOOLS,
            WICHANGENOTIFICATIONS
        }

        public Dictionary<string, string> CmsLanguage { get; set; }
        public ApplicationSettings ApplicationSettings { get; set; }
        public WebApp.Models.User.UserProfile CurrentUser { get; set; }
        public List<EZGO.Api.Models.Area> Areas { get; set; }
        public ApplicationModules Module { get; set; }
        public List<WebApp.Models.User.UserProfile> Users { get; set; }
        public List<EZGO.Api.Models.UserProfile> ActionUsers { get; set; }
        public List<EZGO.Api.Models.Area> ActionAreas { get; set; }
        public List<Models.Skills.SkillAssessmentTemplate> Assessments { get; set; }
        public List<TagGroup> TagGroups { get; set; }
        public List<TemplateSummary> Templates { get; set; }
        public int TemplateId { get; set; }
        public FilterViewModel()
        {
            Areas = new List<EZGO.Api.Models.Area>();
        }
    }
}
