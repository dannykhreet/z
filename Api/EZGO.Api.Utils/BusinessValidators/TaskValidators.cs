using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// TaskValidators; contains all validation methods for validating tasks and values of the tasks.
    /// </summary>
    public static class TaskValidators
    {
        public const string MESSAGE_TASK_ID_IS_NOT_VALID = "TaskId is not valid";
        public const string MESSAGE_VERSION_IS_NOT_VALID = "Version is not valid";
        public const string MESSAGE_TEMPLATE_ID_IS_NOT_VALID = "TemplateId is not valid";
        public const string MESSAGE_STEP_ID_IS_NOT_VALID = "StepId is not valid";
        public const string MESSAGE_RECURRENCY_ID_IS_NOT_VALID = "RecurrencyId is not valid";
        public const string MESSAGE_AREA_IS_NOT_VALID = "Area is not valid";
        public static bool TaskIdIsValid(int taskid)
        {
            if (taskid > 0)
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

        public static bool StepIdIsValid(int stepid)
        {
            if (stepid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool RecurrencyIdIsValid(int recurrencyid)
        {
            if (recurrencyid > 0)
            {
                return true;
            }

            return false;
        }

        public static bool AreaIsValid(string area)
        {
            //Add regex for valid filenames
            if (!string.IsNullOrEmpty(area))
            {
                return true;
            }

            return false;
        }


        public static bool CompanyConnectionIsValid(List<Models.TasksTask> task, int companyId)
        {
            return !(task.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(Models.TasksTask task, int companyId)
        {
            return (task.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this Models.TasksTask task, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (task == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Task is not valid or empty;");
                }

                if (succes && task.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(task: task, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && task.PropertyUserValues != null && task.PropertyUserValues.Count > 0)
                {
                    foreach (var item in task.PropertyUserValues)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && task.Actions != null && task.Actions.Count > 0)
                {
                    foreach (var item in task.Actions)
                    {
                        if (succes)
                        {
                            succes = item.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, ignoreCreatedByCheck: item.Id > 0, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && task.PictureProof != null) // && task.Status != "todo" && task.Status != "skipped")
                {
                    succes = task.PictureProof.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, ignoreCreatedByCheck: task.PictureProof.Id > 0, validUserIds: validUserIds);
                    if (!succes) messageBuilder.Append(possibleMessages);
                }

                if (!string.IsNullOrEmpty(task.Status)) task.Status = TextValidator.StripRogueDataFromText(task.Status);
                if (!string.IsNullOrEmpty(task.Comment)) task.Status = TextValidator.StripRogueDataFromText(task.Comment);
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

        public static bool ValidateAndClean(this Models.TaskStatusWithReason taskStatus, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (taskStatus == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Task status is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(taskStatus.Comment)) taskStatus.Comment = TextValidator.StripRogueDataFromText(taskStatus.Comment);
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

        public static bool ValidateAndClean(this Models.MultiTaskStatusWithReason taskStatus, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (taskStatus == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Task status is not valid or empty;");
                }

                if (!string.IsNullOrEmpty(taskStatus.Comment)) taskStatus.Comment = TextValidator.StripRogueDataFromText(taskStatus.Comment);
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
