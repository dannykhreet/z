using EZGO.Api.Models.Tags;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class TagValidators
    {
        public static bool ValidateAndClean(this Tag tag, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();
            
            try
            {
                if (tag == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Tag is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(tag.Name)) tag.Name = TextValidator.StripRogueDataFromText(tag.Name);
                if (string.IsNullOrEmpty(tag.Name))
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Tag name is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(tag.IconName)) tag.IconName = TextValidator.StripRogueDataFromText(tag.IconName);
                if (!string.IsNullOrEmpty(tag.IconStyle)) tag.IconStyle = TextValidator.StripRogueDataFromText(tag.IconStyle);

                if (succes)
                {
                    //correct system tag flag if incorrectly provided
                    bool tagIdIsSystemTag = tag.Id <= 1000;
                    if (tag.IsSystemTag == true)
                    {
                        if (!tagIdIsSystemTag)
                        {
                            messageBuilder.Append("Tag with id " + tag.Id + " is not a system tag.");
                            tag.IsSystemTag = false;
                        }
                    }
                    else
                    {
                        if (tagIdIsSystemTag)
                        {
                            messageBuilder.Append("Tag with id " + tag.Id + " is a system tag and should be flagged as such.");
                            tag.IsSystemTag = true;
                        }
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
