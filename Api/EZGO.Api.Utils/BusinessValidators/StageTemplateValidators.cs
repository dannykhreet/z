using DocumentFormat.OpenXml.Bibliography;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class StageTemplateValidators
    {
        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.StageTemplate> stage, int companyId)
        {
            return !(stage.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.StageTemplate stage, int companyId)
        {
            return (stage.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.StageTemplate stage, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var success = true;
            var messageBuilder = new StringBuilder();

            try {
                if (stage == null)
                {
                    success = false;
                    if (!success) messageBuilder.Append("Stage template is not valid or empty;");
                }

                if (success && stage.CompanyId > 0)
                {
                    success = CompanyConnectionIsValid(stage: stage, companyId: companyId);
                    if (!success) messageBuilder.AppendLine("Company connection is not valid.");
                }
                stage.Name = TextValidator.StripRogueDataFromText(stage.Name);
                if (!string.IsNullOrEmpty(stage.Description)) stage.Description = TextValidator.StripRogueDataFromText(stage.Description);
            }
            catch (Exception ex)
            {
                success = false;
                messageBuilder.AppendLine(string.Format("Error occured [{0}].", ex.Message));
            }

            messages = messageBuilder.ToString();

            messageBuilder.Clear();
            messageBuilder = null;

            return success;
        }
    }
}
