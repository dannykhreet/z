using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Enumerations;
using EZGO.Maui.Core.Extensions;
using EZGO.Maui.Core.Models.Tasks.Properties;
using EZGO.Maui.Core.Utils;
using NodaTime;
using System;
using System.Linq;

namespace EZGO.Maui.Core.Models.Tasks
{
    public class BasicTaskPropertyModel : NotifyPropertyChanged
    {
        public const string DisplayDateFormat = "dd-MM-yy";
        public const string DisplayDateTimeFormat = "dd-MM-yy\r\nHH:mm";
        public const string DisplayTimeFormat = "HH:mm";
        public const string NoValueString = "-";

        /// <summary>
        /// Indicates if this property is a retrofit for planned time input
        /// </summary>
        public bool IsPlannedTimeProperty { get; private set; }

        public int Index { get; private set; }
        public int Id { get; private set; }
        public int TaskTemplateId { get; private set; }

        public string Description { get; private set; }
        public string ShortName { get; private set; }
        public string Name { get; private set; }

        public string DisplayTitleString => TitleDisplay ?? ShortName;
        public string DisplayFooterString => PropertyValueDisplay ?? Value?.ValueSymbol;

        private string _titleDisplay;
        public string TitleDisplay
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_titleDisplay))
                {
                    if (_titleDisplay.Length > 50)
                    {
                        _titleDisplay = _titleDisplay.Substring(0, 47) + "...";
                    }
                }
                return _titleDisplay;
            }
            private set
            {
                _titleDisplay = value;
            }
        }
        public string PropertyValueDisplay { get; private set; }
        public PropertyTypeEnum Type { get; private set; }
        public PropertyDisplayTypeEnum DisplayType { get; private set; }
        public string DefaultValueUnitTypeDisplay { get; private set; }
        public PropertyValueTypeEnum DisplayValueType { get; private set; }
        public PropertyFieldTypeEnum FieldType { get; private set; }
        public PropertyValueTypeEnum ValueType { get; private set; }

        public bool IsDate => ValueType == PropertyValueTypeEnum.DateTime || ValueType == PropertyValueTypeEnum.Date;

        public PropertyValue Value { get; private set; }

        public string PrimaryValue { get; set; }

        public DateTime? PrimaryDateTimeValue { get; set; }

        public DateTime? SecondaryDateTimeValue { get; set; }

        public int PrimaryIntValue { get; set; }
        public int SecondaryIntValue { get; set; }

        public decimal PrimaryDecimalValue { get; set; }
        public decimal SecondaryDecimalValue { get; set; }

        public void UpdatePrimaryDisplayValue()
        {
            PrimaryValueDisplay = GetUserValueString() ?? PrimaryValue;
        }

        private string _PrimaryValueDisplay;
        public string PrimaryValueDisplay
        {
            get => _PrimaryValueDisplay ?? NoValueString;
            set => _PrimaryValueDisplay = value;
        }

        public string SecondaryValue { get; private set; }
        public bool BoolValue { get; private set; }

        public int PropertyId { get; private set; }
        public int PropertyTemplateId { get; private set; }

        public string ModifiedBy { get; private set; }

        public LocalDateTime LocalModifiedAt
        {
            get
            {
                if (UserValue.ModifiedAt != null)
                    return Settings.ConvertDateTimeToLocal(UserValue.ModifiedAt.ToLocalTime());
                else return new LocalDateTime();
            }
        }

        public LocalDateTime? LocalRegisteredAt
        {
            get
            {
                if (UserValue.RegisteredAt != null)
                    return Settings.ConvertDateTimeToLocal(UserValue.RegisteredAt.Value.ToLocalTime());
                else return new LocalDateTime();
            }
        }

        public bool IsRequired { get; private set; }

        public bool IsValid { get; set; } = true;

        public PropertyUserValue UserValue { get; set; }

        public void UpdateUserValue(PropertyUserValue value)
        {
            UserValue = value;

            UpdatePrimaryDisplayValue();
        }

        public bool HasUserValue => UserValue != null;

        public bool ShowRange => FieldType == PropertyFieldTypeEnum.Range && !HasUserValue && !SecondaryValue.IsNullOrEmpty();

        public string GetUserValueString()
        {
            if (UserValue == null)
                return null;

            return ValueType switch
            {
                PropertyValueTypeEnum.Boolean => UserValue.UserValueBool?.ToString() ?? string.Empty,
                PropertyValueTypeEnum.Date => UserValue.UserValueDate?.ToString(DisplayDateFormat) ?? string.Empty,
                PropertyValueTypeEnum.DateTime => UserValue.UserValueDate?.ToString(DisplayDateTimeFormat) ?? string.Empty,
                PropertyValueTypeEnum.Time => GetTruncatedUserValue(TimeStringRemoveSeconds(UserValue.UserValueTime)) ?? string.Empty,
                PropertyValueTypeEnum.Decimal => GetTruncatedUserValue(UserValue.UserValueDecimal?.ToString("0.##")) ?? string.Empty,
                PropertyValueTypeEnum.Integer => GetTruncatedUserValue(UserValue.UserValueInt?.ToString()) ?? string.Empty,
                PropertyValueTypeEnum.String => UserValue.UserValueString ?? string.Empty,
                _ => null,
            };
        }

        private string GetTruncatedUserValue(string userValue)
        {
            const int MAX_LENGTH = 11;
            if (!userValue.IsNullOrEmpty() && userValue.Length > MAX_LENGTH)
                userValue = userValue.Substring(0, MAX_LENGTH) + "...";

            return userValue;
        }

        private string TimeStringRemoveSeconds(string value)
        {
            if (value == null)
                return null;

            // Even though, we post the time as HH:mm, API adds :ss part at the end of it
            if (value.Count(x => x == ':') > 1)
            {
                value = value.Substring(0, value.LastIndexOf(':'));
            }

            return value;
        }

        public void UpdateDisplayType()
        {
            if (HasUserValue && (ValueType == PropertyValueTypeEnum.Decimal || ValueType == PropertyValueTypeEnum.Integer) && !PrimaryValue.IsNullOrEmpty())
            {
                switch (ValueType)
                {
                    case PropertyValueTypeEnum.Decimal:
                        if (UserValue.UserValueDecimal.HasValue)
                        {
                            decimal decimalUserValue = UserValue.UserValueDecimal.Value;
                            UpdateProperties(() => decimalUserValue >= PrimaryDecimalValue && decimalUserValue <= SecondaryDecimalValue,
                                             () => decimalUserValue == PrimaryDecimalValue,
                                             () => decimalUserValue <= PrimaryDecimalValue,
                                             () => decimalUserValue >= PrimaryDecimalValue);
                        }
                        break;
                    case PropertyValueTypeEnum.Integer:
                        if (UserValue.UserValueInt.HasValue)
                        {
                            int integerUserValue = UserValue.UserValueInt.Value;
                            UpdateProperties(() => integerUserValue >= PrimaryIntValue && integerUserValue <= SecondaryIntValue,
                                             () => integerUserValue == PrimaryIntValue,
                                             () => integerUserValue <= PrimaryIntValue,
                                             () => integerUserValue >= PrimaryIntValue);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void UpdateProperties(Func<bool> isRange, Func<bool> isEqual, Func<bool> isLower, Func<bool> isUpper)
        {
            switch (FieldType)
            {
                case PropertyFieldTypeEnum.Range:
                    if (isRange())
                        DisplayType = PropertyDisplayTypeEnum.RectangularGreen;
                    else
                        DisplayType = PropertyDisplayTypeEnum.RectangularRed;
                    break;
                case PropertyFieldTypeEnum.LowerLimit:
                    if (isLower())
                        DisplayType = PropertyDisplayTypeEnum.RectangularRed;
                    else
                        DisplayType = PropertyDisplayTypeEnum.RectangularGreen;
                    break;
                case PropertyFieldTypeEnum.UpperLimit:
                    if (isUpper())
                        DisplayType = PropertyDisplayTypeEnum.RectangularRed;
                    else
                        DisplayType = PropertyDisplayTypeEnum.RectangularGreen;
                    break;
                default:
                    if (isEqual())
                        DisplayType = PropertyDisplayTypeEnum.RectangularGreen;
                    else
                        DisplayType = PropertyDisplayTypeEnum.RectangularRed;
                    break;
            }
        }

        public bool Validate()
        {
            if (!IsRequired)
                return true;

            var value = GetUserValueString();

            if (value.IsNullOrEmpty())
                IsValid = false;
            else
                IsValid = true;

            return IsValid;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        private BasicTaskPropertyModel()
        { }

        #region Factory Methods

        /// <summary>
        /// Creates a retrofit property for planned time
        /// </summary>
        /// <param name="plannedTime">Default planned time.</param>
        /// <param name="taskMachineStatus">Status of the machine.</param>
        /// <param name="plannedTimeUserValue">User entered realized time.</param>
        /// <param name="index">The index of the property in a list.</param>
        /// <returns>New <see cref="BasicTaskPropertyModel"/> based on the input parameters.</returns>
        public static BasicTaskPropertyModel FromPlannedTime(int plannedTime, MachineStatusEnum taskMachineStatus, int? plannedTimeUserValue, int index)
        {
            var template = new PropertyTaskTemplateModel
            {
                Id = -1,
                TaskTemplateId = -1,
                Property = new Property()
                {
                    ValueType = PropertyValueTypeEnum.Time,
                    Name = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskTimeLabelText),
                    ShortName = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskTimeLabelText),
                    Description = string.Empty,
                    Type = PropertyTypeEnum.Input,
                },
                PrimaryTimeValue = plannedTimeUserValue?.ToString() ?? plannedTime.ToString(),
                TitleDisplay = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskRealizedTimeTitle),
                PropertyValue = new PropertyValue()
                {
                    Name = TranslateExtension.GetValueFromDictionary(LanguageConstants.taskTimeLabelText),
                    ValueSymbol = "min",
                },
                Index = index,
                FieldType = PropertyFieldTypeEnum.Custom,
                ValueType = PropertyValueTypeEnum.Time,
            };

            template.DisplayType = taskMachineStatus switch
            {
                MachineStatusEnum.stopped => PropertyDisplayTypeEnum.RectangularRed,
                MachineStatusEnum.running => PropertyDisplayTypeEnum.RectangularGreen,
                _ => PropertyDisplayTypeEnum.RectangularGrey,
            };

            PropertyUserValue value = null;

            if (plannedTimeUserValue.HasValue)
            {
                value = new PropertyUserValue()
                {
                    Id = -1,
                    UserValueTime = plannedTimeUserValue.Value.ToString(),
                };
            }

            var model = FromTemplateAndValue(template, value, index);
            model.IsPlannedTimeProperty = true;

            return model;
        }

        /// <summary>
        /// Creates a new property model from a given template and value.
        /// </summary>
        /// <param name="template">The template to copy the parameters from.</param>
        /// <param name="value">The value to attach to this property. Can be <see langword="null"/>.</param>
        /// <param name="index">The index in the list of this property.</param>
        /// <returns>New <see cref="BasicTaskPropertyModel"/> based on the input parameters.</returns>
        public static BasicTaskPropertyModel FromTemplateAndValue(PropertyTaskTemplateModel template, PropertyUserValue value, int index)
        {
            BasicTaskPropertyModel result = new BasicTaskPropertyModel
            {
                Index = index,
                Id = template.Id,
                TaskTemplateId = template.TaskTemplateId,
                Name = TranslateExtension.GetValueFromDictionary($"{template.Property.ResourceKeyName}_NAME"),
                TitleDisplay = template.TitleDisplay,
                ShortName = TranslateExtension.GetValueFromDictionary($"{template.Property.ResourceKeyName}_SHORTNAME"),
                Description = template.Property.Description,
                PropertyValueDisplay = template.PropertyValueDisplay,
                DefaultValueUnitTypeDisplay = template.Property.DefaultValueUnitTypeDisplay,
                DisplayType = template.DisplayType ?? default,
                DisplayValueType = template.Property.DisplayValueType ?? default,
                FieldType = template.FieldType,
                Type = template.Property.Type,
                ValueType = template.ValueType ?? default,
                Value = template.PropertyValue,
                PrimaryIntValue = template.PrimaryIntValue ?? 0,
                PrimaryDecimalValue = template.PrimaryDecimalValue ?? 0,
                SecondaryIntValue = (template.FieldType == PropertyFieldTypeEnum.Range) ? template.SecondaryIntValue ?? 0 : 0,
                SecondaryDecimalValue = (template.FieldType == PropertyFieldTypeEnum.Range) ? template.SecondaryDecimalValue ?? 0 : 0,
                UserValue = value,
                PropertyId = template.PropertyId,
                PropertyTemplateId = template.Id,
                ModifiedBy = value?.ModifiedBy,
                IsRequired = template.IsRequired ?? false,
            };

            switch (result.ValueType)
            {
                case PropertyValueTypeEnum.Boolean:
                    result.BoolValue = template.BoolValue ?? false;
                    break;
                case PropertyValueTypeEnum.Date:
                    result.PrimaryValue = template.PrimaryDateTimeValue?.ToString(DisplayDateFormat);
                    result.PrimaryDateTimeValue = template.PrimaryDateTimeValue;
                    result.SecondaryValue = template.SecondaryDateTimeValue?.ToString(DisplayDateFormat);
                    result.SecondaryDateTimeValue = template.SecondaryDateTimeValue;
                    break;
                case PropertyValueTypeEnum.DateTime:
                    result.PrimaryValue = template.PrimaryDateTimeValue?.ToString(DisplayDateTimeFormat);
                    result.PrimaryDateTimeValue = template.PrimaryDateTimeValue;
                    result.SecondaryValue = template.SecondaryDateTimeValue?.ToString(DisplayDateFormat);
                    result.SecondaryDateTimeValue = template.SecondaryDateTimeValue;
                    break;
                case PropertyValueTypeEnum.Time:
                    result.PrimaryValue = template.PrimaryTimeValue;
                    result.SecondaryValue = template.SecondaryTimeValue;
                    break;
                case PropertyValueTypeEnum.Decimal:
                    result.PrimaryValue = template.PrimaryDecimalValue?.ToString("0.##");
                    result.SecondaryValue = template.SecondaryDecimalValue?.ToString("0.##");
                    break;
                case PropertyValueTypeEnum.Integer:
                    result.PrimaryValue = template.PrimaryIntValue.ToString();
                    result.SecondaryValue = template.SecondaryIntValue.ToString();
                    break;
                case PropertyValueTypeEnum.String:
                    result.PrimaryValue = template.PrimaryStringValue;
                    result.SecondaryValue = template.SecondaryStringValue;
                    break;
            }

            result.PrimaryValueDisplay = result.GetUserValueString() ?? result.PrimaryValue;

            result.UpdateDisplayType();
            return result;
        }

        #endregion
    }
}
