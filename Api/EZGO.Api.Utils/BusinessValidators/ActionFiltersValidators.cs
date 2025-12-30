using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// ActionValidators; contains all validation methods for validating properties and values of the actions (are part of tasks and related objects).
    /// </summary>
    public static class ActionFiltersValidators
    {
        /// <summary>
        /// ValidateAndCleanAction; Cleans and validated action.
        /// The following rules must be met:
        /// = The company that is connected to the action must be the company that executes the check.
        /// - The userId that created the item is the same user that executes the check (unless ignoreUserIdCheck is true).
        /// - The Comment can not contain script related tags and filters out html tags
        /// - The Description can not contain script related tags and filters out html tags
        /// - The Images should contain valid media UriParts
        /// - The Videos should contain valid media UriParts
        /// - The VideoThumbnails should contain valid media UriParts
        /// </summary>
        /// <param name="action">action object containing all data</param>
        /// <param name="companyId">company of user that is executing the validation</param>
        /// <param name="userId">userId of the user that is executing the validation</param>
        /// <param name="ignoreCreatedByCheck">ignore the created by user check (for changed items)</param>
        /// <returns>true/false</returns>
        public static bool ValidateAndClean(this ActionFilters actionFilters, out string messages)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try { 
                actionFilters.FilterText = TextValidator.StripRogueDataFromText(actionFilters.FilterText);

                if (succes && actionFilters.CreatedFrom != null && actionFilters.CreatedTo != null && actionFilters.CreatedFrom > actionFilters.CreatedTo)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("CreatedFrom is not before or equal to CreatedTo");
                }

                if (succes && actionFilters.OverdueFrom != null && actionFilters.OverdueTo != null && actionFilters.OverdueFrom > actionFilters.OverdueTo)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("OverdueFrom is not before or equal to OverdueTo");
                }

                if (succes && actionFilters.ResolvedFrom != null && actionFilters.ResolvedTo != null && actionFilters.ResolvedFrom > actionFilters.ResolvedTo)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("ResolvedFrom is not before or equal to ResolvedTo");
                }

            } catch(Exception ex)
            {
                succes = false;
                messageBuilder.AppendLine(string.Format("Error occured [{0}].", ex.Message));
            }
            messages = messageBuilder.ToString();

            messageBuilder.Clear();
            messageBuilder = null;

            return succes;
        }
    }
}
