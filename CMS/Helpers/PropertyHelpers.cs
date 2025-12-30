using EZGO.Api.Models.PropertyValue;
using EZGO.CMS.LIB.Extensions;
using System;
using WebApp.Models.Properties;

namespace WebApp.Helpers
{
    public static class PropertyHelpers
    {
        const string good = "#1ed760";
        const string bad = "#ed5454";
        const string neutral = "rgb(100, 104, 107)"; /*"rgb(200 208 213)";*/ /*"rgba(154,168,179,.486)";*/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateProperty"></param>
        /// <param name="possiblePropertyValue"></param>
        /// <returns></returns>        
        public static string GetStatusColor(PropertyTaskTemplate templateProperty, PropertyUserValue possiblePropertyValue)
        {
            var templatePropertyModel = new TemplatePropertyModel()
            { 
                BoolValue = templateProperty.BoolValue,
                FieldType = (int)templateProperty.FieldType,
                Id = templateProperty.Id,
                Index = templateProperty.Index,
                IsRequired = templateProperty.IsRequired,
                PrimaryDateTimeValue = templateProperty.PrimaryDateTimeValue,
                PrimaryDecimalValue = templateProperty.PrimaryDecimalValue,
                PrimaryIntValue = templateProperty.PrimaryIntValue,
                PrimaryStringValue = templateProperty.PrimaryStringValue,
                PrimaryTimeValue = templateProperty.PrimaryTimeValue,
                Property = templateProperty.Property == null ? null : new PropertyModel()
                {
                    Description = templateProperty.Property.Description,
                    DisplayValueType = (int)(templateProperty.Property.DisplayValueType ?? 0),
                    FieldKindType = (int)(templateProperty.Property.FieldKindType ?? 0),
                    FieldType = (int)(templateProperty.Property.FieldType),
                    Id = templateProperty.Property.Id,
                    Name = templateProperty.Property.Name,
                    ShortName = templateProperty.Property.ShortName,
                    ValueType = (int)(templateProperty.Property.ValueType),
                },
                PropertyGroupId = templateProperty.PropertyGroupId,
                PropertyId = templateProperty.PropertyId,
                PropertyValue = templateProperty.PropertyValue == null ? null : new PropertyValueModel()
                {
                    CreatedAt = templateProperty.PropertyValue.CreatedAt,
                    DefaultValueType = templateProperty.PropertyValue.DefaultValueType,
                    Description = templateProperty.PropertyValue.Description,
                    Id = templateProperty.PropertyValue.Id,
                    ModifiedAt = templateProperty.PropertyValue.ModifiedAt,
                    Name = templateProperty.PropertyValue.Name,
                    PropertyValueKindId = templateProperty.PropertyValue.PropertyValueKindId,
                    ResourceKeyname = templateProperty.PropertyValue.ResourceKeyName,
                    ValueAbbreviation = templateProperty.PropertyValue.ValueAbbreviation,
                    ValueSymbol = templateProperty.PropertyValue.ValueSymbol
                },
                PropertyValueDisplay = templateProperty.PropertyValueDisplay,
                PropertyValueId = templateProperty.PropertyValueId,
                SecondaryDateTimeValue = templateProperty.SecondaryDateTimeValue,
                SecondaryDecimalValue = templateProperty.SecondaryDecimalValue,
                SecondaryIntValue = templateProperty.SecondaryIntValue,
                SecondaryStringValue = templateProperty.SecondaryStringValue,
                SecondaryTimeValue = templateProperty.SecondaryTimeValue,
                TaskTemplateId = templateProperty.TaskTemplateId,
                TitleDisplay = templateProperty.TitleDisplay,
                UnitKindId = templateProperty.UnitKindId ?? 0,
                ValueType = (int?)templateProperty.ValueType ,

                AuditTemplateId = 0,
                ChecklistTemplateId = 0,
                isNew = false
            };

            var propertyUserValueModel = new PropertyUserValueModel()
            { 
                CompanyId = possiblePropertyValue.CompanyId,
                CreatedAt = possiblePropertyValue.CreatedAt,
                Id = possiblePropertyValue.Id,
                ModifiedAt = possiblePropertyValue.ModifiedAt,
                PropertyId = possiblePropertyValue.PropertyId,
                TaskId = possiblePropertyValue.TaskId ?? 0,
                TemplatePropertyId = possiblePropertyValue.TemplatePropertyId,
                UserBoolValue = possiblePropertyValue.UserValueBool,
                UserId = possiblePropertyValue.UserId,
                UserValueDate = possiblePropertyValue.UserValueDate,
                UserValueDecimal = possiblePropertyValue.UserValueDecimal,
                UserValueInt = possiblePropertyValue.UserValueInt,
                UserValueString = possiblePropertyValue.UserValueString,
                UserValueTime = possiblePropertyValue.UserValueTime
            };

            return GetStatusColor(templatePropertyModel, propertyUserValueModel);
        }

        /// <summary>
        /// Get the color for a property status based on the user input in relation to the tamplate values (e.g. Is the user input within range of the template values? yes = green, no = red)
        /// </summary>
        /// <param name="templateProperty">A TemplatePropertyModel of the property</param>
        /// <param name="possibePropertyValue">A PropertyUserValueModel that contains the user input values for the property</param>
        /// <returns>Color corresponding to the status of the user input for this property</returns>
        public static string GetStatusColor(TemplatePropertyModel templateProperty, PropertyUserValueModel possibePropertyValue)
        {
            string statusColor = neutral;

            if (possibePropertyValue.UserBoolValue.HasValue)
            {
                bool primaryValue = templateProperty.BoolValue ?? new bool();
                bool secondaryValue = new bool();
                statusColor = GetStatusColorForFieldType(possibePropertyValue.UserBoolValue.Value, primaryValue, secondaryValue, templateProperty.FieldType, templateProperty.BoolValue != null);
            }
            else if (possibePropertyValue.UserValueDate.HasValue)
            {
                DateTime primaryValue = templateProperty.PrimaryDateTimeValue ?? new DateTime();
                DateTime secondaryValue = templateProperty.SecondaryDateTimeValue ?? new DateTime();
                statusColor = GetStatusColorForFieldType(possibePropertyValue.UserValueDate.Value, primaryValue, secondaryValue, templateProperty.FieldType, templateProperty.PrimaryDateTimeValue != null, templateProperty.SecondaryDateTimeValue != null);
            }
            else if (possibePropertyValue.UserValueDecimal.HasValue)
            {
                decimal primaryValue = templateProperty.PrimaryDecimalValue ?? new decimal();
                decimal secondaryValue = templateProperty.SecondaryDecimalValue ?? new decimal();
                statusColor = GetStatusColorForFieldType(possibePropertyValue.UserValueDecimal.Value, primaryValue, secondaryValue, templateProperty.FieldType, templateProperty.PrimaryDecimalValue != null, templateProperty.SecondaryDecimalValue != null);
            }
            else if (possibePropertyValue.UserValueInt.HasValue)
            {
                int primaryValue = templateProperty.PrimaryIntValue ?? new int();
                int secondaryValue = templateProperty.SecondaryIntValue ?? new int();
                statusColor = GetStatusColorForFieldType(possibePropertyValue.UserValueInt.Value, primaryValue, secondaryValue, templateProperty.FieldType, templateProperty.PrimaryIntValue != null, templateProperty.SecondaryIntValue != null);
            }
            else if (!possibePropertyValue.UserValueString.IsNullOrEmpty())
            {
                statusColor = neutral;
            }
            else if (!possibePropertyValue.UserValueTime.IsNullOrEmpty())
            {
                bool primaryValid = false;
                bool secondaryValid = false;
                if (!TimeSpan.TryParse(templateProperty.PrimaryTimeValue, out TimeSpan primaryValue))
                {
                    return neutral;
                }
                else
                {
                    primaryValid = true;
                }
                if (!TimeSpan.TryParse(templateProperty.SecondaryTimeValue, out TimeSpan secondaryValue))
                {
                    secondaryValue = new TimeSpan();
                }
                else
                {
                    secondaryValid = true;
                }
                if (TimeSpan.TryParse(possibePropertyValue.UserValueTime, out TimeSpan userValue))
                    statusColor = GetStatusColorForFieldType(userValue, primaryValue, secondaryValue, templateProperty.FieldType, primaryValid, secondaryValid);
            }

            return statusColor;
        }

        /// <summary>
        /// Get the status color of a property based on the field type
        /// </summary>
        /// <typeparam name="T">The type of the property variable</typeparam>
        /// <param name="userValue">Value the user entered</param>
        /// <param name="primaryTemplateValue">Primary value from the property template</param>
        /// <param name="secondaryTemplateValue">Secondary value from the property template</param>
        /// <param name="fieldType">Field type of the property</param>
        /// <returns>Color to be used in css</returns>
        private static string GetStatusColorForFieldType<T>(T userValue, T primaryTemplateValue, T secondaryTemplateValue, int fieldType, bool hasDefaultPrimaryValue = false, bool hasDefaultSecondaryValue = false)
            where T : IComparable<T>
        {
            bool isNeutral = false;
            bool inRange = false;

            switch (fieldType)
            {
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.Custom):
                    isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.SingleValue):
                    if (hasDefaultPrimaryValue)
                        inRange = EqualTo(userValue, primaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.Range):
                    if(hasDefaultPrimaryValue && hasDefaultSecondaryValue)
                        inRange = InRange(userValue, primaryTemplateValue, secondaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.UpperLimit):
                    if (hasDefaultPrimaryValue)
                        inRange = LessThan(userValue, primaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.LowerLimit):
                    if (hasDefaultPrimaryValue)
                        inRange = GreaterThan(userValue, primaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.EqualTo):
                    if (hasDefaultPrimaryValue)
                        inRange = EqualTo(userValue, primaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.UpperLimitEqualTo):
                    if (hasDefaultPrimaryValue)
                        inRange = LessThanOrEqualTo(userValue, primaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
                case ((int)EZGO.Api.Models.Enumerations.PropertyFieldTypeEnum.LowerLimitEqualTo):
                    if (hasDefaultPrimaryValue)
                        inRange = GreaterThanOrEqualTo(userValue, primaryTemplateValue);
                    else
                        isNeutral = true;
                    break;
            }
            if (isNeutral) { return neutral; }
            return inRange ? good : bad;
        }

        private static bool GreaterThan<T>(T lhs, T rhs)
            where T : IComparable<T>
        {
            return lhs.CompareTo(rhs) > 0;
        }

        private static bool LessThan<T>(T lhs, T rhs)
            where T : IComparable<T>
        {
            return lhs.CompareTo(rhs) < 0;
        }

        private static bool EqualTo<T>(T lhs, T rhs)
                where T : IComparable<T>
        {
            return lhs.CompareTo(rhs) == 0;
        }

        private static bool InRange<T>(T input, T lowerLimit, T upperLimit)
                where T : IComparable<T>
        {
            return input.CompareTo(lowerLimit) >= 0 && input.CompareTo(upperLimit) <= 0;
        }

        private static bool GreaterThanOrEqualTo<T>(T lhs, T rhs)
            where T : IComparable<T>
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        private static bool LessThanOrEqualTo<T>(T lhs, T rhs)
            where T : IComparable<T>
        {
            return lhs.CompareTo(rhs) <= 0;
        }
    }
}
