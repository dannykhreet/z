using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class PictureProofValidators
    {
        public static bool ValidateAndClean(this Models.PictureProof pictureProof, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (pictureProof == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("PictureProof is not valid or empty;");
                }

                // NO COMPANY AVAILABLE? individual checks not possible
                if (succes && pictureProof.ProofTakenByUserId > 0 && !ignoreCreatedByCheck)
                {
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(pictureProof.ProofTakenByUserId);
                    if (!succes) messageBuilder.AppendLine("User taken by identifier is not valid.");
                }

                if (succes && pictureProof.Media != null && pictureProof.Media.Count > 0)
                {
                    foreach (var item in pictureProof.Media)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes)
                {
                    succes = pictureProof.ProofTakenUtc > DateTime.MinValue;
                    if (!succes) messageBuilder.AppendLine("Picture Taken DateTime is not valid.");
                }

                if (!string.IsNullOrEmpty(pictureProof.Description)) pictureProof.Description = TextValidator.StripRogueDataFromText(pictureProof.Description);

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

        public static bool ValidateAndClean(this Models.PictureProofMedia pictureProofMedia, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (pictureProofMedia == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("pictureProofMedia is not valid or empty;");
                }

                // NO COMPANY AVAILABLE? individual checks not possible
                if (succes && pictureProofMedia.UserId > 0 && !ignoreCreatedByCheck)
                {
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(pictureProofMedia.UserId);
                    if (!succes) messageBuilder.AppendLine("User taken by identifier is not valid.");
                }

                if (!string.IsNullOrEmpty(pictureProofMedia.ItemName)) pictureProofMedia.ItemName = TextValidator.StripRogueDataFromText(pictureProofMedia.ItemName);
                if (!string.IsNullOrEmpty(pictureProofMedia.UserFullName)) pictureProofMedia.UserFullName = TextValidator.StripRogueDataFromText(pictureProofMedia.UserFullName);

                if (succes)
                {
                    succes = UriValidator.MediaUrlPartIsValid(pictureProofMedia.UriPart);
                    if (!succes) messageBuilder.AppendLine(string.Format("Uri [{0}] is not valid.", pictureProofMedia.UriPart));
                }

                if (succes)
                {
                    succes = UriValidator.MediaUrlPartIsValid(pictureProofMedia.ThumbUriPart);
                    if (!succes) messageBuilder.AppendLine(string.Format("Thumbnail Uri [{0}] is not valid.", pictureProofMedia.ThumbUriPart));
                }

                if (succes)
                {
                    succes = pictureProofMedia.PictureTakenUtc > DateTime.MinValue;
                    if (!succes) messageBuilder.AppendLine("Picture Taken DateTime is not valid.");
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
