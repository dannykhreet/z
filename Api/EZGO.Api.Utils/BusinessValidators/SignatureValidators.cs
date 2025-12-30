using EZGO.Api.Models.Skills;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class SignatureValidators
    {

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Relations.ChecklistRelationSigning relation, int companyId)
        {
            return (relation.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Relations.ChecklistRelationStatus relation, int companyId)
        {
            return (relation.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Relations.AuditRelationSigning relation, int companyId)
        {
            return (relation.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Relations.AuditRelationStatus relation, int companyId)
        {
            return (relation.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Relations.AuditRelationStatusScore relation, int companyId)
        {
            return (relation.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Relations.TaskRelationSigning relation, int companyId)
        {
            return (relation.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Signature signature, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (signature == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Signature is not valid or empty;");
                }

                if (succes && !string.IsNullOrEmpty(signature.SignatureImage))
                {
                    succes = UriValidator.MediaUrlPartIsValid(signature.SignatureImage);
                    if (!succes) messageBuilder.AppendLine(string.Format("SignatureImage Uri [{0}] is not valid.", signature.SignatureImage));
                }

                if (!string.IsNullOrEmpty(signature.SignedBy)) signature.SignedBy = TextValidator.StripRogueDataFromText(signature.SignedBy);
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Relations.ChecklistRelationSigning relation, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (relation == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Relation is not valid or empty;");
                }

                if (succes && relation.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(relation: relation, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && relation.Signatures != null && relation.Signatures.Count > 0)
                {
                    foreach (var item in relation.Signatures)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && relation.Signatures != null && relation.Signatures.Count > 0)
                {
                    succes = (relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Contains(userId));
                    if (!succes && validUserIds != null)
                    {
                        if (relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Any())
                        {
                            //check if id is not part of valid user ids, if not break, if all ids then continue and all items are part of this collection.
                            foreach (var possibleUserId in relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById))
                            {
                                if (!validUserIds.Contains(possibleUserId.Value)) break;
                            }
                            succes = true;
                        }
                    }
                    if (!succes) messageBuilder.AppendLine("User can not update or add this checklist.");
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Relations.ChecklistRelationStatus relation, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (relation == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Relation is not valid or empty;");
                }

                if (succes && relation.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(relation: relation, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && relation.UserId > 0)
                {
                    succes = relation.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(relation.UserId);
                    if (!succes) messageBuilder.AppendLine("User can not update or add this audit.");
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Relations.AuditRelationStatus relation, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (relation == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Relation is not valid or empty;");
                }

                if (succes && relation.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(relation: relation, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && relation.UserId > 0)
                {
                    succes = relation.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(relation.UserId);
                    if (!succes) messageBuilder.AppendLine("User can not update or add this audit.");
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Relations.AuditRelationStatusScore relation, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (relation == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Relation is not valid or empty;");
                }

                if (succes && relation.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(relation: relation, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && relation.UserId > 0)
                {
                    succes = relation.UserId == userId;
                    if (!succes && validUserIds != null) succes = validUserIds.Contains(relation.UserId);
                    if (!succes) messageBuilder.AppendLine("User can not update or add this audit.");
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Relations.AuditRelationSigning relation, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try {
                if (relation == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Relation is not valid or empty;");
                }

                if (succes && relation.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(relation: relation, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && relation.Signatures != null && relation.Signatures.Count > 0)
                {
                    foreach (var item in relation.Signatures)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && relation.Signatures != null && relation.Signatures.Count > 0)
                {
                    succes = (relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Contains(userId));
                    if (!succes && validUserIds != null)
                    {
                        if (relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Any())
                        {
                            //check if id is not part of valid user ids, if not break, if all ids then continue and all items are part of this collection.
                            foreach (var possibleUserId in relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById))
                            {
                                if (!validUserIds.Contains(possibleUserId.Value)) break;
                            }
                            succes = true;
                        }
                    }
                    if (!succes) messageBuilder.AppendLine("User can not update or add this checklist.");
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

        public static bool ValidateAndClean(this EZGO.Api.Models.Relations.TaskRelationSigning relation, int companyId, int userId, out string messages, bool ignoreCreatedByCheck = false, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (relation == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Relation is not valid or empty;");
                }

                if (succes && relation.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(relation: relation, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && relation.Signatures != null && relation.Signatures.Count > 0)
                {
                    foreach (var item in relation.Signatures)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && relation.Signatures != null && relation.Signatures.Count > 0)
                {
                    succes = (relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Contains(userId));
                    if (!succes && validUserIds != null)
                    {
                        if (relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Any())
                        {
                            //check if id is not part of valid user ids, if not break, if all ids then continue and all items are part of this collection.
                            foreach (var possibleUserId in relation.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById))
                            {
                                if (!validUserIds.Contains(possibleUserId.Value)) break;
                            }
                            succes = true;
                        }
                    }
                    if (!succes) messageBuilder.AppendLine("User can not update or add this checklist.");
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
