using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Actions;
using EZGO.Maui.Core.Models.Audits;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Instructions;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using Newtonsoft.Json;
using NodaTime;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using System.Text;
using System.ComponentModel;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class BasicTaskTemplateModel : NotifyPropertyChanged, IItemFilter<TaskStatusEnum>, ITaskTemplateModel, IDetailItem
    {
        public int Id { get; set; }

        public long ItemId { get; set; }

        public int? ChecklistId { get; set; }

        public int? ChecklistTemplateId { get; set; }

        public int? AuditId { get; set; }

        public int? AuditTemplateId { get; set; }

        public string Name { get; set; }

        public bool HasDescriptionFile => !string.IsNullOrWhiteSpace(DescriptionFile);

        public bool HasExtraInformation => HasWorkInstructions || HasSteps || HasAttachments;

        public string Description { get; set; }

        public string Comment { get; set; }

        public bool HasComment { get; set; }

        public bool HasMediaItems => !Picture.IsNullOrEmpty() || !Video.IsNullOrEmpty();

        public string Picture { get; set; }

        public string DisplayPicture { get => !HasVideo ? Picture : VideoThumbnail; }

        public TaskStatusEnum FilterStatus { get; set; }

        public MachineStatusEnum MachineStatus { get; set; }

        public bool IsSignButton { get; set; }

        public int? PlannedTime { get; set; }

        public int StepsCount { get; set; }

        public bool HasSteps => StepsCount > 0;

        public bool HasWorkInstructions => WorkInstructionRelations?.Any() ?? false;

        public bool HasStepsOrWorkInstructions => HasSteps || HasWorkInstructions;

        public string Video { get; set; }

        public string VideoThumbnail { get; set; }

        public bool HasVideo => !Video.IsNullOrWhiteSpace();

        public ImageSource MediaSource { get; set; }

        public string DescriptionFile { get; set; }

        public bool HasDocument => !string.IsNullOrWhiteSpace(DescriptionFile);

        public List<StepModel> Steps { get; set; }

        public decimal Weight { get; set; }

        public int? Score { get; set; }

        public ScoreModel NewScore { get; set; }

        public int OpenActionCount { get; set; }

        public int CommentsCount => LocalComments?.Count ?? 0;

        public int ActionBubbleCount { get; set; }

        public string RecurrencyType { get; set; }

        public bool IsBusyLoading { get; set; }

        public List<CommentModel> LocalComments { get; set; }

        public List<ActionsModel> LocalActions { get; set; }

        public List<InstructionsModel> WorkInstructionRelations { get; set; }

        public List<MediaItem> PictureProofMediaItems { get; set; }

        public bool IsThumbsUpBadgeVisible => HasPictureProof && FilterStatus == TaskStatusEnum.Ok;
        public bool IsThumbsDownBadgeVisible => HasPictureProof && FilterStatus == TaskStatusEnum.NotOk;
        public bool IsScoreBadgeVisible => HasPictureProof && (PictureProofMediaItems?.Any() ?? false) && Score.HasValue;

        public PictureProof PictureProof { get; set; }

        public List<Tag> Tags { get; set; }

        public List<Attachment> Attachments { get; set; }

        public bool HasAttachments => Attachments?.Count > 0;

        private SignatureModel signature;
        public SignatureModel Signature
        {
            get => signature;
            set
            {
                signature = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTaskMarked));
                OnPropertyChanged(nameof(TaskMarkedString));
            }
        }
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

        public Stream PDFStream { get; set; } = null;

        #region Properties & features

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

                            string footer = property.Value != null ? property.Value.Name : property.DisplayFooterString;

                            var dateToUse = property.UserValue?.RegisteredAt ?? property.UserValue?.ModifiedAt;
                            string formattedDate = FormatDateTime(Settings.ConvertDateTimeToLocal(dateToUse?.ToLocalTime() ?? DateTime.Now));

                            result += $"{property.DisplayTitleString}: {userValueString} {footer} " +
                                $"{TranslateExtension.GetValueFromDictionary(LanguageConstants.taskDetailMarkedOn)} {formattedDate}";

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

        public void UpdateActionBubbleCount()
        {
            ActionBubbleCount = OpenActionCount + CommentsCount;
        }

        public bool IsSelected { get; set; }

        [JsonIgnore]
        public bool HasFeatures { get; private set; }

        public List<PropertyTaskTemplateModel> Properties { get; set; } = new List<PropertyTaskTemplateModel>();
        public List<PropertyUserValue> PropertyValues { get; set; } = new List<PropertyUserValue>();

        [JsonIgnore]
        public List<PropertyUserValue> ModifiedPropertyValues { get; private set; } = new List<PropertyUserValue>();

        public IReadOnlyList<BasicTaskPropertyModel> PropertyList { get; private set; }

        public bool IsPropertyButton { get; set; }

        public bool HasPictureProof { get; set; }

        public MachineStatusEnum TaskMachineStatus => MachineStatusEnum.not_applicable;

        public int? DeepLinkId { get; set; } = null;

        public string CommentString { get; }

        public bool IsTaskMarked => Signature != null;

        public string DueDateString { get; }

        public string OverDueString { get; }

        public bool ShowPlannedTimeButton => false;

        public int? StageTemplateId { get; set; }

        public bool IsStageLocked { get; set; }

        //TODO Create Control
        public void CreatePropertyList()
        {
            var result = new List<BasicTaskPropertyModel>();
            int i = 0;
            if (PlannedTime != null)
            {
                result.Add(BasicTaskPropertyModel.FromPlannedTime(PlannedTime.Value, MachineStatus, null, i + 1));
                i++;
            }

            if (CompanyFeatures.CompanyFeatSettings.TasksPropertyValueRegistrationEnabled && Properties != null && Properties.Any())
            {
                result.AddRange(Properties.OrderBy(x => x.Index)
                                            .Select((template, index) =>
                    BasicTaskPropertyModel.FromTemplateAndValue(template, PropertyValues?.Find(value => value.TemplatePropertyId == template.Id), index + i)));
            }

            PropertyList = result;
            HasFeatures = result?.Any() ?? false;
            RefreshPropertyValueString();
        }

        public bool AddOrUpdateUserProperty(PropertyUserValue newValue)
        {
            var property = PropertyList.Where(x => x.Id == newValue.TemplatePropertyId).FirstOrDefault();

            if (property != null)
            {
                var userValue = PropertyValues.FirstOrDefault(x => x.Id == newValue.Id);
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

                    PropertyValues.Add(userValue);
                }

                userValue.UserValueBool = newValue.UserValueBool;
                userValue.UserValueDate = newValue.UserValueDate;
                userValue.UserValueDecimal = newValue.UserValueDecimal;
                userValue.UserValueInt = newValue.UserValueInt;
                userValue.UserValueString = newValue.UserValueString;
                userValue.UserValueTime = newValue.UserValueTime;
                userValue.ModifiedBy = newValue.ModifiedBy;
                userValue.ModifiedAt = newValue.ModifiedAt;


                property.UpdateUserValue(userValue);
                property.UpdateDisplayType();
                property.Validate();
                return updateNeeded;
            }

            return false;
        }


        public void AddModifiedProperty(PropertyUserValue propertyUserValue)
        {
            var isModifiedPropertyInList = ModifiedPropertyValues.Contains(propertyUserValue);
            if (!isModifiedPropertyInList)
                ModifiedPropertyValues.Add(propertyUserValue);
        }

        #endregion

        private static string FormatDateTime(LocalDateTime? date)
        {
            string result = string.Empty;

            if (date.HasValue)
                result = $"{date.Value.ToString("d MMM yyyy", CultureInfo.CurrentUICulture)}, {date.Value.ToString(CultureInfo.InvariantCulture.DateTimeFormat.ShortTimePattern, CultureInfo.CurrentUICulture)}";

            return result;
        }

        public void SetPictureProofMediaItems()
        {
            var result = new List<MediaItem>();
            if (CompanyFeatures.CompanyFeatSettings.RequiredProof && (PictureProof?.Media.Any() ?? false))
            {
                result = PictureProof.Media.Select(x => MediaItem.OnlinePicture(x.UriPart)).ToList();
            }
            PictureProofMediaItems ??= result;
            HasPictureProof = PictureProofMediaItems.Any();
        }

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
        }

        public bool Validate()
        {
            return ValidateProperties();
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
        #endregion

        [JsonIgnore]
        public bool IsLocalMedia { get; set; }


        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsDeepLinkValid { get; set; } = false;
        [JsonIgnore]
        public bool? DeepLinkCompletionIsRequired { get; set; } = false;
        [JsonIgnore]
        public int? CompletedDeeplinkId { get; set; }
    }
}
