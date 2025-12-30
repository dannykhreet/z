using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class AnnouncementValidators
    {
        public static bool ValidateAndClean(this EZGO.Api.Models.General.Announcement announcement, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (announcement == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Announcement is not valid or empty;");
                }

                if (succes && announcement.Media != null && announcement.Media.Count > 0)
                {
                    foreach (string item in announcement.Media)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item));
                        }
                    }
                }

                announcement.Title = TextValidator.StripRogueDataFromText(announcement.Title);
                if (!string.IsNullOrEmpty(announcement.Description)) announcement.Description = TextValidator.StripRogueDataFromText(announcement.Description);

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
