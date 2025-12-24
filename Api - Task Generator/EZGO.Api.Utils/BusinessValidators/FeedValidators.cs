using EZGO.Api.Models.Feed;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class FeedValidators
    {
        public static bool CompanyConnectionIsValid(List<FactoryFeed> factoryFeed, int companyId)
        {
            return !(factoryFeed.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(FactoryFeed factoryFeed, int companyId)
        {
            return (factoryFeed.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(List<FeedMessageItem> factoryMessageItem, int companyId)
        {
            return !(factoryMessageItem.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(FeedMessageItem factoryMessageItem, int companyId)
        {
            return (factoryMessageItem.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Feed.FactoryFeed factoryFeed, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (factoryFeed == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("FactoryFeed is not valid or empty;");
                }
                if (succes && factoryFeed.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(factoryFeed: factoryFeed, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && factoryFeed.Attachments != null && factoryFeed.Attachments.Count > 0)
                {
                    foreach (string item in factoryFeed.Attachments)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item));
                        }
                    }
                }
                factoryFeed.Name = TextValidator.StripRogueDataFromText(factoryFeed.Name);
                if (!string.IsNullOrEmpty(factoryFeed.Description)) factoryFeed.Description = TextValidator.StripRogueDataFromText(factoryFeed.Description);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Feed.FeedMessageItem factoryMessageItem, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (factoryMessageItem == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("FactoryMessageItem is not valid or empty;");
                }
                if (succes && factoryMessageItem.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(factoryMessageItem: factoryMessageItem, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                if (succes && factoryMessageItem.Attachments != null && factoryMessageItem.Attachments.Count > 0)
                {
                    foreach (string item in factoryMessageItem.Attachments)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item);
                            if (!succes) messageBuilder.AppendLine(string.Format("Attachment Uri [{0}] is not valid.", item));
                        }
                    }
                }
                if (succes && factoryMessageItem.Media != null && factoryMessageItem.Media.Count > 0)
                {
                    foreach (var item in factoryMessageItem.Media)
                    {
                        if (succes)
                        {
                            succes = UriValidator.MediaUrlPartIsValid(item.Uri);
                            if (!string.IsNullOrEmpty(item.VideoThumbnailUri))
                            {
                                succes = UriValidator.MediaUrlPartIsValid(item.VideoThumbnailUri);
                                if (!succes) messageBuilder.AppendLine(string.Format("Media Video Thumbnail Uri [{0}] is not valid.", item.VideoThumbnailUri));
                            }
                            if (!succes) messageBuilder.AppendLine(string.Format("Media Uri [{0}] is not valid.", item.Uri));
                        }
                    }
                }

                if (succes && factoryMessageItem.UserId > 0 && !ignoreCreatedByCheck)
                {
                    succes = factoryMessageItem.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(factoryMessageItem.UserId.Value);
                    if (!succes) messageBuilder.AppendLine("User by identifier is not valid.");
                }

                factoryMessageItem.Title = TextValidator.StripRogueDataFromText(factoryMessageItem.Title);
                if (!string.IsNullOrEmpty(factoryMessageItem.Description)) factoryMessageItem.Description = TextValidator.StripRogueDataFromText(factoryMessageItem.Description);

                if (succes && factoryMessageItem.Title.Length > 250)
                {
                    succes = factoryMessageItem.Title.Length <= 250;
                    if (!succes) messageBuilder.AppendLine("Title can not be longer than 250 characters.");
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
