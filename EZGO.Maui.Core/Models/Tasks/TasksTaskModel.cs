using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.ModelInterfaces;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class TasksTaskModel : TasksTask, IBase<BasicTaskModel>, IItemFilter<TaskStatusEnum>
    {
        public new Signature Signature { get; set; }
        public new List<ActionsModel> Actions { get; set; }
        public List<CommentModel> LocalComments { get; set; }
        public List<ActionsModel> LocalActions { get; set; }
        public bool HasSignature => !Signature?.SignedBy.IsNullOrEmpty() ?? false;
        public new List<PropertyTaskTemplateModel> Properties { get; set; }

        public new List<StepModel> Steps { get; set; }

        private string _signatureImage;

        public string SignatureImage
        {
            get { return string.Format(Constants.MediaBaseUrl, Signature.SignatureImage); }
            set { _signatureImage = value; }
        }

        public List<int> EditedByUsersId
        {
            get
            {
                if (EditedByUsers != null && EditedByUsers.Count > 0)
                {
                    var editedIds = EditedByUsers.Select(x => x.Id).ToList();
                    return editedIds;
                }
                else
                {
                    return new List<int>();
                }
            }
        }

        public LocalDateTime LocalSignedAt
        {
            get
            {
                if (Signature?.SignedAt.HasValue ?? false)
                    return Settings.ConvertDateTimeToLocal(Signature.SignedAt.Value.ToLocalTime());
                else if (ModifiedAt.HasValue) return Settings.ConvertDateTimeToLocal(ModifiedAt.Value);
                else return new LocalDateTime();
            }
        }

        public TaskStatusEnum TaskStatus => (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), Status?.Replace(" ", string.Empty) ?? "skipped", true);

        public new int TemplateId { get; set; }

        public int Percentage { get; set; }

        public new int ActionsCount
        {
            get => actionsCount + (LocalActions?.Count ?? 0);
            set => actionsCount = value;
        }

        public new int CommentCount { get; set; } = 0;

        public bool HasActions => ActionsCount != 0;

        public bool HasComments => CommentCount != 0;

        public int ActionsBubbleCount { get => ActionsCount + CommentCount; }

        public bool HasCommentsOrActions => HasActions || HasComments;

        public new int TotalScore { get; set; }

        public Audits.ScoreModel NewScore { get; set; }

        public ScoreTypeEnum ScoreType { get; set; }

        public bool IsScoreButtonVisible { get => Score != null && TaskStatus == TaskStatusEnum.Todo; }

        public string DueDateString => DueAt.HasValue ? $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailDue)} {FormatDateTime(Settings.ConvertDateTimeToLocal(DueAt))}." : null;

        public bool IsTaskMarked => TimeTaken.HasValue || Signature != null;

        public List<InstructionsModel> WorkInstructionRelations { get; set; }

        public bool HasFeatures { get; private set; }

        public List<MediaItem> PictureProofMediaItems { get; set; }

        public bool IsThumbsUpBadgeVisible => (PictureProof?.Media?.Any() ?? false) && TaskStatus == TaskStatusEnum.Ok;
        public bool IsThumbsDownBadgeVisible => (PictureProof?.Media?.Any() ?? false) && TaskStatus == TaskStatusEnum.NotOk;
        public bool IsScoreBadgeVisible => (PictureProofMediaItems?.Any() ?? false) && ScoreType == ScoreTypeEnum.Score;

        public bool HasPictureProof { get; set; }

        public string TaskMarkedString
        {
            get
            {
                StringBuilder markedOnString = new StringBuilder(string.Empty);
                string name = string.Empty;

                if ((Signature != null && TaskStatus != TaskStatusEnum.Todo) || TimeTaken.HasValue)
                {
                    markedOnString.Append($"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarked)}");
                }

                if (Signature != null && TaskStatus != TaskStatusEnum.Todo)
                {
                    // NOTE need to convert the SignetAt date to local because the API returns all dates in local except this one, which is in UTC
                    markedOnString.Append($" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedOn)} {FormatDateTime(Settings.ConvertDateTimeToLocal(Signature.SignedAt?.ToLocalTime()))}");
                    name = Signature.SignedBy;
                }

                if (TimeTaken.HasValue)
                {
                    markedOnString.Append($" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskRealizedTimeIn)} {TimeTaken.Value} {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskMinutesLabelText)}");
                    name = Signature?.SignedBy ?? TimeRealizedBy;
                }

                if (!string.IsNullOrWhiteSpace(markedOnString.ToString()))
                {
                    markedOnString.Append($" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy)} {name}");
                }

                return markedOnString.ToString();
            }
        }

        private string picture;
        private int actionsCount = 0;

        public new string Picture
        {
            get => !HasVideo ? picture : VideoThumbnail;
            set
            {
                picture = value;
            }
        }

        public bool HasVideo => !Video.IsNullOrWhiteSpace();
        public IReadOnlyList<BasicTaskPropertyModel> PropertyList { get; private set; }
        public IReadOnlyList<BasicTaskPropertyModel> UserPropertyList { get; private set; }

        public string PropertyValuesString { get; set; }
        public string PropertyValuesStringWithMore { get; set; }
        public bool HasMoreProperties => UserPropertyList.Count > 1;
        public TaskStatusEnum FilterStatus { get => TaskStatus; set { } }

        public int? StageId { get; set; }

        public void CreatePropertyList()
        {
            var result = new List<BasicTaskPropertyModel>();
            int i = 0;

            if (Properties != null && Properties.Any())
            {
                result.AddRange(Properties.OrderBy(x => x.Index)
                                            .Select((template, index) =>
                    BasicTaskPropertyModel.FromTemplateAndValue(template, PropertyUserValues?.Find(value => value.TemplatePropertyId == template.Id), index + i)));
            }

            PropertyList = result;
            HasFeatures = result?.Any() ?? false;
            CreatePropertyValueString();
        }

        private static string FormatDateTime(DateTime? date)
        {
            string result = string.Empty;

            if (date.HasValue)
                result = $"{date.Value:d MMM yyyy}, {date.Value.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortTimePattern)}";

            return result;
        }

        private static string FormatDateTime(LocalDateTime? date)
        {
            string result = string.Empty;

            if (date.HasValue)
                result = date.Value.ToString(BaseDateFormats.DayFirstShortMonthDateTimeFormat, null);

            return result;
        }

        private void CreatePropertyValueString()
        {
            var userPropertyList = new List<BasicTaskPropertyModel>();

            var result = "";
            if (PropertyList != null)
            {
                foreach (var property in PropertyList)
                {
                    if (property.IsPlannedTimeProperty)
                        continue;

                    var userValueString = property.GetUserValueString();
                    if (!string.IsNullOrEmpty(userValueString))
                    {
                        userPropertyList.Add(property);
                        try
                        {
                            if (!result.IsNullOrEmpty())
                            {
                                PropertyValuesStringWithMore = result;
                            }

                            if (!string.IsNullOrEmpty(result))
                                result += ",\n";

                            var dateToUse = property.UserValue?.RegisteredAt ?? property.UserValue?.ModifiedAt;
                            string formattedDate = FormatDateTime(Settings.ConvertDateTimeToLocal(dateToUse?.ToLocalTime() ?? DateTime.Now));

                            result += $"{property.DisplayTitleString}: {userValueString} {property.DisplayFooterString} " +
                                      $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedOn)} {formattedDate}";

                            if (!property?.UserValue?.ModifiedBy.IsNullOrEmpty() ?? false)
                            {
                                result += $" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy)} " +
                                          $"{property?.UserValue.ModifiedBy}";
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            PropertyValuesString = result;
            UserPropertyList = userPropertyList;
        }

        public BasicTaskModel ToBasic()
        {
            BasicTaskModel result = new BasicTaskModel()
            {
                ActionsCount = ActionsBubbleCount,
                Id = Id,
                TemplateId = TemplateId,
                ShiftId = ShiftId ?? -1,
                Name = Name,
                Picture = Picture,
                FilterStatus = TaskStatus,
                TimeRealizedBy = TimeRealizedBy,
                DueAt = DueAt ?? DateTime.Today,
                Signature = (SignatureModel)Signature,
                Description = Description,
                Steps = Steps,
                DescriptionFile = DescriptionFile,
                WorkInstructionRelations = WorkInstructionRelations,
                Video = Video,
                VideoThumbnail = VideoThumbnail,
                Attachments = Attachments
            };
            return result;
        }

        public void SetPictureProofMediaItems(bool setHasPictureProof = true)
        {
            var result = new List<MediaItem>();
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof && (PictureProof?.Media.Any() ?? false))
            {
                result = PictureProof.Media.Select(x => MediaItem.OnlinePicture(x.UriPart)).ToList();
            }
            PictureProofMediaItems ??= result;
            if (setHasPictureProof)
                HasPictureProof = (PictureProofMediaItems?.Any() ?? false) && (TaskStatus != TaskStatusEnum.Skipped && TaskStatus != TaskStatusEnum.Todo || IsScoreButtonVisible);
        }
    }
}

