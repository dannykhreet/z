using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.Filters;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.OpenFields;
using EZGO.Maui.Core.Models.Stages;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Checklists
{
    public class ChecklistModel : EZGO.Api.Models.Checklist, IOpenTextFields, ICompletedTemplate
    {
        public Guid LocalGuid { get; set; }

        public new DateTime CreatedAt { get; set; }

        public new List<SignatureModel> Signatures { get; set; }

        public new List<TasksTaskModel> Tasks { get; set; }

        public new List<StageModel> Stages { get; set; }

        public bool HasStages => Stages != null && Stages.Any();

        public SignatureModel FirstSignature { get => Signatures?.FirstOrDefault() ?? new SignatureModel { SignedBy = "Unknown" }; }

        public SignatureModel SecondSignature { get => Signatures?.ElementAtOrDefault(1) ?? null; }

        public string SignatureString
        {
            get
            {
                StringBuilder signature = new StringBuilder(string.Empty);
                signature.Append(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy));
                signature.Append($" {FirstSignature.SignedBy}");

                if (SecondSignature != null)
                {
                    signature.Append(',');
                    signature.Append($" {SecondSignature.SignedBy}");
                }
                return signature.ToString();
            }
        }

        public LocalDateTime LocalSignedAt
        {
            get
            {
                if (FirstSignature.SignedAt.HasValue)
                    return Settings.ConvertDateTimeToLocal(FirstSignature.SignedAt.Value.ToLocalTime());
                else return Settings.ConvertDateTimeToLocal(ModifiedAt.Value);
            }
        }

        public LocalDateTime LastChange => Settings.ConvertDateTimeToLocal(ModifiedAt.Value.ToLocalTime());
        public LocalDateTime LocalCreatedAt => Settings.ConvertDateTimeToLocal(CreatedAt.ToLocalTime());

        public string Date
        {
            get
            {
                var dt = ModifiedAt ?? DateTime.MinValue;
                if (dt != DateTime.MinValue)
                    return string.Format("{0:MMM dd, yyyy, HH:mm}", dt);
                else return string.Empty;
            }
        }

        public void SetupAfterLoaded()
        {
            SetTasksAmount();
            SetHasPictureProof();
        }

        private void SetHasPictureProof()
        {
            foreach (var task in Tasks)
            {
                if (task.TaskStatus == TaskStatusEnum.Skipped || task.TaskStatus == TaskStatusEnum.Todo)
                    task.HasPictureProof = false;
            }
        }

        private void SetTasksAmount()
        {
            TaskFilterControl.SetUnfilteredItems(Tasks);
            TaskHelper.CalculateTaskAmounts(TaskFilterControl);
        }

        public List<TemplatePropertyModel> OpenFieldsProperties { get; set; }
        public List<UserValuesPropertyModel> OpenFieldsPropertyUserValues { get; set; }

        public bool IsPieChartVisible { get; set; } = true;

        public FilterControl<TasksTaskModel, TaskStatusEnum> TaskFilterControl { get; set; } = new FilterControl<TasksTaskModel, TaskStatusEnum>(null);

        public List<StatusModel<TaskStatusEnum>> TaskStatuses => TaskFilterControl.TaskStatusList.StatusModels;

        public int PercentageFinished { get; set; } = 0;

        public void SetPercentageFinished()
        {
            if (Tasks == null)
                return;

            var tasksWithStatusCount = Tasks.Count(t => t.TaskStatus != TaskStatusEnum.Todo);
            var percentageFinished = (int)Math.Round((double)(tasksWithStatusCount * 100) / Tasks.Count, mode: MidpointRounding.AwayFromZero);
            PercentageFinished = percentageFinished > 0 ? percentageFinished : 0;
        }

        public List<int> EditedByUsersId => EditedByUsers?.Select(x => x.Id).ToList() ?? new List<int>();
    }
}
