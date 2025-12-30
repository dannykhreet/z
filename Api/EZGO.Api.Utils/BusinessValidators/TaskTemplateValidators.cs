using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Utils.Converters;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    /// <summary>
    /// TaskTemplateValidators; contains all validation methods for validating tasktemplate and values of the tasktemplate.
    /// </summary>
    public static class TaskTemplateValidators
    {
        public const string MESSAGE_TASKTEMPLATE_IS_NOT_VALID = "TaskTemplate is not valid. Type, RecurrencyType, Recurrency.RecurrencyType or Recurrency.Schedule.MonthRecurrencyType not correct.";

        public static bool TaskTemplateIsValid(TaskTemplate taskTemplate)
        {
            bool output = true;

            if (taskTemplate.Type != TaskTypeEnum.Checklist.ToString().ToLower() &&
                taskTemplate.Type != TaskTypeEnum.Audit.ToString().ToLower() &&
                taskTemplate.Type != TaskTypeEnum.Task.ToString().ToLower()
                ) output = false;


            if (taskTemplate.RecurrencyType != RecurrencyTypeEnum.Month.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.NoRecurrency.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.Shifts.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.Week.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.PeriodDay.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.PeriodHour.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.PeriodMinute.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.DynamicDay.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.DynamicHour.ToDatabaseString() &&
                taskTemplate.RecurrencyType != RecurrencyTypeEnum.DynamicMinute.ToDatabaseString()) output = false;

            if(taskTemplate.Recurrency != null)
            {
                if (taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.Month.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.NoRecurrency.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.Shifts.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.Week.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.PeriodDay.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.PeriodHour.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.PeriodMinute.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.DynamicDay.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.DynamicHour.ToDatabaseString() &&
                taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.DynamicMinute.ToDatabaseString()) output = false;

                if (taskTemplate.Recurrency.Schedule == null ||
                    (!string.IsNullOrEmpty(taskTemplate.Recurrency.Schedule.MonthRecurrencyType) &&
                        taskTemplate.Recurrency.Schedule.MonthRecurrencyType != MonthRecurrencyTypeEnum.DayOfMonth.ToDatabaseString() &&
                        taskTemplate.Recurrency.Schedule.MonthRecurrencyType != MonthRecurrencyTypeEnum.Weekday.ToDatabaseString())) output = false;
            }

            return output;
        }

        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.TaskTemplate> taskTemplates, int companyId)
        {
            return !(taskTemplates.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.TaskTemplate taskTemplate, int companyId)
        {
            return (taskTemplate.CompanyId == companyId);
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.TaskRecurrency taskRecurrency, int companyId)
        {
            return (taskRecurrency.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this TaskTemplate taskTemplate, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (taskTemplate == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Task is not valid or empty;");
                }

                if (succes && taskTemplate.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(taskTemplate: taskTemplate, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes &&
                    taskTemplate.Type != TaskTypeEnum.Checklist.ToString().ToLower() &&
                    taskTemplate.Type != TaskTypeEnum.Audit.ToString().ToLower() &&
                    taskTemplate.Type != TaskTypeEnum.Task.ToString().ToLower()
                    )
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Task type is not valid or empty;");

                }

                if (succes && taskTemplate.Type == TaskTypeEnum.Task.ToString().ToLower())
                {
                    if (succes &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.Month.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.NoRecurrency.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.Shifts.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.Week.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.PeriodDay.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.PeriodHour.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.PeriodMinute.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.DynamicDay.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.DynamicHour.ToDatabaseString() &&
                       taskTemplate.RecurrencyType != RecurrencyTypeEnum.DynamicMinute.ToDatabaseString())
                    {
                        succes = false;
                        if (!succes) messageBuilder.Append("Task recurrency type is not valid or empty;");

                    }

                    if (succes &&
                        taskTemplate.Recurrency != null)
                    {
                        if (succes &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.Month.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.NoRecurrency.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.Shifts.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.Week.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.PeriodDay.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.PeriodHour.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.PeriodMinute.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.DynamicDay.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.DynamicHour.ToDatabaseString() &&
                        taskTemplate.Recurrency.RecurrencyType != RecurrencyTypeEnum.DynamicMinute.ToDatabaseString())
                        {
                            succes = false;
                            if (!succes) messageBuilder.Append("Task recurrency type is not valid or empty;");
                        }


                        if (taskTemplate.Recurrency.Schedule == null)
                        {
                            succes = false;
                            if (!succes) messageBuilder.Append("Task recurrency schedule is not valid or empty;");
                        }
                        else
                        {
                            if (taskTemplate.Recurrency.RecurrencyType == RecurrencyTypeEnum.Week.ToDatabaseString())
                            {
                                if (!taskTemplate.Recurrency.Schedule.Week.HasValue) { taskTemplate.Recurrency.Schedule.Week = 1; }
                                //TODO add more validation and defaults
                            }
                        }

                        if (succes &&
                            !string.IsNullOrEmpty(taskTemplate.Recurrency.Schedule.MonthRecurrencyType) &&
                            taskTemplate.Recurrency.Schedule.MonthRecurrencyType != MonthRecurrencyTypeEnum.DayOfMonth.ToDatabaseString() &&
                            taskTemplate.Recurrency.Schedule.MonthRecurrencyType != MonthRecurrencyTypeEnum.Weekday.ToDatabaseString())
                        {
                            succes = false;
                            if (!succes) messageBuilder.Append("Task month recurrency is not valid or empty;");
                        }
                    }
                }


                if (succes && taskTemplate.Steps != null && taskTemplate.Steps.Count > 0)
                {
                    foreach (Step step in taskTemplate.Steps)
                    {
                        if (succes)
                        {
                            succes = step.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && taskTemplate.Properties != null && taskTemplate.Properties.Count > 0)
                {
                    foreach (PropertyTaskTemplate prop in taskTemplate.Properties)
                    {
                        if (succes)
                        {
                            succes = prop.ValidateAndClean(companyId: companyId, userId: userId, out var possibleMessages, validUserIds: validUserIds);
                            if (!succes) messageBuilder.Append(possibleMessages);
                        }
                    }
                }

                if (succes && !string.IsNullOrEmpty(taskTemplate.Picture))
                {
                    succes = UriValidator.MediaUrlPartIsValid(taskTemplate.Picture);
                    if (!succes) messageBuilder.AppendLine(string.Format("Picture Uri [{0}] is not valid.", taskTemplate.Picture));
                }

                if (succes && !string.IsNullOrEmpty(taskTemplate.VideoThumbnail))
                {
                    succes = UriValidator.MediaUrlPartIsValid(taskTemplate.VideoThumbnail);
                    if (!succes) messageBuilder.AppendLine(string.Format("VideoThumbnail Uri [{0}] is not valid.", taskTemplate.VideoThumbnail));
                }

                if (succes && !string.IsNullOrEmpty(taskTemplate.Video))
                {
                    succes = UriValidator.MediaUrlPartIsValid(taskTemplate.Video);
                    if (!succes) messageBuilder.AppendLine(string.Format("Video Uri [{0}] is not valid.", taskTemplate.Video));
                }

                if (succes && !string.IsNullOrEmpty(taskTemplate.DescriptionFile))
                {
                    succes = UriValidator.MediaUrlPartIsValid(taskTemplate.DescriptionFile);
                    if (!succes) messageBuilder.AppendLine(string.Format("DescriptionFile Uri [{0}] is not valid.", taskTemplate.DescriptionFile));
                }

                taskTemplate.Name = TextValidator.StripRogueDataFromText(taskTemplate.Name);

                if (!string.IsNullOrEmpty(taskTemplate.Description)) taskTemplate.Description = TextValidator.StripRogueDataFromText(taskTemplate.Description);
                if (!string.IsNullOrEmpty(taskTemplate.DeepLinkTo)) taskTemplate.DeepLinkTo = TextValidator.StripRogueDataFromText(taskTemplate.DeepLinkTo);
                if (!string.IsNullOrEmpty(taskTemplate.MachineStatus)) taskTemplate.MachineStatus = TextValidator.StripRogueDataFromText(taskTemplate.MachineStatus);
                if (!string.IsNullOrEmpty(taskTemplate.Role)) taskTemplate.Role = TextValidator.StripRogueDataFromText(taskTemplate.Role);
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

        public static bool ValidateAndClean(this TaskRecurrency taskRecurrency, int companyId, int userId, out string messages, List<int> validUserIds = null)
        {
            var succes = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (taskRecurrency == null)
                {
                    succes = false;
                    if (!succes) messageBuilder.Append("Task recurrency is not valid or empty;");
                }

                if (succes && taskRecurrency.CompanyId > 0)
                {
                    succes = CompanyConnectionIsValid(taskRecurrency: taskRecurrency, companyId: companyId);
                    if (!succes) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (succes && taskRecurrency != null)
                {
                    if (succes &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.Month.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.NoRecurrency.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.Shifts.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.Week.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.PeriodDay.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.PeriodHour.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.PeriodMinute.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.DynamicDay.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.DynamicHour.ToDatabaseString() &&
                    taskRecurrency.RecurrencyType != RecurrencyTypeEnum.DynamicMinute.ToDatabaseString())
                    {
                        succes = false;
                        if (!succes) messageBuilder.Append("Task recurrency type is not valid or empty;");
                    }


                    if (taskRecurrency.Schedule == null)
                    {
                        succes = false;
                        if (!succes) messageBuilder.Append("Task recurrency schedule is not valid or empty;");
                    }
                    else
                    {
                        if (taskRecurrency.RecurrencyType == RecurrencyTypeEnum.Week.ToDatabaseString())
                        {
                            if (!taskRecurrency.Schedule.Week.HasValue) { taskRecurrency.Schedule.Week = 1; }
                            //TODO add more validation and defaults
                        }
                    }

                    if (succes &&
                        !string.IsNullOrEmpty(taskRecurrency.Schedule.MonthRecurrencyType) &&
                        taskRecurrency.Schedule.MonthRecurrencyType != MonthRecurrencyTypeEnum.DayOfMonth.ToDatabaseString() &&
                        taskRecurrency.Schedule.MonthRecurrencyType != MonthRecurrencyTypeEnum.Weekday.ToDatabaseString())
                    {
                        succes = false;
                        if (!succes) messageBuilder.Append("Task month recurrency is not valid or empty;");
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
