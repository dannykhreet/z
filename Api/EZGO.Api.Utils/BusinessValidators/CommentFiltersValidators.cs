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
    public static class CommentFiltersValidators
    {
        public static bool ValidateAndClean(this CommentFilters commentFilters, out string messages)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                commentFilters.FilterText = TextValidator.StripRogueDataFromText(commentFilters.FilterText);

                if (succes && commentFilters.CreatedFrom != null && commentFilters.CreatedTo != null && commentFilters.CreatedFrom > commentFilters.CreatedTo)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("CreatedFrom is not before or equal to CreatedTo");
                }
            }
            catch (Exception ex)
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
