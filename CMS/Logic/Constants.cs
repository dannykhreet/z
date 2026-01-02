using System;
using EZGO.CMS.LIB.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;

namespace WebApp.Logic
{
    public static class Constants
    {
        /// <summary>
        /// Class containing all unique urls for ez-go action related api calls.
        /// </summary>
        public static class Action
        {
            public const string GetActionsUrl = @"/v1/actions?include=unviewedcommentnr,mainparent,tags&limit=0";
            public const string GetActionsUserSpecificCreatedByUrl = @"/v1/actions?createdbyid={0}&limit=100";
            public const string GetActionsUserSpecificAssignedToUrl = @"/v1/actions?assigneduserid={0}&limit=100";
            public const string GetLastActionsUrl = @"/v1/actions?limit={0}";//&timestamp={1}";
            public const string GetTaskActionsUrl = @"/v1/actions?taskid={0}&include=unviewedcommentnr,mainparent,tags&limit=0";
            public const string GetActionDetails = @"/v1/action/{0}?include=unviewedcommentnr,comments,assignedusers,assignedareas,mainparent,tags";
            public const string GetActionComments = @"/v1/actioncomments?actionid={0}";
            public const string PostActionComment = @"/v1/actioncomment/add";
            public const string GetAllComments = @"/v1/actioncomments";
            public const string GetAssignedUsers = @"/v1/actions/assignedusers";
            public const string GetAssignedAreas = @"/v1/actions/assignedareas";
            public const string SetActionResolved = @"/v1/action/setresolved/{0}";
            public const string SetActionCommentsViewed = @"/v1/actioncomment/setviewedall/{0}";
            public const string PostNewAction = @"/v1/action/add";
            public const string PostChangeAction = @"/v1/action/change/{0}";
            public const string UploadPictureUrl = @"/v1/media/image/upload/{0}/{1}";
            public const string UploadVideoUrl = @"/v1/media/video/upload/{0}/0?includebaseurlonreturn=true";

            public static string VideoBaseUrl = Startup.EzgoConfig.GetSection("AppSettings").GetValue<String>("VideoBaseUrl");

            public static string GetMediaUrl(string mediaSuffix)
            {
                if (!mediaSuffix.StartsWith("blob:"))
                    return string.Format("{0}{1}", General.MediaUrl, mediaSuffix);
                else return mediaSuffix;
            }

            public const string ApiDateTimeFormat = "dd-MM-yyyy HH:mm:ss";
        }

        public static class Announcements
        {
            public const string GetAnnouncements = @"/v1/announcements";
        }

        public static class FactoryFeed
        {
            public const string GetFactoryFeed = @"/v1/feeds?include=feeditems&usetreeview=true&limit={1}&offset={2}";
            public const string GetFactoryFeedItems = @"/v1/feeds/items/{0}?limit={1}&offset={2}";
            public const string GetFactoryFeedItemComments = @"/v1/feeds/itemcomments/{0}/{1}?limit={2}&offset={3}";
            public const string AddFeedMessage = @"/v1/feeds/item/add";
            public const string RemoveFeedMessage = @"/v1/feeds/item/setactive/{0}";
            public const string ChangeFeedMessage = @"/v1/feeds/item/change/{0}";
            public const string SetItemLiked = @"/v1/feeds/item/setliked/{0}";

            public const string UploadPictureUrl = @"/v1/media/image/upload/19/0";
            public const string UploadPictureUrlItem = @"/v1/media/image/upload/20/0";

            public const string UploadVideoUrlItem = @"/v1/media/video/upload/20/0";

            public const string UploadDocsUrlItem = @"/v1/media/docs/upload/20/0";

            public const string GetMyStatistics = @"/v1/reporting/statistics/my";

            public const string GetMyEZFeedStatistics = @"/v1/reporting/statistics/my/ezfeed";
            public const string AddFactoryFeed = @"/v1/feeds/add";
            public const string UpdateCheck = @"/v1/updatecheckfeed?fromdateutc={0}";
        }

        /// <summary>
        /// Class containing all unique urls for ez-go audit related api calls.
        ///
        /// </summary>
        public static class Audit
        {
            public const string GetAuditTemplatesUrl = @"/v1/audittemplates?include=tasktemplates,steps,areapaths,tags,areapathids&limit=0";
            public const string GetAuditTemplatesDetailUrl = @"/v1/audittemplate/{0}?include=tasktemplates,actions,steps,propertyvalues,tags,properties,openfields,instructionrelations&limit=0";
            public const string GetAuditTemplatesMoreDetailUrl = @"/v1/audittemplate/{0}?include=areapaths,areapathids,tasktemplates,actions,steps,propertyvalues,properties,openfields,tags&limit=0";

            //Used for deeplinking in tasks
            public const string GetAuditTemplatesSimple = @"/v1/audittemplates?include=areapaths&limit=0";

            public const string GetAuditTemplatesByAreaId = @"/v1/audittemplates?areaid={0}&filterareatype=1&include=tasktemplates,tags,steps&limit=0";

            public const string PostNewAudit = @"/v1/audittemplate/add?fulloutput=true";

            public const string PostChangeAudit = @"/v1/audittemplate/change/{0}?fulloutput=true";

            public const string PostDeleteAudit = @"/v1/audittemplate/setactive/{0}";

            public const string ShareAuditTemplate = @"/v1/audittemplate/share/{0}";

            /// <summary>
            /// Checklists = 4,
            /// ChecklistItems = 5,
            /// ChecklistSteps = 6,
            /// </summary>
            public const string UploadPictureUrl = @"/v1/media/image/upload/{0}/0";

            public const string UploadVideoUrl = @"/v1/media/video/upload/{0}/0?includebaseurlonreturn=true";

            public const string UploadDocsUrl = @"/v1/media/docs/upload/{0}/0?";

            public const string GetCompletedAudits = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields,pictureproof&limit={0}";

            public const string GetCompletedAuditsWithTemplateId = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields,pictureproof&limit={0}&templateId={1}";

            public const string GetCompletedAuditsWithTemplateIdAndOffset = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields,pictureproof&limit={0}&templateId={1}&offset={2}";

            public const string GetCompletedAuditsWithOffset = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&limit={0}&offset={1}";

            public const string GetCompletedAuditsWithOffsetAndArea = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&areaid={0}&limit={1}&offset={2}";

            public const string GetCompletedAuditsWithOffsetAndAreaAndDate = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&areaid={0}&limit={1}&offset={2}&starttimestamp={3}&endtimestamp={4}";

            public const string GetCompletedAuditsWithOffsetAndDate = @"/v1/audits?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&limit={0}&offset={1}&starttimestamp={2}&endtimestamp={3}";

            public const string GetCompletedAuditTasks = @"/v1/audit/{0}?include=tasks,tags,steps,properties,propertyvalues,propertyuservalues,openfields,pictureproof";

            public const string GetConnectedTaskTemplateIds = @"/v1/audittemplate/connections/tasktemplates/{0}";
        }

        public static class AppSettings
        {
            public const string ApplicationSettingsUri = @"/v1/app/settings";
        }

        public static class Authentication
        {
            public const string CheckAuthentication = @"/v1/authentication/check";
        }

        public static class Bookmark
        {
            public const string CreateOrRetrieve = @"/v1/bookmark/createorretrieve/{0}/{1}/{2}";
        }

        /// <summary>
        /// Class containing all unique urls for ez-go checklist related api calls.
        /// </summary>
        public static class Checklist
        {
            public const string GetChecklistTemplates = @"/v1/checklisttemplates?include=tasktemplates,steps,tags,areapaths,areapathids,propertyvalues,property&limit=0";

            public const string GetChecklistTemplatesSimple = @"/v1/checklisttemplates?include=areapaths,tags&limit=0";

            /// <summary>
            /// Use this url in a string.format with as first param the (int) id of the requested checklist template.
            /// </summary>
            public const string GetChecklistTemplateDetails = @"/v1/checklisttemplate/{0}?include=tasktemplates,areapaths,actions,steps,propertyvalues,properties,openfields,instructionrelations,tags";

            /// <summary>
            /// Get checklist template overview by area filtering
            /// </summary>
            public const string GetChecklistTemplatesByAreaId = @"/v1/checklisttemplates?areaid={0}&include=tasktemplates,tags,steps,areapaths&limit=0&filterareatype=1";

            public const string PostNewChecklist = @"/v1/checklisttemplate/add?fulloutput=true";

            public const string PostChangeChecklist = @"/v1/checklisttemplate/change/{0}?fulloutput=true";

            public const string PostDeleteChecklist = @"/v1/checklisttemplate/setactive/{0}";

            public const string PostDeleteCompletedChecklist = @"/v1/checklist/setactive/{0}";

            public const string PostDeleteSharedChecklist = @"/v1/inbox/reject/{0}";

            public const string UploadPictureUrl = @"/v1/media/image/upload/{0}/0";

            public const string UploadVideoUrl = @"/v1/media/video/upload/{0}/0?includebaseurlonreturn=true";

            public const string UploadDocsUrl = @"/v1/media/docs/upload/{0}/0?";

            public const string GetCompletedChecklistsTopList = @"/v1/checklists?iscompleted=true&include=tasks,tags,areapaths,properties,propertyvalues,propertyuservalues,openfields,pictureproof&limit={0}";

            public const string GetCompletedChecklists = @"/v1/checklists?iscompleted=true&include=areapaths,tags,areapathids&limit={0}";

            public const string GetCompletedChecklistsWithTemplateId = @"/v1/checklists?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields,userinformation&limit={0}&templateId={1}";

            public const string GetCompletedChecklistsWithTemplateIdIncludeTasks = @"/v1/checklists?iscompleted=true&include=tasks,areapaths,tags,areapathids&limit={0}&templateId={1}";

            public const string GetCompletedChecklistsWithTemplateIdAndOffset = @"/v1/checklists?iscompleted=true&include=tasks,areapaths,tags,areapathids&limit={0}&templateId={1}&offset={2}";

            public const string GetCompletedChecklistsWithOffset = @"/v1/checklists?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&limit={0}&offset={1}";

            public const string GetCompletedChecklistsWithOffsetAndArea = @"/v1/checklists?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&areaid={0}&limit={1}&offset={2}";

            public const string GetCompletedChecklistsWithOffsetAndAreaAndDate = @"/v1/checklists?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&areaid={0}&limit={1}&offset={2}&starttimestamp={3}&endtimestamp={4}";

            public const string GetCompletedChecklistsWithOffsetAndDate = @"/v1/checklists?iscompleted=true&include=tasks,tags,areapaths,areapathids,properties,propertyvalues,propertyuservalues,openfields&limit={0}&offset={1}&starttimestamp={2}&endtimestamp={3}";

            public const string GetCompletedChecklistTask = @"/v1/checklist/{0}?include=tasks,tags,steps,properties,propertyvalues,propertyuservalues,openfields,pictureproof,userinformation";

            public const string ShareChecklistTemplate = @"/v1/checklisttemplate/share/{0}";

            public const string GetConnectedTaskTemplateIds = @"/v1/checklisttemplate/connections/tasktemplates/{0}";
        }

        public static class CmsLanguage
        {
            public const string GetLanguageKeys = @"v1/app/resources/language/?language={0}&resourcetype=2";
            public const string CreateLanguageKey = @"v1/tools/resources/language/{0}/create";
            public const string UpdateLanguageKeyValue = @"v1/tools/resources/language/{0}/change/{1}";
            public const string UpdateLanguageKeyDescription = @"v1/tools/resources/language/{0}/set/description";
        }

        /// <summary>
        /// Class containing all unique urls for ez-go comment related api calls.
        /// </summary>
        public static class Comment
        {
            public const string GetCommentsUrl = @"/v1/comments?limit=0&include=tags";
            public const string GetTaskCommentsUrl = @"/v1/comments?taskid={0}&include=tags";
            public const string SetTaskCommentActiveState = @"/v1/comment/setactive/{0}";
            public const string GetCommentUrl = @"/v1/comment/{0}?include=tags";
        }

        public static class Company
        {
            public const string GetCompany = @"/v1/company";
            public const string GetCompanyWithShifts = @"/v1/company?include=shifts";
            public const string SetCompanySetting = @"/v1/company/{0}/settings/set";
        }

        public static class General
        {
            public const string ImageUnavailableUrl = @"/assets/img/normal_unavailable_image.png";
            public const string UserImagePlaceHolder = @"/images/user-placeholder.jpg";
            public const string AreaFlatList = @"/v1/areas?usetreeview=false&maxlevel=10&include=sappmfunctionallocations";
            public const string AreaList = @"v1/areas?usetreeview=true&maxlevel=10&include=sappmfunctionallocations";
            public const string PropertyList = @"/v1/properties?include=propertyvalues&propertygroupids=1,6";

            public const string GetMediaTokenUrl = @"/v1/authentication/fetchmediatoken";

            public static string MediaUrl = string.Format("{0}media/", Startup.EzgoConfig.GetSection("AppSettings").GetValue<String>("ApiUri"));

            public static string GetMediaUrl(string mediaSuffix)
            {
                return string.Format("{0}{1}", MediaUrl, mediaSuffix);
            }

            public const string LANGUAGE_COOKIE_STORAGE_KEY = "language";

            public const int NumberOfLastCompletedOnDetailsPage = 5;
            public const int NumberOfLastCompletedOnDashboard = 5;
        }

        public static class Holding
        {
            public const string CompanyBasics = @"/v1/company/holding/companies";
            public const string CompanyBasicsWithTemplateSharingEnabled = @"/v1/company/holding/features/templatesharing/companies";
        }

        public static class Language
        {
            public const string GetLanguageKeys = @"v1/app/resources/language/?language={0}";
            public const string CreateLanguageKey = @"v1/tools/resources/language/{0}/create";
            public const string UpdateLanguageKeyValue = @"v1/tools/resources/language/{0}/change/{1}";
            public const string UpdateLanguageKeyDescription = @"v1/tools/resources/language/{0}/set/description";
        }

        public static class SharedTemplates
        {
            public const string GetInboxItems = @"/v1/inbox";
            public const string GetInboxCount = @"/v1/inbox/count";
            public const string GetSharedTemplateDetails = @"/v1/inbox/{0}";
        }

        public static class Shift
        {
            public const string GetShifts = @"v1/shifts";
            public const string GetShiftById = @"v1/shifts";
        }

        /// <summary>
        /// Class containing all unique urls for ez-go statistic related api calls.
        /// </summary>
        public static class Statistics
        {
            public const string GetCompanyOverviewTotalsUrl = @"/v1/reporting/statistics/companyoverview";
            public const string GetCompanyLogAuditing = @"/v1/tools/logauditing/company?includedata=true";
        }

        public static class Tags
        {
            public const string GetTags = @"/v1/tags";
            public const string GetTagGroups = @"/v1/taggroups?include=usage";
            public const string GetAllTagGroups = @"/v1/taggroups/all?include=usage";
            public const string UpdateTagGroups = @"/v1/taggroups/change";
            public const string AddTag = @"/v1/tags/add";
            public const string UpdateTag = @"/v1/tags/change/";
            public const string DeleteTag = @"/v1/tag/setactive/";
        }

        /// <summary>
        /// Class containing all unique urls for ez-go task related api calls.
        /// </summary>
        public static class Task
        {
            public const string GetTaskTemplatesUrl = @"/v1/tasktemplates?filterareatype=1&tasktype=0&include=areapaths,areapathids,recurrecy,tags,recurrencyshifts,steps";
            public const string GetTaskTemplateDetailUrl = @"/v1/tasktemplate/{0}?filterareatype=1&tasktype=0&include=areapaths,recurrency,recurrencyshifts,tags,steps,propertyvalues,instructionrelations,pictureproof";
            public const string GetTaskAreas = @"/v1/areas?maxlevel=10&include=sappmfunctionallocations";
            public const string GetTaskAreaById = @"/v1/area/{0}?maxlevel=10";
            public const string GetTaskAreasFlatten = @"/v1/areas?maxlevel=10&usetreeview=false&include=sappmfunctionallocations";
            public const string GetTaskTemplateDownloadUrl = @"https://ezgo.api.ezfactory.nl:444/v1/export/tasktemplates/xlsx/61";


            public const string PostNewTaskTemplate = @"/v1/tasktemplate/add?fulloutput=true";

            public const string DuplicateTaskTemplate = @"/v1/tasktemplate/add?fulloutput=true&generateTemplate={0}";

            public const string PostChangeTaskTemplate = @"/v1/tasktemplate/change/{0}?fulloutput=true";

            public const string PostDeleteTaskTemplate = @"/v1/tasktemplate/setactive/{0}";

            public const string UploadPictureUrl = @"/v1/media/image/upload/{0}/0";

            public const string UploadVideoUrl = @"/v1/media/video/upload/{0}/0?includebaseurlonreturn=true";

            public const string UploadDocsUrl = @"/v1/media/docs/upload/{0}/0?";

            /// <summary>
            /// Get task template overview by area filtering
            /// </summary>
            public const string GetTaskTemplatesByAreaId = @"/v1/tasktemplates?areaid={0}&filterareatype=1&tasktype=0&include=areapaths,tags";

            public const string GetCompletedTasks = @"/v1/taskslatest?limit={0}&include=steps,areapaths,properties,tags,propertyvalues,propertyuservalues,pictureproof";

            public const string GetCompletedTasksByTemplateId = @"/v1/taskslatest?limit={0}&templateId={1}&include=steps,areapaths,properties,tags,propertyvalues,propertyuservalues,pictureproof";

            public const string ShareTaskTemplate = @"/v1/tasktemplate/share/{0}";
        }

        public static class User
        {
            public const string UserPermissionUrl = @"/v1/userprofiles";
            public const string UserProfileUrl = @"/v1/userprofile/{0}?include=company,areas,displayareas";
            public const string ChangePasswordUrl = @"v1/userprofile/change/{0}/password";
            public const string GetUserWithCompany = @"v1/userprofile?include=company";
        }

        public static class Assessments
        {
            public const string GetAssessmentTemplates = @"/v1/assessmenttemplates?include=instructions,instructionitems,tags,areapaths,areapathids&limit=0";

            public const string GetAssessments = @"/v1/assessments?include=instructions,instructionitems,areapaths,tags,areapathids&templateid={0}";

            public const string GetAssessmentsNonFilter = @"/v1/assessments?include=instructions,instructionitems,areapaths,tags,areapathids&iscompleted=true";

            public const string GetAssessmentTemplateForCreation = @"/v1/skillassessmenttemplate/{0}?include=instructions,instructionitems,tags";

            public const string PostNewAssessmentUrl = @"/v1/assessment/add?fulloutput=true";

            public const string PostChangeAssessmentUrl = @"/v1/assessment/change/{0}?fulloutput=true";

            public const string UploadPictureUrl = @"/v1/media/image/upload/25/0";

            public const string DeleteAssessment = @"/v1/skillassessment/delete/{0}";

        }

        public static class Skills
        {
            //[Route("skillassessments")]
            public const string SkillAssessmentsUrl = @"/v1/assessments?include=areapaths,areapathids,tags";

            //[Route("skillassessment/{skillassessmentid}")]
            public const string SkillAssessmentDetailsUrl = @"/v1/assessment/{0}?include=instructions,instructionitems,mutationinformation,areapaths,areapathids,tags";

            //[Route("skillassessmenttemplates")]
            public const string SkillAssessmentTemplatesUrl = @"/v1/assessmenttemplates?limit=0&include=instructions,instructionitems,areapaths,areapathids,tags";

            public const string SkillAssessmentCompletedFilterUrl = @"/v1/assessments?include=instructions,instructionitems,areapaths,tags,areapathids&limit={0}&templateid={1}&iscompleted=true";

            //[Route("skillassessmenttemplate/{skillassessmenttemplateid}")]
            public const string SkillAssessmentTemplateDetailsUrl = @"/v1/skillassessmenttemplate/{0}?include=instructions,instructionitems,tags,mutationinformation,areapaths,areapathids";

            public const string GetCompletedAssessmentsWithTemplateId = @"/v1/assessments?iscompleted=true&include=areapaths,tags,instructions,instructionitems&limit={0}&templateId={1}";
            public const string GetCompletedAssessmentsWithTemplateIdAndOffset = @"/v1/assessments?iscompleted=true&include=areapaths,tags,instructions,instructionitems&limit={0}&templateId={1}&offset={2}";

            public const string GetCompletedAssessments = @"/v1/assessments?iscompleted=true&include=areapaths,tags,areapathids&limit={0}";
            public const string GetCompletedAssessmentsWithOffset = @"/v1/assessments?iscompleted=true&include=areapaths,tags,areapathids&limit={0}&offset={1}";
            public const string GetCompletedAssessmentsWithOffsetAndArea = @"/v1/assessments?iscompleted=true&include=areapaths,tags,areapathids&areaid={0}&limit={1}&offset={2}";
            public const string GetCompletedAssessmentsWithOffsetAndAreaAndDate = @"/v1/assessments?iscompleted=true&include=areapaths,tags,areapathids&areaid={0}&limit={1}&offset={2}&starttimestamp={3}&endtimestamp={4}";
            public const string GetCompletedAssessmentsWithOffsetAndDate = @"/v1/assessments?iscompleted=true&include=areapaths,tags,areapathids&limit={0}&offset={1}&starttimestamp={2}&endtimestamp={3}";

            //[Route("skillsmatrix/add")]
            public const string PostNewAssessment = @"/v1/assessmenttemplate/add?fulloutput=true&include=instructions,tags,instructionitems,mutationinformation,areapaths,areapathids";

            //[Route("skillsmatrix/change/{skillsmatrixid}")]
            public const string PostChangeAssessment = @"/v1/assessmenttemplate/change/{0}?fulloutput=true&include=instructions,tags,instructionitems,mutationinformation,areapaths,areapathids";

            //[Route("skillsmatrix/delete/{skillsmatrixid}")]
            public const string PostDeleteAssessment = @"/v1/assessmenttemplate/setactive/{0}";
            public const string PostDeleteMatrix = @"/v1/skillsmatrix/setactive/{0}";

            //[Route("skillsmatrices")]
            public const string SkillMatricesUrl = @"/v1/skillsmatrices";

            //[Route("skillsmatrix/{skillsmatrixid}")]
            public const string SkillMatrixDetailsUrl = @"/v1/skillsmatrix/{0}";

            public const string SkillMatrixStatisticsUrl = @"/v1/skillsmatrix/statistics/{0}";
            public const string SkillMatrixTotalsUrl = @"/v1/skillsmatrix/totals/{0}";

            public const string SkillMatrixUserGroupsUrl = @"/v1/skillsmatrix/{0}/groups";

            public const string SkillMatrixUsers = @"/v1/skillsmatrix/{0}/users";

            // Legend Configuration
            public const string SkillMatrixLegendUrl = @"/v1/skillsmatrix/legend/{0}";
            public const string SkillMatrixLegendItemUrl = @"/v1/skillsmatrix/legend/{0}/item";

            public const string UserSkills = @"/v1/userskills";
            public const string UploadPictureUrl = @"/v1/media/image/upload/{0}/0";
        }

        public static class WorkInstructions
        {
            //[Route("workinstructions")]
            public const string WorkInstructionsUrl = @"/v1/workinstructions";

            //[Route("workinstruction/{workinstructionid}")]
            public const string WorkInstructionDetailsUrl = @"/v1/workinstruction/{0}";

            //[Route("workinstructiontemplates")]
            public const string WorkInstructionTemplatesUrl = @"/v1/workinstructiontemplates?offset=0&limit=999999&include=items,areapaths,areapathids,tags";
            public const string WorkInstructionsTemplatesUrl = @"/v1/workinstructiontemplates?offset=0&instructiontype=0&limit=999999&include=items,areapaths,areapathids,tags";
            public const string AssessmentInstructionTemplatesUrl = @"/v1/workinstructiontemplates?offset=0&instructiontype=1&limit=999999&include=items,areapaths,areapathids,tags";

            //[Route("workinstructiontemplate/{workinstructiontemplateid}")]
            public const string WorkInstructionTemplateDetailsUrl = @"/v1/workinstructiontemplate/{0}?include=items,tags";

            public const string WorkInstructionTemplateDeleteUrl = @"/v1/workinstructiontemplate/delete/{0}";

            public const string PostNewWorkInstruction = @"/v1/workinstructiontemplate/add?fulloutput=true";


            public const string PostChangeWorkInstruction = @"/v1/workinstructiontemplate/change/{0}?fulloutput=true";
            public const string PostChangeWorkInstructionExtended = @"/v1/workinstructiontemplate/extended/change/{0}?fulloutput=true";

            public const string ShareWorkInstruction = @"/v1/workinstructiontemplate/share/{0}";

        }

        public static class Search
        {
            public const string SearchTaskTemplatsUrl = @"/v1/search/tasktemplates";
            public const string SearchChecklistTemplatsUrl = @"/v1/search/checklisttemplates";
            public const string SearchAuditTemplatsUrl = @"/v1/search/audittemplates";
            public const string SearchWorkInstructionTemplatsUrl = @"/v1/search/workinstructiontemplates";
        }

        public static class AuditingLog
        {
            public const string AuditingLatestAuditTemplateUrl = @"/v1/logauditing/audittemplate/{0}/latest";
            public const string AuditingLatestChecklistTemplateUrl = @"/v1/logauditing/checklisttemplate/{0}/latest";
            public const string AuditingLatestTaskTemplateUrl = @"/v1/logauditing/tasktemplate/{0}/latest";
            public const string AuditingLatestActionUrl = @"/v1/logauditing/actiontemplate/{0}/latest";
            public const string AuditingLatestCommentUrl = @"/v1/logauditing/commenttemplate/{0}/latest";
            public const string AuditingLatestAssessmentTemplateUrl = @"/v1/logauditing/assessmenttemplate/{0}/latest";
            public const string AuditingLatestWorkInstructionUrl = @"/v1/logauditing/workinstructiontemplate/{0}/latest";
            public const string AuditingLatestUsersUrl = @"/v1/logauditing/users/{0}/latest";
            public const string AuditingLatestMatricesUrl = @"/v1/logauditing/matrices/{0}/latest";

            public const string AuditingAuditTemplateUrl = @"/v1/logauditing/audittemplate/{0}?limit={1}&offset={2}";
            public const string AuditingChecklistTemplateUrl = @"/v1/logauditing/checklisttemplate/{0}?limit={1}&offset={2}";
            public const string AuditingTaskTemplateUrl = @"/v1/logauditing/tasktemplate/{0}?limit={1}&offset={2}";
            public const string AuditingActionUrl = @"/v1/logauditing/action/{0}?limit={1}&offset={2}";
            public const string AuditingCommentUrl = @"/v1/logauditing/comment/{0}?limit={1}&offset={2}";
            public const string AuditingAssessmentTemplateUrl = @"/v1/logauditing/assessmenttemplate/{0}?limit={1}&offset={2}";
            public const string AuditingWorkInstructionUrl = @"/v1/logauditing/workinstructiontemplate/{0}?limit={1}&offset={2}";
            public const string AuditingUsersUrl = @"/v1/logauditing/users/{0}?limit={1}&offset={2}";
            public const string AuditingMatricesUrl = @"/v1/logauditing/matrices/{0}?limit={1}&offset={2}";

            public const string AuditingByUserUrl = @"/v1/logauditing/user/{0}?limit={1}&offset={2}";
            public const string AuditingLogAuditing = @"/v1/logauditing?limit={0}&offset={1}";
            public const string AuditingLogAuditingOverview = @"/v1/logauditing/overview?limit={0}&offset={1}";
            public const string AuditingLogDetails = @"/v1/logauditing/{0}";
        }

    }
}
