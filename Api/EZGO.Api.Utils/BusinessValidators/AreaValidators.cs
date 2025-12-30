using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// AreaValidators; contains all validation methods for validating areas and values of the areas. Area's are part of a company and are directly related to templates and tasks.
    /// </summary>
    public static class AreaValidators
    {
        public const string MESSAGE_AREA_ID_IS_NOT_VALID = "AreaId is not valid";

        public static bool AreaIdIsValid(int areaid)
        {
            if (areaid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.Area> areas, int companyId)
        {
            return !(areas.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Area area, int companyId)
        {
            return (area.CompanyId == companyId);
        }

        /// <summary>
        /// ValidateAndClean; Cleans and validated area.
        /// The following rules must be met:
        /// = The company that is connected to the action must be the company that executes the check.
        /// - The Name can not contain script related tags and filters out html tags
        /// - The Description can not contain script related tags and filters out html tags
        /// - The Picture should contain valid media UriParts
        /// </summary>
        /// <param name="area"></param>
        /// <param name="companyId"></param>
        /// <param name="userId"></param>
        /// <param name="messages"></param>
        /// <param name="ignoreCreatedByCheck"></param>
        /// <returns></returns>
        public static bool ValidateAndClean(this EZGO.Api.Models.Area area, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (area == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Area is not valid or empty;");
                }
                if (succes && area.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(area: area, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && !string.IsNullOrEmpty(area.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(area.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", area.Picture));

                }

                area.Name = TextValidator.StripRogueDataFromText(area.Name);
                if (!string.IsNullOrEmpty(area.Description)) area.Description = TextValidator.StripRogueDataFromText(area.Description);

                if (area.SystemInformation != null)
                {
                    area.SystemInformation.SystemRole = TextValidator.StripRogueDataFromText(area.SystemInformation.SystemRole);
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

        //TODO add validation of Area object (e.g. what is are the rules)

    }
}
