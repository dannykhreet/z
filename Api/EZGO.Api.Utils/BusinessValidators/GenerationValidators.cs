using EZGO.Api.Models.Feed;
using EZGO.Api.Models.TaskGeneration;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class GenerationValidators
    {
        public static bool CompanyConnectionIsValid(PlanningConfiguration planningConfiguration, int companyId)
        {
            return (planningConfiguration.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this PlanningConfiguration planningConfiguration, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (planningConfiguration == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Planning configuration is not valid or empty;");
                }

                if (succes && planningConfiguration.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(planningConfiguration: planningConfiguration, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && planningConfiguration.UserId > 0 && !ignoreCreatedByCheck)
                {
                    succes = planningConfiguration.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(planningConfiguration.UserId.Value);
                    if (!succes) messageBuilder.AppendLine("User by identifier is not valid.");
                }

                if (succes && planningConfiguration.ConfigurationItems != null && planningConfiguration.ConfigurationItems.Count > 0)
                {
                    foreach (var item in planningConfiguration.ConfigurationItems)
                    {
                        item.Reason = TextValidator.StripRogueDataFromText(item.Reason);
                    }
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