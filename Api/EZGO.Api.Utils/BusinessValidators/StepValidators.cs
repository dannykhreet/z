using EZGO.Api.Models;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class StepValidators
    {
        public static bool ValidateAndClean(this Step step, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (step == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Step is not valid or empty;");
                }
                if (succes && !string.IsNullOrEmpty(step.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(step.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", step.Picture));
                }

                if (succes && !string.IsNullOrEmpty(step.VideoThumbnail))
                {
                    succes = UriValidator.MediaUrlPartIsValid(step.VideoThumbnail);
                    if (!succes) messageBuilder.AppendLine(string.Format("VideoThumbnail Uri [{0}] is not valid.", step.VideoThumbnail));
                }

                if (succes && !string.IsNullOrEmpty(step.Video))
                {
                    succes = UriValidator.MediaUrlPartIsValid(step.Video);
                    if (!succes) messageBuilder.AppendLine(string.Format("Video Uri [{0}] is not valid.", step.Video));
                }

                if (!string.IsNullOrEmpty(step.Description)) step.Description = TextValidator.StripRogueDataFromText(step.Description);

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
