using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Drawing;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;

namespace EZGO.Api.Utils.Converters
{
    public static class PropertiesConverter
    {
        public static List<PropertyDTO> ToPropertyDTOList(this IEnumerable<BasePropertyObject> propertyTemplates, List<PropertyUserValue> propertyUserValues = null, List<UserBasic> userList = null)
        {
            List<PropertyDTO> propertyDTOList = new();
            foreach (BasePropertyObject prop in propertyTemplates)
            {
                PropertyUserValue userValue = null;
                if(propertyUserValues != null)
                {
                    userValue = propertyUserValues.FirstOrDefault(uv => uv.TemplatePropertyId == prop.Id);
                }                    
                PropertyDTO propertyGen4 = prop.ToPropertyDTO(userValue);
                
                if(propertyGen4.UserValue != null && userList != null)
                {
                    propertyGen4.UserValue.User = userList.FirstOrDefault(u => u.Id == userValue.UserId);
                }
                propertyDTOList.Add(propertyGen4);
            }
            return propertyDTOList;
        }

        private static PropertyDTO ToPropertyDTO(this BasePropertyObject propertyTemplate, PropertyUserValue propertyUserValue = null)
        {
            if ((!propertyTemplate.ValueType.HasValue))
            {
                return null;
            }
            
            PropertyValue propertyValue = propertyTemplate.Property?.PropertyValueKind?.PropertyValues?.FirstOrDefault(pv => pv.Id == propertyTemplate.PropertyValueId);
            string primaryValue = null;
            string secondaryValue = null;
            //convert primary and secondary values to strings
            switch (propertyTemplate.ValueType)
            {
                case (PropertyValueTypeEnum.Integer):
                    primaryValue = propertyTemplate.PrimaryIntValue?.ToString();
                    secondaryValue = propertyTemplate.SecondaryIntValue?.ToString();
                    break;
                case (PropertyValueTypeEnum.Decimal):
                    primaryValue = propertyTemplate.PrimaryDecimalValue?.ToString();
                    secondaryValue = propertyTemplate.SecondaryDecimalValue?.ToString();
                    break;
                case (PropertyValueTypeEnum.String):
                    primaryValue = propertyTemplate.PrimaryStringValue;
                    secondaryValue = propertyTemplate.SecondaryStringValue?.ToString();
                    break;
                case PropertyValueTypeEnum.Date:
                case PropertyValueTypeEnum.DateTime:
                    // preserve the same clock value but mark as UTC so "o" emits 'Z'
                    primaryValue = propertyTemplate.PrimaryDateTimeValue.HasValue
                                   ? DateTime.SpecifyKind(propertyTemplate.PrimaryDateTimeValue.Value, DateTimeKind.Utc).ToString("o")
                                   : null;
                    secondaryValue = propertyTemplate.SecondaryDateTimeValue.HasValue
                                   ? DateTime.SpecifyKind(propertyTemplate.SecondaryDateTimeValue.Value, DateTimeKind.Utc).ToString("o")
                                   : null;
                    break;
                case (PropertyValueTypeEnum.Time):
                    primaryValue = propertyTemplate.PrimaryDateTimeValue?.ToString();
                    secondaryValue = propertyTemplate.SecondaryTimeValue?.ToString();
                    break;
                case (PropertyValueTypeEnum.Boolean):
                    primaryValue = propertyTemplate.BoolValue?.ToString();
                    break;
            }

            string defaultValue = null;
            string lowerLimit = null;
            string upperLimit = null;
            string targetValue = null;
            //set the default, upper limit, lower limit and target values based on the field type
            defaultValue = primaryValue;
            if (propertyTemplate.FieldType == PropertyFieldTypeEnum.LowerLimit || propertyTemplate.FieldType == PropertyFieldTypeEnum.LowerLimitEqualTo)
            {
                lowerLimit = primaryValue;
            }
            else if (propertyTemplate.FieldType == PropertyFieldTypeEnum.UpperLimit || propertyTemplate.FieldType == PropertyFieldTypeEnum.UpperLimitEqualTo)
            {
                upperLimit = primaryValue;
            }
            else if (propertyTemplate.FieldType == PropertyFieldTypeEnum.Range)
            {
                lowerLimit = primaryValue;
                upperLimit = secondaryValue;
            }
            else if (propertyTemplate.FieldType == PropertyFieldTypeEnum.EqualTo)
            {
                targetValue = primaryValue;
            }

            PropertyTemplateDTO propertyTemplateDTO = new()
            {
                Id = propertyTemplate.Id,
                PropertyId = propertyTemplate.PropertyId,
                Name = propertyTemplate.TitleDisplay ?? propertyTemplate.Property?.ShortName,
                Unit = propertyValue?.Name,
                UnitSymbol = propertyValue?.UnitSymbol ??
                             propertyValue?.UnitAbbreviation ??
                             propertyValue?.ValueSymbol ??
                             propertyValue?.ValueAbbreviation,
                Footer = propertyTemplate.PropertyValueDisplay ?? propertyValue?.Name,
                ValueType = propertyTemplate.ValueType.Value,
                DefaultValue = defaultValue,
                IsMandatory = propertyTemplate.IsRequired ?? false,
                TargetValue = targetValue,
                UpperValueLimit = upperLimit,
                LowerValueLimit = lowerLimit,
                FieldType = propertyTemplate.FieldType, 
                Index = propertyTemplate.Index
            };

            
            PropertyUserValueDTO userValueDTO = null;
            //set uservalue if it exists
            if (propertyUserValue != null)
            {
                string userValue =
                    propertyUserValue.UserValueTime
                    ?? propertyUserValue.UserValueDate?.ToString("o")
                    ?? propertyUserValue.UserValueString
                    ?? propertyUserValue.UserValueBool?.ToString()
                    ?? propertyUserValue.UserValueDecimal?.ToString()
                    ?? propertyUserValue.UserValueInt?.ToString();

                userValueDTO = new()
                {
                    Id = propertyUserValue.Id,
                    UserValue = userValue,
                    ModifiedAt = propertyUserValue.ModifiedAt,
                    RegisteredAt = propertyUserValue.RegisteredAt,
                    User = new()
                    {
                        Id = propertyUserValue.UserId,
                        Name = propertyUserValue.ModifiedBy
                    },
                    TaskId = propertyUserValue.TaskId,
                    AuditId = propertyUserValue.AuditId,
                    ChecklistId = propertyUserValue.ChecklistId

                };
            }

            PropertyDTO propertyDTO = new()
            {
                PropertyTemplate = propertyTemplateDTO,
                UserValue = userValueDTO
            };

            return propertyDTO;
        }
    }
}
