using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Skills;
using EZGO.Api.Models.WorkInstructions;
using System.Linq;

namespace EZGO.Api.Utils.Mappers
{
    public static class TemplateVersionMapper
    {
        public static TasksTask ApplyTemplateVersion(this TasksTask task, TaskTemplate versionedTemplate, string include = null)
        {
            if (versionedTemplate != null)
            {
                task.AreaId = versionedTemplate.AreaId;
                task.Description = versionedTemplate.Description;
                task.DescriptionFile = versionedTemplate.DescriptionFile;
                task.RecurrencyType = versionedTemplate.RecurrencyType;
                task.Recurrency = versionedTemplate.Recurrency;
                task.AreaPath = versionedTemplate.AreaPath;
                task.AreaPathIds = versionedTemplate.AreaPathIds;
                task.Index = versionedTemplate.Index;
                task.Name = versionedTemplate.Name;
                task.Picture = versionedTemplate.Picture;
                task.TaskType = versionedTemplate.Type;
                task.Video = versionedTemplate.Video;
                task.VideoThumbnail = versionedTemplate.VideoThumbnail;
                task.Attachments = versionedTemplate.Attachments;
                task.Template = versionedTemplate;
                task.TaskSteps = versionedTemplate.Steps;
                task.MachineStatus = versionedTemplate.MachineStatus;
                task.PlannedTime = versionedTemplate.PlannedTime;

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))
                {
                    task.Steps = versionedTemplate.Steps;
                    task.TaskSteps = versionedTemplate.Steps;

                    if (task.Steps != null && task.Steps.Count > 0)
                    {
                        task.Steps = task.Steps.OrderBy(s => s.Index).ToList();
                    }

                    if (task.TaskSteps != null && task.TaskSteps.Count > 0)
                    {
                        task.TaskSteps = task.TaskSteps.OrderBy(s => s.Index).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower())))
                {
                    task.Properties = versionedTemplate.Properties;

                    if (task.Properties != null && task.Properties.Count > 0)
                    {
                        task.Properties = task.Properties.OrderBy(s => s.Index).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower())))
                {
                    task.WorkInstructionRelations = versionedTemplate.WorkInstructionRelations;
                    task.WorkInstructions = versionedTemplate.WorkInstructions;

                    if (task.WorkInstructionRelations != null && task.WorkInstructionRelations.Count > 0)
                    {
                        task.WorkInstructionRelations = task.WorkInstructionRelations.OrderBy(w => w.Index).ToList();
                    }

                    if (task.WorkInstructions != null && task.WorkInstructions.Count > 0)
                    {
                        task.WorkInstructions = task.WorkInstructions.OrderBy(w => w.Id).ToList(); //work instructions don't have an index, add default sort by id
                    }
                }

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PictureProof.ToString().ToLower()))
                    task.HasPictureProof = versionedTemplate.HasPictureProof;

                if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
                {
                    task.Tags = versionedTemplate.Tags;

                    if (task.Tags != null && task.Tags.Count > 0)
                    {
                        task.Tags = task.Tags.OrderBy(w => w.Id).ToList(); //tags don't have an index, add default sort by id
                    }
                }
            }

            return task;
        }

        public static Checklist ApplyTemplateVersion(this Checklist checklist, ChecklistTemplate versionedTemplate, string include = null)
        {
            if (versionedTemplate.Id == 0)
            {
                return checklist;
            }

            checklist.AreaId = versionedTemplate.AreaId;
            checklist.Description = versionedTemplate.Description;
            checklist.IsDoubleSignatureRequired = versionedTemplate.IsDoubleSignatureRequired;
            checklist.IsSignatureRequired = versionedTemplate.IsSignatureRequired;
            checklist.Name = versionedTemplate.Name;
            checklist.Picture = versionedTemplate.Picture;
            checklist.WorkInstructions = versionedTemplate.WorkInstructions;

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
            {
                checklist.Tags = versionedTemplate.Tags;

                if (checklist.Tags != null && checklist.Tags.Count > 0)
                {
                    checklist.Tags = checklist.Tags.OrderBy(w => w.Id).ToList(); //tags don't have an index, add default sort by id
                }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
            {
                checklist.OpenFieldsProperties = versionedTemplate.OpenFieldsProperties;

                if (checklist.OpenFieldsProperties != null && checklist.OpenFieldsProperties.Count > 0)
                {
                    checklist.OpenFieldsProperties = checklist.OpenFieldsProperties.OrderBy(p => p.Index).ToList();
                }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower()) && checklist.Tasks != null)
            {
                foreach (TasksTask item in checklist.Tasks)
                {
                    TaskTemplate versionedItemTemplate = versionedTemplate.TaskTemplates.FirstOrDefault(t => t.Id == item.TemplateId);
                    if (versionedItemTemplate != null)
                    {
                        item.AreaId = versionedItemTemplate.AreaId;
                        item.Description = versionedItemTemplate.Description;
                        item.DescriptionFile = versionedItemTemplate.DescriptionFile;
                        item.Index = versionedItemTemplate.Index;
                        item.Name = versionedItemTemplate.Name;
                        item.Picture = versionedItemTemplate.Picture;
                        item.TaskType = versionedItemTemplate.Type;
                        item.Video = versionedItemTemplate.Video;
                        item.VideoThumbnail = versionedItemTemplate.VideoThumbnail;
                        item.Attachments = versionedItemTemplate.Attachments;

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PictureProof.ToString().ToLower()))
                        {
                            item.HasPictureProof = versionedItemTemplate.HasPictureProof;
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
                        {
                            item.Tags = versionedItemTemplate.Tags;

                            if (item.Tags != null && item.Tags.Count > 0)
                            {
                                item.Tags = item.Tags.OrderBy(w => w.Id).ToList(); //tags don't have an index, add default sort by id
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower())))
                        {
                            item.Properties = versionedItemTemplate.Properties;

                            if (item.Properties != null && item.Properties.Count > 0)
                            {
                                item.Properties = item.Properties.OrderBy(s => s.Index).ToList();
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))
                        {
                            item.WorkInstructionRelations = versionedItemTemplate.WorkInstructionRelations;
                            item.WorkInstructions = versionedItemTemplate.WorkInstructions;

                            if (item.WorkInstructionRelations != null && item.WorkInstructionRelations.Count > 0)
                            {
                                item.WorkInstructionRelations = item.WorkInstructionRelations.OrderBy(w => w.Index).ToList();
                            }

                            if (item.WorkInstructions != null && item.WorkInstructions.Count > 0)
                            {
                                item.WorkInstructions = item.WorkInstructions.OrderBy(w => w.Id).ToList(); //work instructions don't have an index, add default sort by id
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))
                        {
                            item.Steps = versionedItemTemplate.Steps;
                            item.TaskSteps = versionedItemTemplate.Steps;

                            if (item.Steps != null && item.Steps.Count > 0)
                            {
                                item.Steps = item.Steps.OrderBy(s => s.Index).ToList();
                            }

                            if (item.TaskSteps != null && item.TaskSteps.Count > 0)
                            {
                                item.TaskSteps = item.TaskSteps.OrderBy(s => s.Index).ToList();
                            }
                        }
                    }
                }
                if (checklist.Tasks != null && checklist.Tasks.Count > 0)
                {
                    checklist.Tasks = checklist.Tasks.OrderBy(t => t.Index).ToList();
                }
            }
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower()) && checklist.Stages != null)
            {
                foreach (Stage stage in checklist.Stages)
                {
                    StageTemplate versionedStageTemplate = versionedTemplate.StageTemplates.FirstOrDefault(s => s.Id == stage.StageTemplateId);
                    if (versionedStageTemplate != null)
                    {
                        stage.Name = versionedStageTemplate.Name;
                        stage.Description = versionedStageTemplate.Description;
                        stage.BlockNextStagesUntilCompletion = versionedStageTemplate.BlockNextStagesUntilCompletion;
                        stage.LockStageAfterCompletion = versionedStageTemplate.LockStageAfterCompletion;
                        stage.UseShiftNotes = versionedStageTemplate.UseShiftNotes;
                        stage.NumberOfSignaturesRequired = versionedStageTemplate.NumberOfSignaturesRequired;
                        stage.NumberOfSignatures = versionedStageTemplate.NumberOfSignaturesRequired; //for backwards compatibility with very early version of transferable checklists in mobile app
                        stage.TaskTemplateIds = versionedStageTemplate.TaskTemplateIds;

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
                        {
                            stage.Tags = versionedStageTemplate.Tags;

                            if (stage.Tags != null && stage.Tags.Count > 0)
                            {
                                stage.Tags = stage.Tags.OrderBy(w => w.Id).ToList(); //tags don't have an index, add default sort by id
                            }
                        }
                    }
                }
            }

            return checklist;
        }

        public static Audit ApplyTemplateVersion(this Audit audit, AuditTemplate versionedTemplate, string include = null)
        {
            if (versionedTemplate.Id == 0)
            {
                return audit;
            }

            audit.AreaId = versionedTemplate.AreaId;
            audit.MinTaskScore = versionedTemplate.MinScore;
            audit.MaxTaskScore = versionedTemplate.MaxScore;
            audit.ScoreType = versionedTemplate.ScoreType;
            audit.Description = versionedTemplate.Description;
            audit.IsDoubleSignatureRequired = versionedTemplate.IsDoubleSignatureRequired;
            audit.IsSignatureRequired = versionedTemplate.IsSignatureRequired;
            audit.Name = versionedTemplate.Name;
            audit.Picture = versionedTemplate.Picture;
            audit.AreaPath = versionedTemplate.AreaPath;
            audit.AreaPathIds = versionedTemplate.AreaPathIds;

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))
            {
                audit.WorkInstructions = versionedTemplate.WorkInstructions;

                if (audit.WorkInstructions != null && audit.WorkInstructions.Count > 0)
                {
                    audit.WorkInstructions = audit.WorkInstructions.OrderBy(w => w.Id).ToList(); //work instructions don't have an index, add default sort by id
                }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
            {
                audit.Tags = versionedTemplate.Tags;

                if (audit.Tags != null && audit.Tags.Count > 0)
                {
                    audit.Tags = audit.Tags.OrderBy(t => t.Id).ToList(); //tags don't have an index, add default sort by id
                }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.OpenFields.ToString().ToLower()))
            {
                audit.OpenFieldsProperties = versionedTemplate.OpenFieldsProperties;

                if (audit.OpenFieldsProperties != null && audit.OpenFieldsProperties.Count > 0)
                {
                    audit.OpenFieldsProperties = audit.OpenFieldsProperties.OrderBy(t => t.Index).ToList();
                }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tasks.ToString().ToLower()) && audit.Tasks != null)
            {
                foreach (TasksTask item in audit.Tasks)
                {
                    TaskTemplate versionedItemTemplate = versionedTemplate.TaskTemplates.FirstOrDefault(t => t.Id == item.TemplateId);
                    if (versionedItemTemplate != null)
                    {
                        item.AreaId = versionedItemTemplate.AreaId;
                        item.Description = versionedItemTemplate.Description;
                        item.DescriptionFile = versionedItemTemplate.DescriptionFile;
                        item.RecurrencyType = versionedItemTemplate.RecurrencyType;
                        item.Recurrency = versionedItemTemplate.Recurrency;
                        item.AreaPath = versionedItemTemplate.AreaPath;
                        item.AreaPathIds = versionedItemTemplate.AreaPathIds;
                        item.MaxScore = versionedTemplate.MaxScore;
                        item.Index = versionedItemTemplate.Index;
                        item.Name = versionedItemTemplate.Name;
                        item.Picture = versionedItemTemplate.Picture;
                        item.TaskType = versionedItemTemplate.Type;
                        item.Video = versionedItemTemplate.Video;
                        item.VideoThumbnail = versionedItemTemplate.VideoThumbnail;
                        item.Template = versionedItemTemplate;
                        item.MachineStatus = versionedItemTemplate.MachineStatus;
                        item.PlannedTime = versionedItemTemplate.PlannedTime;
                        item.Attachments = versionedItemTemplate.Attachments;

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.PictureProof.ToString().ToLower()))
                        {
                            item.HasPictureProof = versionedItemTemplate.HasPictureProof;
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
                        {
                            item.Tags = versionedItemTemplate.Tags;

                            if (item.Tags != null && item.Tags.Count > 0)
                            {
                                item.Tags = item.Tags.OrderBy(t => t.Id).ToList(); //tags don't have an index, add default sort by id
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.PropertyValues.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.Properties.ToString().ToLower()) || include.Split(",").Contains(IncludesEnum.PropertyDetails.ToString().ToLower())))
                        {
                            item.Properties = versionedItemTemplate.Properties;

                            if (item.Properties != null && item.Properties.Count > 0)
                            {
                                item.Properties = item.Properties.OrderBy(t => t.Index).ToList();
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.InstructionRelations.ToString().ToLower()))
                        {
                            item.WorkInstructionRelations = versionedItemTemplate.WorkInstructionRelations;
                            item.WorkInstructions = versionedItemTemplate.WorkInstructions;

                            if (item.WorkInstructionRelations != null && item.WorkInstructionRelations.Count > 0)
                            {
                                item.WorkInstructionRelations = item.WorkInstructionRelations.OrderBy(w => w.Index).ToList();
                            }

                            if (item.WorkInstructions != null && item.WorkInstructions.Count > 0)
                            {
                                item.WorkInstructions = item.WorkInstructions.OrderBy(w => w.Id).ToList(); //work instructions don't have an index, add default sort by id
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Steps.ToString().ToLower()))
                        {
                            item.Steps = versionedItemTemplate.Steps;
                            item.TaskSteps = versionedItemTemplate.Steps;

                            if (item.Steps != null && item.Steps.Count > 0)
                            {
                                item.Steps = item.Steps.OrderBy(s => s.Index).ToList();
                            }

                            if (item.TaskSteps != null && item.TaskSteps.Count > 0)
                            {
                                item.TaskSteps = item.TaskSteps.OrderBy(s => s.Index).ToList();
                            }
                        }
                    }
                }

                if (audit.Tasks != null && audit.Tasks.Count > 0)
                {
                    audit.Tasks = audit.Tasks.OrderBy(t => t.Index).ToList();
                }
            }

            return audit;
        }

        public static Assessment ApplyTemplateVersion(this Assessment assessment, AssessmentTemplate versionedTemplate, string include = null)
        {
            if (versionedTemplate.Id == 0)
            {
                return assessment;
            }

            assessment.Name = versionedTemplate.Name;
            assessment.Description = versionedTemplate.Description;
            assessment.Picture = versionedTemplate.Picture;
            assessment.AssessmentType = versionedTemplate.AssessmentType;
            assessment.AreaId = versionedTemplate.AreaId;
            assessment.AreaPath = versionedTemplate.AreaPath;
            assessment.AreaPathIds = versionedTemplate.AreaPathIds;
            assessment.Media = versionedTemplate.Media;
            assessment.Role = versionedTemplate.Role;
            assessment.SignatureType = versionedTemplate.SignatureType;
            assessment.SignatureRequired = versionedTemplate.SignatureRequired;
            assessment.NumberOfSkillInstructions = versionedTemplate.NumberOfSkillInstructions;

            if (!string.IsNullOrEmpty(include) && (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())))
            {
                assessment.Tags = versionedTemplate.Tags;

                if (assessment.Tags != null && assessment.Tags.Count > 0)
                {
                    assessment.Tags = assessment.Tags.OrderBy(t => t.Id).ToList(); //add default sort by id because tags don't have an index
                }
            }

            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Instructions.ToString().ToLower()) && assessment.SkillInstructions != null)
            {
                foreach (var skillInstruction in assessment.SkillInstructions)
                {
                    AssessmentTemplateSkillInstruction versionedInstructionTemplate = null;
                    if (versionedTemplate.SkillInstructions != null)
                    {
                        versionedInstructionTemplate = versionedTemplate.SkillInstructions.FirstOrDefault(t => t.Id == skillInstruction.AssessmentTemplateSkillInstructionId);
                    }

                    if (versionedInstructionTemplate != null)
                    {
                        skillInstruction.Name = versionedInstructionTemplate.Name;
                        skillInstruction.Description = versionedInstructionTemplate.Description;
                        skillInstruction.Picture = versionedInstructionTemplate.Picture;
                        skillInstruction.AreaId = versionedInstructionTemplate.AreaId;
                        skillInstruction.AreaPath = versionedInstructionTemplate.AreaPath;
                        skillInstruction.AreaPathIds = versionedInstructionTemplate.AreaPathIds;
                        skillInstruction.Media = versionedInstructionTemplate.Media;
                        skillInstruction.WorkInstructionType = versionedInstructionTemplate.WorkInstructionType;
                        skillInstruction.Role = versionedInstructionTemplate.Role;
                        skillInstruction.IsAvailableForAllAreas = versionedInstructionTemplate.IsAvailableForAllAreas;

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
                        {
                            skillInstruction.Tags = versionedInstructionTemplate.Tags;

                            if (skillInstruction.Tags != null && skillInstruction.Tags.Count > 0)
                            {
                                skillInstruction.Tags = skillInstruction.Tags.OrderBy(t => t.Id).ToList(); //add default sort by id because tags don't have an index
                            }
                        }

                        if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.InstructionItems.ToString().ToLower()) && skillInstruction.InstructionItems != null)
                        {
                            foreach (var instructionItem in skillInstruction.InstructionItems)
                            {
                                InstructionItemTemplate versionedInstructionItemTemplate = versionedInstructionTemplate.InstructionItems.FirstOrDefault(t => t.Id == instructionItem.WorkInstructionTemplateItemId);

                                if (versionedInstructionItemTemplate != null)
                                {
                                    instructionItem.WorkInstructionTemplateId = versionedInstructionItemTemplate.InstructionTemplateId;
                                    instructionItem.AssessmentTemplateId = versionedInstructionItemTemplate.AssessmentTemplateId;
                                    instructionItem.Name = versionedInstructionItemTemplate.Name;
                                    instructionItem.Description = versionedInstructionItemTemplate.Description;
                                    instructionItem.Picture = versionedInstructionItemTemplate.Picture;
                                    instructionItem.Video = versionedInstructionItemTemplate.Video;
                                    instructionItem.VideoThumbnail = versionedInstructionItemTemplate.VideoThumbnail;
                                    instructionItem.Media = versionedInstructionItemTemplate.Media;
                                    instructionItem.Index = versionedInstructionItemTemplate.Index;
                                    instructionItem.Attachments = versionedInstructionItemTemplate.Attachments;

                                    if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower()))
                                    {
                                        instructionItem.Tags = versionedInstructionItemTemplate.Tags;

                                        if (instructionItem.Tags != null && instructionItem.Tags.Count > 0)
                                        {
                                            instructionItem.Tags = instructionItem.Tags.OrderBy(t => t.Id).ToList(); //add default sort by id because tags don't have an index
                                        }

                                    }
                                }
                            }

                            if (skillInstruction.InstructionItems != null && skillInstruction.InstructionItems.Count > 0)
                            {
                                skillInstruction.InstructionItems = skillInstruction.InstructionItems.OrderBy(t => t.Index).ToList();
                            }
                        }
                    }
                }

                if (assessment.SkillInstructions != null && assessment.SkillInstructions.Count > 0)
                {
                    assessment.SkillInstructions = assessment.SkillInstructions.OrderBy(t => t.Index).ToList();
                }
            }

            return assessment;
        }
    }
}
