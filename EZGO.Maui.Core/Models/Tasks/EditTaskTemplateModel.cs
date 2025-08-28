using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Steps;
using EZGO.Maui.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Models.Tasks
{
    /// <summary>
    /// View-model for creating and editing <see cref="TaskTemplateModel"/>
    /// </summary>
    public class EditTaskTemplateModel : NotifyPropertyChanged
    {
        #region Public Properties

        /// <summary>
        /// Indicates if the editing is enabled 
        /// </summary>
        public bool IsEditingEnabled { get; set; }

        /// <summary>
        /// Indicates if this template is brand new 
        /// </summary>
        /// <value>
        /// <para><see langword="true"/> if the underlaying template is a new object.</para>
        /// <para><see langword="false"/> if the underlaying template already exists in the app.</para>
        /// </value>
        public bool IsNewTemplate { get; set; }

        /// <summary>
        /// Name of the template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Users assigned to this task
        /// </summary>
        public RoleTypeEnum Role { get; set; }

        /// <summary>
        /// Task occurrence information
        /// </summary>
        public EditTaskRecurrencyModel Recurrency { get; set; }
        
        /// <summary>
        /// Task description in form of steps
        /// </summary>
        public List<StepModel> Steps { get; set; }

        public FileItem OriginalInstructionsFile { get; private set; }

        /// <summary>
        /// Represents a file that is attached as an instruction for the task
        /// </summary>
        public FileItem InstructionsFile { get; set; }

        /// <summary>
        /// The media item that represents the image/video of this template
        /// </summary>
        public MediaItem MediaItem { get; set; }

        #endregion

        #region Private Members

        /// <summary>
        /// The underlying task template object.
        /// <para>When creating a new template this object is <see langword="null"/></para>
        /// </summary>
        private TaskTemplateModel Template;

        #endregion

        #region Commands

        /// <summary>
        /// Enables editing on all the editor pages
        /// </summary>
        public ICommand EnableEditingCommand => new Command(() => IsEditingEnabled = true);

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        private EditTaskTemplateModel()
        { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates the model state.
        /// </summary>
        /// <returns>The list of errors that were found. If no errors are present an empty list is returned.</returns>
        public List<string> ValidateRecurrency()
        {
            var greaterThan = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationGreaterThan0);
            var errors = new List<string>();
            switch (Recurrency.RecurrencyType)
            {
                case RecurrencyTypeEnum.NoRecurrency:
                    if (!Recurrency.Shifts.Any())
                        errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationNoShifts));
                    break;
                case RecurrencyTypeEnum.Shifts:
                    if (Recurrency.Shifts.Any() == false)
                        errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationNoShifts));
                    break;

                case RecurrencyTypeEnum.Week:
                    if (Recurrency.Schedule.Week == null || Recurrency.Schedule.Week < 1)
                        errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationWeek));
                    if (Recurrency.Schedule.Weekday0.GetValueOrDefault() == false &&
                        Recurrency.Schedule.Weekday1.GetValueOrDefault() == false &&
                        Recurrency.Schedule.Weekday2.GetValueOrDefault() == false &&
                        Recurrency.Schedule.Weekday3.GetValueOrDefault() == false &&
                        Recurrency.Schedule.Weekday4.GetValueOrDefault() == false &&
                        Recurrency.Schedule.Weekday5.GetValueOrDefault() == false &&
                        Recurrency.Schedule.Weekday6.GetValueOrDefault() == false)
                        errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationNoWeekDay));
                    break;

                case RecurrencyTypeEnum.Month:
                    if (Recurrency.Schedule.MonthRecurrencyType.IsNullOrEmpty())
                        errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationNoRecurrency));
                    else
                    {
                        if(Recurrency.Schedule.MonthRecurrencyType == "day_of_month")
                        {
                            if (Recurrency.Schedule.Week == null || Recurrency.Schedule.Week < 1)
                                errors.Add(greaterThan.Format(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationDay)));
                        }
                        else if (Recurrency.Schedule.MonthRecurrencyType == "weekday")
                        {
                            if (Recurrency.Schedule.WeekDayNumber == null)
                                errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationDayOfWeek));

                            if (Recurrency.Schedule.WeekDay == null)
                                errors.Add(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationWeekDaySet));
                        }

                        if (Recurrency.Schedule.Month == null || Recurrency.Schedule.Month < 1)
                            errors.Add(greaterThan.Format(TranslateExtension.GetValueFromDictionary(LanguageConstants.taskConstructorValidationMonth)));
                    }
                    break;
            }

            return errors;
        }

        public TaskTemplateModel GetUpdatedObject()
        {
            TaskTemplateModel template;
            if (Template == null)
                template = new TaskTemplateModel();
            else
                template = Template;

            template.Name = Name;
            template.Description = Description;
            template.Role = Role == RoleTypeEnum.ShiftLeader ? "shift_leader" : Role.ToString().ToLower();
            template.Recurrency = Recurrency.GetUpdatedObject();
            template.RecurrencyType = template.Recurrency.RecurrencyType;
            template.AreaId = template.Recurrency.AreaId;
            template.Steps = Steps;

            if (MediaItem != null)
            {
                if (MediaItem.IsVideo)
                {
                    template.Picture = null;
                    template.Video = MediaItem.VideoUrl;
                    template.VideoThumbnail = MediaItem.PictureUrl;
                }
                else
                {
                    template.Picture = MediaItem.PictureUrl;
                    template.Video = null;
                    template.VideoThumbnail = null;
                }
            }

            if (InstructionsFile == null || InstructionsFile.IsEmpty)
            {
                template.DescriptionFile = null;
            }
            else
            {
                template.Description = InstructionsFile.Url;
            }

            return template;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Creates a new instance of the view-model that is set up for creating a new template
        /// </summary>
        /// <returns>A new instance of the view-model</returns>
        public static EditTaskTemplateModel New()
        {
            return new EditTaskTemplateModel()
            {
                Recurrency = new EditTaskRecurrencyModel(),
                Steps = new List<StepModel>(),
                IsNewTemplate = true,

                // When creating a new template editing is always enabled
                IsEditingEnabled = true,
            };
        }

        /// <summary>
        /// Creates a new instance of the view-model that is set up for editing an existing template
        /// </summary>
        /// <param name="template">The template to edit</param>
        /// <returns>A new instance of the view-model</returns>
        public static EditTaskTemplateModel FromExisting(TaskTemplateModel template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            return new EditTaskTemplateModel()
            {
                Template = template,
                Name = template.Name,
                Description = template.Description,
                Role = (RoleTypeEnum)Enum.Parse(typeof(RoleTypeEnum), template.Role.Replace("_", ""), true),
                MediaItem = GetMediaItem(template),
                Recurrency = new EditTaskRecurrencyModel(template.Recurrency),
                Steps = template.Steps?.Select(x => x.Clone()).ToList() ?? new List<StepModel>(),
                InstructionsFile = string.IsNullOrEmpty(template.DescriptionFile) ? null : FileItem.FromOnlineFile(template.DescriptionFile),
                OriginalInstructionsFile = string.IsNullOrEmpty(template.DescriptionFile) ? null : FileItem.FromOnlineFile(template.DescriptionFile),

                IsNewTemplate = false,
                // By default when opening in editing mode disable editing until the user enables it
                IsEditingEnabled = false,
            };
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Creates a media item from a template data.
        /// </summary>
        /// <param name="template">The template to create media item from</param>
        /// <returns>
        /// A new <see cref="MediaItem"/> containing a video or a picture from the supplied template.
        /// <para>If the template has neither a picture or a video returns <see langword="null"/>.</para>
        /// </returns>
        private static MediaItem GetMediaItem(TaskTemplateModel template)
        {
            if (!string.IsNullOrEmpty(template.Picture))
                return MediaItem.OnlinePicture(template.Picture);

            if (!string.IsNullOrEmpty(template.Video))
                return MediaItem.OnlineVideo(template.Video, template.VideoThumbnail);

            return null;
        }

        #endregion
    }
}
