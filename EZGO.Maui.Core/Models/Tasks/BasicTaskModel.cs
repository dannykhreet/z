using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Classes.DateFormats;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using Newtonsoft.Json;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class BasicTaskModel : NotifyPropertyChanged, IItemFilter<TaskStatusEnum>, ITaskTemplateModel, IDetailItem
    {
        public long Id { get; set; }

        public int? Index { get; set; }

        public int TemplateId { get; set; }

        public int? ShiftId { get; set; }

        public string RecurrencyType { get; set; }

        public string Name { get; set; }

        public bool IsScoreBadgeVisible => false;
        public bool IsScoreButtonVisible => false;

        public int? AuditId { get; set; }
        public int? ChecklistId { get; set; }

        /// <summary>
        /// Id of the area this task is assigned to
        /// </summary>
        public int AreaId { get; set; }

        public string AreaPath { get; set; }

        public string Picture { get; set; }

        public string Video { get; set; }

        public string VideoThumbnail { get; set; }

        public bool HasVideo => !string.IsNullOrEmpty(Video) && !string.IsNullOrEmpty(VideoThumbnail);

        public string DisplayPicture => HasVideo ? VideoThumbnail : Picture;

        public string Status { get; set; }

        public Stream PDFStream { get; set; } = null;

        private TaskStatusEnum? taskStatus;
        public TaskStatusEnum FilterStatus
        {
            get
            {
                TaskStatusEnum result;

                if (!taskStatus.HasValue)
                    result = (TaskStatusEnum)Enum.Parse(typeof(TaskStatusEnum), Status?.Replace(" ", string.Empty) ?? "skipped", true);
                else
                    result = taskStatus.Value;

                return result;
            }
            set
            {
                taskStatus = value;

                OnPropertyChanged();
            }
        }

        public int? Score { get; set; } = null;
        public int MaxScore { get; set; }
        public int Percentage { get; set; }

        public string Description { get; set; }

        public string Comment { get; set; }

        public bool HasComment => !string.IsNullOrWhiteSpace(Comment) && FilterStatus == TaskStatusEnum.Skipped;

        public List<StepModel> Steps { get; set; }

        public bool HasSteps => Steps?.Any() ?? false;

        public bool HasStepsOrWorkInstructions => HasSteps || HasWorkInstructions;

        public bool HasWorkInstructions => WorkInstructionRelations?.Any() ?? false;

        public string DescriptionFile { get; set; }

        public bool HasDescriptionFile => !string.IsNullOrWhiteSpace(DescriptionFile);

        public bool HasExtraInformation => HasWorkInstructions || HasSteps || HasAttachments;

        private int? plannedTime;

        public int? PlannedTime
        {
            get
            {
                if (timeTaken != null)
                    return timeTaken;
                return plannedTime;
            }
            set
            {
                plannedTime = value;

            }
        }

        public int? DeepLinkId { get; set; }

        public string DeepLinkTo { get; set; }

        public string MachineStatus { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? StartDate { get; set; }

        public MachineStatusEnum TaskMachineStatus => (MachineStatusEnum)Enum.Parse(typeof(MachineStatusEnum), MachineStatus ?? "not_applicable", true);

        public int? TimeRealizedById { get; set; }

        public int OpenActionCount { get; set; }
        public int CommentCount { get; set; }
        public int ActionBubbleCount => OpenActionCount + CommentCount;

        public int ActionsCount { get; set; }

        private string timeRealizedBy;

        public string TimeRealizedBy
        {
            get => timeRealizedBy;
            set
            {
                timeRealizedBy = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTaskMarked));
                OnPropertyChanged(nameof(TaskMarkedString));
            }
        }

        private int? timeTaken;

        public int? TimeTaken
        {
            get => timeTaken;
            set
            {
                timeTaken = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTaskMarked));
                OnPropertyChanged(nameof(TaskMarkedString));
                OnPropertyChanged(nameof(PlannedTime));
            }
        }

        public DateTime? DueAt { get; set; }

        public string DueDateString => DueAt.HasValue ? $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailDue)} {FormatDateTime(Settings.ConvertDateTimeToLocal(DueAt))}." : null;

        public bool IsOverdue { get; set; }
        //public string OverDueString => IsOverdue ? $"{CheckOverdue(DueAt, TaskStatus)}": null;
        public string OverDueString => IsOverdue ? $"({TranslateExtension.GetValueFromDictionary(LanguageConstants.galleryScreenShiftOverdueText)})" : null;

        public string CommentString => HasComment ? $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.multiskipTaskComment)} \"{Comment}\"" : null;

        public bool ShowPlannedTimeButton => PlannedTime.HasValue && !DeepLinkId.HasValue;

        public bool IsTaskMarked => TimeTaken.HasValue || Signature != null;

        public bool? DeepLinkCompletionIsRequired { get; set; } = false;

        public bool IsDeepLinkValid { get; set; } = true;

        public int? CompletedDeeplinkId { get; set; }

        public bool IsStageLocked { get; set; } = false;

        /// <summary>
        /// Used in the Slide view scenario to show the sidebar
        /// </summary>
        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }


        private SignatureModel signature;
        public SignatureModel Signature
        {
            get => signature;
            set
            {
                signature = value;
                if (OriginalSignature == null) { OriginalSignature = value; }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTaskMarked));
                OnPropertyChanged(nameof(TaskMarkedString));
            }
        }

        public SignatureModel OriginalSignature { get; set; }

        public string TaskMarkedString
        {
            get
            {
                StringBuilder markedOnString = new StringBuilder(string.Empty);
                string name = string.Empty;

                if ((Signature != null && FilterStatus != TaskStatusEnum.Todo))
                {
                    markedOnString.Append($"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarked)}");
                }

                if (Signature != null && FilterStatus != TaskStatusEnum.Todo)
                {
                    // NOTE need to convert the SignetAt date to local because the API returns all dates in local except this one, which is in UTC
                    markedOnString.Append($" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedOn)} {FormatDateTime(Settings.ConvertDateTimeToLocal(Signature.SignedAt?.ToLocalTime()))}");
                    name = Signature.SignedBy;
                }

                if (!string.IsNullOrWhiteSpace(markedOnString.ToString()))
                {
                    markedOnString.Append($" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy)} {name}");
                }

                return markedOnString.ToString();
            }
        }

        public LocalDateTime TaskMarkedDate
        {
            get
            {
                if (!IsTaskMarked)
                    return DateTimeHelper.MinValue;

                if (Signature != null && FilterStatus != TaskStatusEnum.Todo)
                {
                    return Settings.ConvertDateTimeToLocal(Signature.SignedAt?.ToLocalTime()) ?? DateTimeHelper.MinValue;
                }
                return DateTimeHelper.MinValue;
            }
        }

        private static string FormatDateTime(LocalDateTime? date)
        {
            string result = string.Empty;

            if (date.HasValue)
                result = date.Value.ToString(BaseDateFormats.DayFirstShortMonthDateTimeFormat, CultureInfo.CurrentUICulture);

            return result;
        }

        public TaskPeriodTypes TaskPeriods { get; set; }

        public List<InstructionsModel> WorkInstructionRelations { get; set; }

        public bool HasPictureProof { get; set; }

        public List<MediaItem> PictureProofMediaItems { get; set; }

        public bool IsThumbsUpBadgeVisible => HasPictureProof && FilterStatus == TaskStatusEnum.Ok;
        public bool IsThumbsDownBadgeVisible => HasPictureProof && FilterStatus == TaskStatusEnum.NotOk;

        public PictureProof PictureProof { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Attachment> Attachments { get; set; }

        public bool HasAttachments => Attachments?.Count > 0;

        private AttachmentEnum? _attachmentType;

        public AttachmentEnum? AttachmentType
        {

            get
            {
                if (HasAttachments)
                    return Attachments?.FirstOrDefault().AttachmentType.ToLower() switch
                    {
                        "pdf" => AttachmentEnum.Pdf,
                        _ => AttachmentEnum.Link
                    };
                else
                    return null;

            }


            private set { _attachmentType = value; }

        }

        public bool IsCompleted { get; set; } = true;

        #region Properties & features

        [JsonIgnore]
        public bool HasFeatures { get; private set; }

        //TODO Move Property Methods to Separate Class from here and BasicTaskTemplateModel
        public List<PropertyTaskTemplateModel> Properties { get; set; } = new List<PropertyTaskTemplateModel>();

        public List<PropertyUserValue> PropertyUserValues { get; set; } = new List<PropertyUserValue>();

        /// <summary>
        /// Updates user's property value.
        /// </summary>
        /// <param name="newValue">The value to update.</param>
        /// <returns>True if the update was necessary otherwise false.</returns>
        public bool AddOrUpdateUserProperty(TaskPropertyUserValueBasic newValue)
        {
            var property = PropertyList.Where(x => x.Id == newValue.TemplatePropertyId).FirstOrDefault();

            if (property != null)
            {
                var userValue = PropertyUserValues.FirstOrDefault(x => x.Id == newValue.Id);
                var updateNeeded = (userValue?.ModifiedAt ?? DateTime.MinValue) != newValue.ModifiedAt;

                if (!updateNeeded)
                    return false;

                if (userValue == null)
                {
                    userValue = new PropertyUserValue()
                    {
                        Id = newValue.Id,
                        CompanyId = newValue.CompanyId,
                        CreatedAt = newValue.CreatedAt,
                        ModifiedAt = newValue.ModifiedAt,
                        PropertyId = newValue.PropertyId,
                        TaskId = newValue.TaskId,
                        TemplatePropertyId = newValue.TemplatePropertyId,
                        UserId = newValue.UserId,
                        ModifiedBy = newValue.ModifiedBy,
                    };

                    PropertyUserValues.Add(userValue);
                }

                userValue.UserValueBool = newValue.UserValueBool;
                userValue.UserValueDate = newValue.UserValueDate;
                userValue.UserValueDecimal = newValue.UserValueDecimal;
                userValue.UserValueInt = newValue.UserValueInt;
                userValue.UserValueString = newValue.UserValueString;
                userValue.UserValueTime = newValue.UserValueTime;

                property.UpdateUserValue(userValue);
                property.UpdateDisplayType();
                property.Validate();
                return updateNeeded;
            }

            return false;
        }

        [JsonIgnore]
        public IReadOnlyList<BasicTaskPropertyModel> PropertyList { get; private set; }

        public void RefreshPropertyValueString()
        {
            OnPropertyChanged(nameof(PropertyValuesString));
        }

        [JsonIgnore]
        public string PropertyValuesString
        {
            get
            {
                var result = "";
                if (PropertyList != null)
                {
                    foreach (var property in PropertyList)
                    {
                        var userValueString = property.GetUserValueString();
                        if (!string.IsNullOrEmpty(userValueString))
                        {
                            if (!string.IsNullOrEmpty(result))
                                result += "\n";

                            if (property.IsPlannedTimeProperty)
                            {
                                result += $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarked)} " +
                                    $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskRealizedTimeIn)} " +
                                    $"{TimeTaken.Value} {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskMinutesLabelText)} " +
                                    $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy)} " +
                                    $"{TimeRealizedBy}";
                                continue;
                            }

                            result += $"{property.DisplayTitleString}: {userValueString} {property.DisplayFooterString} " +
                                $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedOn)} " +
                                $"{FormatDateTime(Settings.ConvertDateTimeToLocal(property.UserValue?.ModifiedAt.ToLocalTime()))}";

                            if (!property?.UserValue?.ModifiedBy.IsNullOrEmpty() ?? false)
                            {
                                result += $" {TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedBy)} " +
                                $"{property?.UserValue.ModifiedBy}";
                            }
                        }
                    }
                }
                return result;
            }
        }

        //TODO Create Control
        public void CreatePropertyList()
        {
            var result = new List<BasicTaskPropertyModel>();
            int i = 0;
            if (PlannedTime != null)
            {
                result.Add(BasicTaskPropertyModel.FromPlannedTime(PlannedTime.Value, TaskMachineStatus, TimeTaken, i + 1));
                i++;
            }

            if (Properties.Any() && CompanyFeatures.TasksPropertyValueRegistrationEnabled)
            {
                result.AddRange(Properties.OrderBy(x => x.Index)
                                          .Select((template, index) => BasicTaskPropertyModel.FromTemplateAndValue(template,
                                                                        PropertyUserValues
                                                                        .Where(value => value.TemplatePropertyId == template.Id)
                                                                        .OrderByDescending(x => x.ModifiedAt)
                                                                        .FirstOrDefault(), index + i)));
            }

            PropertyList = result;
            HasFeatures = result?.Any() ?? false;
            RefreshPropertyValueString();
        }

        public void SetPictureProofMediaItems()
        {
            var result = new List<MediaItem>();
            if (CompanyFeatures.RequiredProof && (PictureProof?.Media.Any() ?? false))
            {
                result = PictureProof.Media.Select(x => MediaItem.OnlinePicture(x.UriPart)).ToList();
            }
            PictureProofMediaItems = result;
        }
        #endregion

        #region Validation
        public void ResetValidation()
        {
            if (PropertyList?.Count > 0)
            {
                foreach (var property in PropertyList)
                {
                    property.IsValid = true;
                }
            }
            IsDeepLinkValid = true;
        }

        public bool Validate()
        {
            bool arePropertiesValid = ValidateProperties();
            bool isDeepLinkValid = ValidateDeepLink();
            return arePropertiesValid && isDeepLinkValid;
        }

        private bool ValidateProperties()
        {
            bool isValid = true;
            if (PropertyList?.Count > 0)
            {
                foreach (var property in PropertyList)
                {
                    if (!property.Validate())
                        isValid = false;
                }
            }
            return isValid;
        }

        public bool ValidateDeepLink()
        {
            IsDeepLinkValid = true;
            if (!DeepLinkCompletionIsRequired ?? false)
                return true;

            IsDeepLinkValid = CompletedDeeplinkId.HasValue;
            return IsDeepLinkValid;
        }
        #endregion

        [JsonIgnore]
        public bool IsLocalMedia { get; set; }
    }
}
