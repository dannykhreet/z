using EZGO.Api.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Attributes;
using WebApp.Logic.Interfaces;

namespace WebApp.Filters
{

    /// <summary>
    /// FeatureGlobalFilter; Global filter functionality for use with controllers and controller actions
    /// Based on the features that a company has access to the executing of a controller (or action) is continued or halted.
    /// </summary>
    public class FeatureGlobalFilter : IAsyncActionFilter
    {
        private readonly IApplicationSettingsHelper _applicationSettingsHelper;
        private ApplicationSettings _applicationSettings;
        const string MESSSAGE = "Company has no rights to access {0}";

        public FeatureGlobalFilter(IApplicationSettingsHelper applicationSettingsHelper)
        {
            _applicationSettingsHelper = applicationSettingsHelper;
        }

        /// <summary>
        /// OnActionExecutionAsync; General OnActionExecutionAsync method. Check if user/company has rights to execute the functionality.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //TODO refactor; Make negative and only set to true when feature active. 
            var featureAttribute = context.ActionDescriptor.FilterDescriptors
              .Select(x => x.Filter).OfType<FeatureAttribute>().FirstOrDefault();

            var continueWithRequest = true;

            if (featureAttribute != null)
            {
                _applicationSettings = await _applicationSettingsHelper.GetApplicationSettings();
                if (_applicationSettings != null)
                {
                    //TODO add other features enum.
                    switch (featureAttribute.Feature)
                    {
                        case FeatureAttribute.FeatureFiltersEnum.Actions:
                            if (_applicationSettings.Features.ActionsEnabled.HasValue && !_applicationSettings.Features.ActionsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.ActionComments:
                            if (_applicationSettings.Features.ActionCommentsEnabled.HasValue && !_applicationSettings.Features.ActionCommentsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.ActionOnTheSpot:
                            if (_applicationSettings.Features.ActionOnTheSportEnabled.HasValue && !_applicationSettings.Features.ActionOnTheSportEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Audits:
                            if (_applicationSettings.Features.AuditsEnabled.HasValue && !_applicationSettings.Features.AuditsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Checklists:
                            if (_applicationSettings.Features.ChecklistsEnabled.HasValue && !_applicationSettings.Features.ChecklistsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Comments:
                            if (_applicationSettings.Features.EasyCommentsEnabled.HasValue && !_applicationSettings.Features.EasyCommentsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Exports:
                            if (_applicationSettings.Features.ExportEnabled.HasValue && !_applicationSettings.Features.ExportEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.ExportsBasicData:
                            //TODO change to other parameter (not filled yet)
                            if (_applicationSettings.Features.ExportEnabled.HasValue && !_applicationSettings.Features.ExportEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.ExportsBasicTemplates:
                            //TODO change to other parameter (not filled yet)
                            if (_applicationSettings.Features.ExportEnabled.HasValue && !_applicationSettings.Features.ExportEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.FactoryFeed:
                            if (_applicationSettings.Features.FactoryFeedEnabled.HasValue && !_applicationSettings.Features.FactoryFeedEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.RequiredProof:
                            if (_applicationSettings.Features.RequiredProof.HasValue && !_applicationSettings.Features.RequiredProof.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Reports:
                            //TODO change to other parameter (not filled yet)
                            if (_applicationSettings.Features.ReportsEnabled.HasValue && !_applicationSettings.Features.ReportsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Tasks:
                            if (_applicationSettings.Features.TasksEnabled.HasValue && !_applicationSettings.Features.TasksEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.MarketPlace:
                            if (_applicationSettings.Features.MarketEnabled.HasValue && !_applicationSettings.Features.MarketEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.SkillAssessments:
                            if (_applicationSettings.Features.SkillAssessments.HasValue && !_applicationSettings.Features.SkillAssessments.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.WorkInstructions:
                            if (_applicationSettings.Features.WorkInstructions.HasValue && !_applicationSettings.Features.WorkInstructions.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.SkillMatrix:
                            if (_applicationSettings.Features.SkillMatrix.HasValue && !_applicationSettings.Features.SkillMatrix.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.TaskGenerationManager:
                            if (_applicationSettings.Features.TaskGenerationOptions.HasValue && !_applicationSettings.Features.TaskGenerationOptions.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.RunningAssessmentsInCMS:
                            if (_applicationSettings.Features.SkillAssessmentsRunningInCms.HasValue && !_applicationSettings.Features.SkillAssessmentsRunningInCms.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.Tags:
                            if (_applicationSettings.Features.TagsEnabled.HasValue && !_applicationSettings.Features.TagsEnabled.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.WorkInstructionItemAttachmentPdf:
                            if (_applicationSettings.Features.WorkInstructionItemAttachmentPdf.HasValue && !_applicationSettings.Features.WorkInstructionItemAttachmentPdf.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.WorkInstructionItemAttachmentLink:
                            if (_applicationSettings.Features.WorkInstructionItemAttachmentLink.HasValue && !_applicationSettings.Features.WorkInstructionItemAttachmentLink.Value) continueWithRequest = false;
                            break;
                        case FeatureAttribute.FeatureFiltersEnum.TemplateSharing:
                            if (_applicationSettings.Features.TemplateSharingEnabled.HasValue && !_applicationSettings.Features.TemplateSharingEnabled.Value) continueWithRequest = false;
                            break;
                    }
                }
            }

            if (continueWithRequest)
            {
                await next(); // continue the actual action
            }
            else
            {
                context.Result = CreateBadRequest(featureAttribute.Feature.ToString());
            }
        }

        /// <summary>
        /// CreateBadRequest; Create a bad request to return to a IActionResult action if the user has no rights to this functionality.
        /// </summary>
        /// <param name="messagePart"></param>
        /// <returns>ContentResult containing message and status.</returns>
        private ContentResult CreateBadRequest(string messagePart)
        {
            var badResult = new ContentResult();
            badResult.StatusCode = 400;
            badResult.Content = string.Format(MESSSAGE, messagePart);
            return badResult;
        }
    }
}
