using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EZGO.Api.Security.Helpers;
using EZGO.Api.Models.Settings;
using System.Linq;
using EZGO.Api.Security.Interfaces;
using System.Net;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Logic.Managers;

namespace EZGO.Api.Middleware
{
    /// <summary>
    /// FeatureGlobalFilter; Global filter functionality for use with controllers and controller actions
    /// Based on the features that a company has access to the executing of a controller (or action) is continued or halted.
    /// </summary>
    public class FeatureGlobalFilter : IAsyncActionFilter
    {
        private Features _features;
        private IApplicationUser _applicationUser;
        private IGeneralManager _generalManager;
        const string MESSSAGE = "Company has no rights to access {0}";

        public FeatureGlobalFilter(IApplicationUser applicationUser, IGeneralManager generalManager) {
            _applicationUser = applicationUser;
            _generalManager = generalManager;
        }

        /// <summary>
        /// OnActionExecutionAsync; General OnActionExecutionAsync method. Check if user/company has rights to execute the functionality.
        /// </summary>
        /// <param name="context">Current executing context which is being called through a controller.</param>
        /// <param name="next">Delegate to continue</param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        { 
            var featureAttributes = context.ActionDescriptor.FilterDescriptors.Select(x => x.Filter).OfType<FeatureAttribute>();

            var continueWithRequest = false;

            if (featureAttributes.Any() && !featureAttributes.Where(x => x.Feature == FeatureAttribute.FeatureFiltersEnum.Ignore).Any())
            {
                _features = await _generalManager.GetFeatures(companyId: await _applicationUser.GetAndSetCompanyIdAsync(),
                                                              userId: await _applicationUser.GetAndSetUserIdAsync());
                if (_features != null)
                {
                    foreach (var feature in featureAttributes)
                    {
                        switch (feature.Feature)
                        {
                            case FeatureAttribute.FeatureFiltersEnum.Actions:
                                if (_features.ActionsEnabled.HasValue && _features.ActionsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.ActionComments:
                                if (_features.ActionCommentsEnabled.HasValue && _features.ActionCommentsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.ActionOnTheSpot:
                                if (_features.ActionOnTheSportEnabled.HasValue && _features.ActionOnTheSportEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Audits:
                                if (_features.AuditsEnabled.HasValue && _features.AuditsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Checklists:
                                if (_features.ChecklistsEnabled.HasValue && _features.ChecklistsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Comments:
                                if (_features.EasyCommentsEnabled.HasValue && _features.EasyCommentsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Exports:
                                if (_features.ExportEnabled.HasValue && _features.ExportEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.ExportsBasicData:
                                if (_features.ExportEnabled.HasValue && _features.ExportEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.ExportsBasicTemplates:
                                if (_features.ExportEnabled.HasValue && _features.ExportEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.FactoryFeed:
                                if (_features.FactoryFeedEnabled.HasValue && _features.FactoryFeedEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.RequiredProof:
                                if (_features.RequiredProof.HasValue && _features.RequiredProof.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Reports:
                                if (_features.ReportsEnabled.HasValue && _features.ReportsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Roles:
                                if (_features.RoleManagementEnabled.HasValue && _features.RoleManagementEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Tasks:
                                if (_features.TasksEnabled.HasValue && _features.TasksEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.MarketPlace:
                                if (_features.MarketEnabled.HasValue && _features.MarketEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.SkillAssessments:
                                if (_features.SkillAssessments.HasValue && _features.SkillAssessments.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.WorkInstructions:
                                if (_features.WorkInstructions.HasValue && _features.WorkInstructions.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.SkillMatrix:
                                if (_features.SkillMatrix.HasValue && _features.SkillMatrix.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.TaskGenerationManager:
                                if (_features.TaskGenerationOptions.HasValue && _features.TaskGenerationOptions.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS:
                                if (_features.SkillAssessmentsRunningInCms.HasValue && _features.SkillAssessmentsRunningInCms.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Tags:
                                if (_features.TagsEnabled.HasValue && _features.TagsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.UserExtendedDetails:
                                if (_features.UserExtendedDetailsEnabled.HasValue && _features.UserExtendedDetailsEnabled.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.WorkInstructionItemAttachmentPdf:
                                if (_features.WorkInstructionItemAttachmentPdf.HasValue && _features.WorkInstructionItemAttachmentPdf.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.WorkInstructionItemAttachmentLink:
                                if (_features.WorkInstructionItemAttachmentLink.HasValue && _features.WorkInstructionItemAttachmentLink.Value) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.TemplateSharing:
                                if (_features.TemplateSharingEnabled == true) continueWithRequest = true;
                                break;
                            case FeatureAttribute.FeatureFiltersEnum.Ignore:
                                continueWithRequest = true;
                                break;
                        }
                    }
                }
                
            } else
            {
                continueWithRequest = true; //no feature available to check, continue with request
            }

            if (continueWithRequest)
            {
                await next(); // continue the actual action
            }
            else
            {
                context.Result = CreateForbiddenRequest(string.Join(",", featureAttributes.Select(x => x.Feature.ToString())));
            }
        }

        /// <summary>
        /// CreateBadRequest; Create a bad request to return to a IActionResult action if the user has no rights to this functionality.
        /// </summary>
        /// <param name="messagePart"></param>
        /// <returns>ContentResult containing message and status.</returns>
        private ContentResult CreateForbiddenRequest(string messagePart)
        {
            var badResult = new ContentResult();
            badResult.StatusCode = (int)HttpStatusCode.Forbidden;
            badResult.Content = string.Format(MESSSAGE, messagePart);
            return badResult;
        }
    }
}
