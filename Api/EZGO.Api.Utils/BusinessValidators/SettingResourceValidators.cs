using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class SettingResourceValidators
    {
        public static bool ValidateAndClean(this EZGO.Api.Models.Settings.SettingResourceItem setting, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (setting == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Setting is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(setting.Description)) setting.Description = TextValidator.StripRogueDataFromText(setting.Description);
                if (!string.IsNullOrEmpty(setting.Value)) setting.Value = TextValidator.StripRogueDataFromText(setting.Value, ignoreMinusChar: true);
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
