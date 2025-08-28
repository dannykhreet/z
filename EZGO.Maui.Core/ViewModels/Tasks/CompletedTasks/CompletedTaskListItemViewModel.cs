using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Models;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Utils;
using System.Text;
using EZGO.Maui.Core.Extensions;
using EZGO.Api.Models.Tags;

namespace EZGO.Maui.Core.ViewModels.Tasks.CompletedTasks
{
    public class CompletedTaskListItemViewModel : NotifyPropertyChanged, IBase<BasicTaskModel>, IItemFilter<TaskStatusEnum>
    {
        public long Id { get; set; }

        public int TaskTemplateId { get; set; }

        public int ShiftId { get; set; }

        public string Name { get; set; }

        public string Picture { get; set; }

        public TaskStatusEnum TaskStatus { get; set; }

        public string TimeRealizedBy { get; set; }

        public DateTime? DueAtDT { get; set; }
        public string DueAt { get; set; }

        public DateTime? SignedAt { get; set; }

        public string SignedBy { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public bool HasComment => !string.IsNullOrWhiteSpace(Comment) && TaskStatus == TaskStatusEnum.Skipped;

        public string CommentString => HasComment ? $"{EZGO.Maui.Core.Extensions.TranslateExtension.GetValueFromDictionary(LanguageConstants.multiskipTaskComment)} \"{Comment}\"" : null;

        public List<StepModel> Steps { get; set; }

        public bool HasSteps => Steps?.Any() ?? false;

        public string DescriptionFile { get; set; }

        public List<InstructionsModel> WorkInstructionRelations { get; set; }

        public bool HasWorkInstructions => WorkInstructionRelations?.Any() ?? false;

        public bool HasStepsOrWorkInstructions => HasSteps || HasWorkInstructions;

        public bool HasExtraInformation => HasWorkInstructions || HasSteps || HasDescriptionFile;
        public bool HasDescriptionFile
        {
            get => !string.IsNullOrWhiteSpace(DescriptionFile);
        }

        public bool Ok => TaskStatus == TaskStatusEnum.Ok;

        public bool NotOk => TaskStatus == TaskStatusEnum.NotOk;

        public bool Skipped => TaskStatus == TaskStatusEnum.Skipped;
        public bool Todo => TaskStatus == TaskStatusEnum.Todo;

        public bool Action { get; set; }

        public bool HasProperties => !string.IsNullOrWhiteSpace(PropertyValuesString);

        public string PropertyValuesString { get; set; }

        public bool IsTaskMarked { get; set; }

        public string TaskMarkedString { get; set; }

        public string DueDateString { get; set; }

        public string DueDateTaskMarkedString => $"{DueDateString} {TaskMarkedString}";

        public string TaskDetailsString => GetTaskDetailString();

        public int ActionBubbleCount { get; set; }

        public string VideoThumbnail { get; set; }

        public bool HasVideo => !string.IsNullOrEmpty(Video) && !string.IsNullOrEmpty(VideoThumbnail);

        public string DisplayPicture => HasVideo ? VideoThumbnail : Picture;

        public string Video { get; set; }

        public SignatureModel Signature { get; set; }

        public int? Score { get; set; }
        public int? MaxScore { get; set; }

        public bool IsScoreButtonVisible { get; set; } = false;

        public bool IsStatusButtonVisible => !IsScoreButtonVisible && TaskStatus != TaskStatusEnum.Todo;

        public IScoreColorCalculator ScoreColorCalculator { get; set; }

        private void SetScoreCalculator()
        {
            ScoreColorCalculator = ScoreColorCalculatorFactory.Default(0, MaxScore ?? 5);
        }
        public TaskStatusEnum FilterStatus { get; set; }

        public bool HasFeatures => PropertyList?.Any() ?? false;
        public List<PropertyUserValue> PropertyUserValues { get; }
        public IReadOnlyList<BasicTaskPropertyModel> PropertyList { get; }

        public bool HasPictureProof { get; private set; }

        public IReadOnlyList<MediaItem> PictureProofMediaItems { get; set; }

        public List<Tag> Tags { get; set; }


        public CompletedTaskListItemViewModel(BasicTaskModel source)
        {
            Action = (source.ActionsCount > 0 || source.CommentCount > 0);
            Id = source.Id;
            TaskTemplateId = source.TemplateId;
            ShiftId = source.ShiftId ?? -1;
            Name = source.Name;
            Picture = source.Picture;
            TaskStatus = source.FilterStatus;
            TimeRealizedBy = source.TimeRealizedBy;
            DueAt = source.DueDateString;
            DueAtDT = source.DueAt;
            SignedAt = source.Signature?.SignedAt?.ToLocalTime();
            SignedBy = source.Signature?.SignedBy;
            Description = source.Description;
            Comment = source.Comment;
            Steps = source.Steps;
            DescriptionFile = source.DescriptionFile;
            PropertyValuesString = source.PropertyValuesString;
            IsTaskMarked = source.IsTaskMarked;
            TaskMarkedString = source.TaskMarkedString ?? "";
            DueDateString = source.DueDateString;
            ActionBubbleCount = source.ActionBubbleCount + source.ActionsCount;
            WorkInstructionRelations = source.WorkInstructionRelations;
            VideoThumbnail = source.VideoThumbnail;
            Video = source.Video;
            Signature = source.Signature;
            Score = source.Score;
            MaxScore = source.MaxScore;
            PropertyList = source.PropertyList?.Where(x => x.UserValue != null).ToList();
            HasPictureProof = source.PictureProofMediaItems?.Any() ?? false;
            PictureProofMediaItems = source.PictureProofMediaItems;
            Tags = source.Tags;

            if (IsScoreButtonVisible)
                SetScoreCalculator();
            FilterStatus = source.FilterStatus;
        }

        public CompletedTaskListItemViewModel(TasksTaskModel source)
        {
            Action = (source.ActionsCount > 0 || source.CommentCount > 0);
            Id = source.Id;
            TaskTemplateId = source.TemplateId;
            ShiftId = source.ShiftId ?? -1;
            Name = source.Name;
            Picture = source.Picture;
            TaskStatus = source.TaskStatus;
            TimeRealizedBy = source.TimeRealizedBy;
            DueAt = source.DueDateString;
            DueAtDT = source.DueAt;
            SignedAt = source.Signature?.SignedAt?.ToLocalTime();
            SignedBy = source.Signature?.SignedBy;
            Description = source.Description;
            Comment = source.Comment;
            Steps = source.Steps;
            DescriptionFile = source.DescriptionFile;
            PropertyValuesString = source.PropertyValuesString;
            IsTaskMarked = source.IsTaskMarked;
            TaskMarkedString = source.TaskMarkedString ?? "";
            DueDateString = source.DueDateString;
            ActionBubbleCount = source.ActionsBubbleCount;
            WorkInstructionRelations = source.WorkInstructionRelations;
            if (source.Signature != null)
            {
                var signature = new SignatureModel() { SignatureImage = source.Signature.SignatureImage, SignedAt = source.Signature.SignedAt, SignedBy = source.Signature.SignedBy, SignedById = source.Signature.SignedById };
                Signature = signature;
            }
            VideoThumbnail = source.VideoThumbnail;
            Video = source.Video;
            Score = source.Score;
            MaxScore = source.MaxScore;
            IsScoreButtonVisible = source.IsScoreButtonVisible;
            PropertyList = source.UserPropertyList;
            HasPictureProof = source.HasPictureProof && (source.PictureProofMediaItems?.Any() ?? false);
            PictureProofMediaItems = source.PictureProofMediaItems;
            Tags = source.Tags;

            if (IsScoreButtonVisible)
                SetScoreCalculator();
        }

        public BasicTaskModel ToBasic()
        {
            BasicTaskModel result = new BasicTaskModel()
            {
                ActionsCount = ActionBubbleCount,
                Id = Id,
                TemplateId = TaskTemplateId,
                ShiftId = ShiftId,
                Name = Name,
                Picture = Picture,
                FilterStatus = TaskStatus,
                TimeRealizedBy = TimeRealizedBy,
                DueAt = DueAtDT,
                Signature = Signature,
                Description = Description,
                Comment = Comment,
                Steps = Steps,
                DescriptionFile = DescriptionFile,
                WorkInstructionRelations = WorkInstructionRelations,
                Video = Video,
                VideoThumbnail = VideoThumbnail,
                Tags = Tags
            };
            return result;
        }

        private string GetTaskDetailString()
        {
            StringBuilder result = new StringBuilder();
            if (DeviceSettings.ScreenDencity < 8)
                result.AppendLine(DueDateTaskMarkedString);
            else
            {
                result.AppendLine(DueDateString);
                if (!TaskMarkedString.IsNullOrEmpty())
                    result.AppendLine(TaskMarkedString);
            }

            if (!PropertyValuesString.IsNullOrEmpty())
                result.AppendLine(PropertyValuesString);

            if (!CommentString.IsNullOrEmpty())
                result.AppendLine(CommentString);

            var result1 = result.ToString();
            return result.ToString();
        }
    }
}
