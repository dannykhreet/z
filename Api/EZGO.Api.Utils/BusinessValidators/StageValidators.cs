using DocumentFormat.OpenXml.Bibliography;
using EZGO.Api.Models;
using EZGO.Api.Utils.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Api.Utils.BusinessValidators
{
    public static class StageValidators
    {
        public static bool CompanyConnectionIsValid(List<EZGO.Api.Models.Stage> stage, int companyId)
        {
            return !(stage.Where(x => x.CompanyId != companyId).Any());
        }

        public static bool CompanyConnectionIsValid(EZGO.Api.Models.Stage stage, int companyId)
        {
            return (stage.CompanyId == companyId);
        }

        public static bool ValidateAndClean(this EZGO.Api.Models.Stage stage, int companyId, List<EZGO.Api.Models.TasksTask> tasks, out string messages, List<TaskTemplate> taskTemplates = null, EZGO.Api.Models.StageTemplate stageTemplate = null, EZGO.Api.Models.Stage existingStage = null)
        {
            var success = true;
            var messageBuilder = new StringBuilder();

            try
            {
                if (stage == null)
                {
                    success = false;
                    if (!success) messageBuilder.Append("Stage is not valid or empty;");
                }

                if (success && stage.CompanyId > 0)
                {
                    success = CompanyConnectionIsValid(stage: stage, companyId: companyId);
                    if (!success) messageBuilder.AppendLine("Company connection is not valid.");
                }

                if (success && stage.TaskIds != null && stage.TaskIds.Count > 0)
                {
                    foreach (var item in stage.TaskIds)
                    {
                        if (success)
                        {
                            success = item > 0;
                        }
                    }
                }

                if (success && stageTemplate != null)
                {
                    var stageTasks = tasks.Where(t => stageTemplate.TaskTemplateIds?.Contains(t.TemplateId) ?? false).ToList();

                    if (stageTemplate != null && taskTemplates != null)
                    {
                        var stageIndex = stageTemplate.Index;
                        var firstTaskTemplateAfterStage = taskTemplates.Where(t => t.Index > stageIndex).FirstOrDefault();

                        if (firstTaskTemplateAfterStage != null)
                        {
                            var firstTaskIndexAfterStage = tasks.IndexOf(tasks.Where(t => t.TemplateId == firstTaskTemplateAfterStage.Id && !stageTasks.Select(t => t.TemplateId).Contains(t.TemplateId)).FirstOrDefault());
                            if (firstTaskIndexAfterStage < tasks.Count && firstTaskIndexAfterStage != -1)
                            {
                                var tasksAfterStage = tasks.Skip(firstTaskIndexAfterStage)
                                                           .Take(tasks.Count - firstTaskIndexAfterStage)
                                                           .Where(t => !stageTasks.Select(t => t.TemplateId).Contains(t.TemplateId))
                                                           .ToList();

                                if (stageTemplate.BlockNextStagesUntilCompletion && (stage.Status == "todo" || tasks.Where(t => stageTemplate.TaskTemplateIds != null && stageTemplate.TaskTemplateIds.Contains(t.TemplateId) && t.Status == "todo").Count() > 0))
                                {
                                    if (tasksAfterStage != null && tasksAfterStage.Where(t => t.Status != "todo").Count() > 0)
                                    {
                                        success = false;
                                        messageBuilder.AppendLine("Marked tasks detected after unmarked locked stage. Tasks are not valid.");
                                    }
                                }
                            }
                        }
                    }
                }

                if (success && existingStage != null && stageTemplate != null && stageTemplate.LockStageAfterCompletion && tasks != null)
                {
                    var stageTasks = tasks.Where(t => stageTemplate.TaskTemplateIds != null && stageTemplate.TaskTemplateIds.Contains(t.TemplateId)).ToList();
                    if (stageTasks != null && existingStage.Status == "done" && (stageTasks.Where(t => t.Status != "todo").Count() > 0 || stage.Status != "todo"))
                    {
                        success = false;
                        messageBuilder.AppendLine("Marked tasks detected in previously completed stage. Tasks are not valid.");
                    }
                }
                
                if(success)
                {
                    if (!string.IsNullOrEmpty(stage.ShiftNotes)) stage.ShiftNotes = TextValidator.StripRogueDataFromText(stage.ShiftNotes);
                }
               
            }
            catch (Exception ex)
            {
                success = false;
                messageBuilder.AppendLine(string.Format("Error occured [{0}].", ex.Message));
            }
            

            messages = messageBuilder.ToString();

            messageBuilder.Clear();
            messageBuilder = null;

            return success;
        }
    }
}
