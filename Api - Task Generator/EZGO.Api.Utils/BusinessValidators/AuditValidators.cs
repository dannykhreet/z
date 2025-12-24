using EZGO.Api.Models;
using EZGO.Api.Models.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// AuditValidators; contains all validation methods for validating audits and values of the audits. Audits are directly related to templates and tasks.
    /// </summary>
    public static class AuditValidators
    {
        public const string MESSAGE_AUDIT_ID_IS_NOT_VALID = "AuditId is not valid";
        public const string MESSAGE_TEMPLATE_ID_IS_NOT_VALID = "TemplateId is not valid";
        public static bool AuditIdIsValid(int auditid)
        {
            if (auditid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool TemplateIdIsValid(int templateid)
        {
            if (templateid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool CompanyConnectionIsValid(List<Models.Audit> audits, int companyId)
        {
            return !(audits.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(Models.Audit audit, int companyId)
        {
            return (audit.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this Models.Audit audit, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (audit == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Audit is not valid or empty;");
                }

                if (succes && audit.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(audit: audit, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }
                else if (audit.CompanyId == 0)
                {
                    succes = false;
                    messageBuilder.AppendLine("Company id is not set.");
                }

                if (succes && audit.PropertyUserValues != null && audit.PropertyUserValues.Count > 0)
                {
                    foreach (var item in audit.PropertyUserValues)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && audit.OpenFieldsPropertyUserValues != null && audit.OpenFieldsPropertyUserValues.Count > 0)
                {
                    foreach (var item in audit.OpenFieldsPropertyUserValues)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && audit.Tasks != null && audit.Tasks.Count > 0)
                {
                    foreach (var item in audit.Tasks)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }

                        if (succes)
                        {
                            succes = item.Status != null;
                            if (!succes) messageBuilder.Append("Status not set for all tasks.");
                        }
                    }
                }

                if (succes && audit.Signatures != null && audit.Signatures.Count > 0)
                {
                    foreach (var item in audit.Signatures)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && audit.Signatures != null && audit.Signatures.Count > 0)
                {
                    succes = (audit.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Contains(userId));
                    if (!succes && validUserIds != null)
                    {
                        if (audit.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById).Any())
                        {
                            //check if id is not part of valid user ids, if not break, if all ids then continue and all items are part of this collection.
                            foreach (var possibleUserId in audit.Signatures.Where(x => x.SignedById.HasValue).Select(x => x.SignedById))
                            {
                                if (!validUserIds.Contains(possibleUserId.Value)) break;
                            }
                            succes = true;
                        }
                    }
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

        public static bool ValidateMutation(this Models.Audit audit, Models.Audit currentAudit, out string messages)
        {
            bool success = true;
            StringBuilder messageBuilder = new();

            if (success && currentAudit.IsCompleted)
            {
                success = false;
                messageBuilder.AppendLine("Completed audit can not be changed.");
            }

            if (success && audit.Id != currentAudit.Id)
            {
                success = false;
                messageBuilder.AppendLine("Id cannot be changed.");
            }

            if (success && audit.CompanyId != currentAudit.CompanyId)
            {
                success = false;
                messageBuilder.AppendLine("Company id cannot be changed.");
            }

            if (success && audit.TemplateId != currentAudit.TemplateId)
            {
                success = false;
                messageBuilder.AppendLine("Template id cannot be changed.");
            }
            if (success && currentAudit.LinkedTaskId.HasValue && currentAudit.LinkedTaskId.Value > 0 && audit.LinkedTaskId.HasValue && audit.LinkedTaskId.Value > 0 && audit.LinkedTaskId.Value != currentAudit.LinkedTaskId.Value)
            {
                success = false;
                messageBuilder.AppendLine("Linked task id cannot be changed.");
            }

            if (success && !string.IsNullOrEmpty(audit.Version) && !string.IsNullOrEmpty(currentAudit.Version) && audit.Version != currentAudit.Version)
            {
                success = false;
                messageBuilder.AppendLine("Version can not be changed.");
            }

            messages = messageBuilder.ToString();
            return success;
        }
    }
}
