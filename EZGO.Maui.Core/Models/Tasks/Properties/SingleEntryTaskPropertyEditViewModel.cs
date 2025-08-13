using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Extensions;
using System;

namespace EZGO.Maui.Core.Models.Tasks.Properties
{
    /// <summary>
    /// Implementation of the <see cref="BaseTaskPropertyEditViewModel"/> that handles single data entry for types of
    /// <see cref="PropertyValueTypeEnum.String"/>, <see cref="PropertyValueTypeEnum.Integer"/>, <see cref="PropertyValueTypeEnum.Decimal"/> and <see cref="PropertyValueTypeEnum.Date"/>.
    /// </summary>
    public class SingleEntryTaskPropertyEditViewModel : BaseTaskPropertyEditViewModel
    {
        #region Private Memebers

        /// <summary>
        /// Entry in correct type. E.g. when the type should be <see cref="int"/> the SfNumericTextBox returns <see cref="decimal"/> value,
        /// this variable will hold the entry in type corresponding to the underlying property settings.
        /// </summary>
        private object EntryInCorrectType;

        #endregion

        #region Public Properties

        /// <summary>
        /// User input entry
        /// </summary>
        public object Entry { get; set; }

        /// <summary>
        /// Secondary user input entry. Used by timer picker since you can't pick date and time at the same time
        /// </summary>
        public TimeSpan TimeEntry { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="property"><inheritdoc/></param>
        public SingleEntryTaskPropertyEditViewModel(BasicTaskPropertyModel property) : base(property)
        {
            object objectValue = null;
            TimeSpan objectValueSecondary = TimeSpan.Zero;
            switch (property.ValueType)
            {
                case PropertyValueTypeEnum.String:
                    objectValue = HasValue ? Value.UserValueString : property.PrimaryValue;
                    break;

                case PropertyValueTypeEnum.Integer:
                    if (HasValue)
                    {
                        objectValue = Value.UserValueInt;
                    }
                    else
                    {
                        if (int.TryParse(property.PrimaryValue, out var valueInt))
                            objectValue = valueInt;
                        else
                            objectValue = default(int);
                    }
                    break;
                case PropertyValueTypeEnum.Decimal:
                    if (HasValue)
                    {
                        objectValue = Value.UserValueDecimal;
                    }
                    else
                    {
                        if (decimal.TryParse(property.PrimaryValue, out var valueDecimal))
                            objectValue = valueDecimal;
                        else
                            objectValue = default(decimal);
                    }

                    break;
                case PropertyValueTypeEnum.Date:
                case PropertyValueTypeEnum.DateTime:
                    if (HasValue)
                    {
                        objectValue = Value.UserValueDate;
                        objectValueSecondary = Value.UserValueDate?.TimeOfDay ?? DateTime.Now.TimeOfDay;
                    }
                    else
                    {
                        objectValue = property.PrimaryDateTimeValue ?? DateTime.Now.Date;
                        objectValueSecondary = property.PrimaryDateTimeValue?.TimeOfDay ?? DateTime.Now.TimeOfDay;
                    }
                    break;

                case PropertyValueTypeEnum.Time:
                    {
                        string value;
                        if (HasValue)
                        {
                            value = Value.UserValueTime;
                        }
                        else
                        {
                            value = property.PrimaryValue;
                        }

                        if (TimeSpan.TryParse(value, out var valueTimeSpan))
                            objectValueSecondary = valueTimeSpan;
                        else
                            objectValueSecondary = DateTime.Now.TimeOfDay;
                    }
                    break;
            }

            Entry = objectValue;
            TimeEntry = objectValueSecondary;
        }

        #endregion

        #region Implementation

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void WriteChanges()
        {
            switch (Property.ValueType)
            {
                case PropertyValueTypeEnum.String:
                    Value.UserValueString = (string)EntryInCorrectType;
                    break;
                case PropertyValueTypeEnum.Integer:
                    Value.UserValueInt = (int)EntryInCorrectType;
                    break;
                case PropertyValueTypeEnum.Decimal:
                    Value.UserValueDecimal = (decimal)EntryInCorrectType;
                    break;
                case PropertyValueTypeEnum.Date:
                    Value.UserValueDate = (DateTime)EntryInCorrectType;
                    break;
                case PropertyValueTypeEnum.DateTime:
                    Value.UserValueDate = ((DateTime)EntryInCorrectType);
                    break;
                case PropertyValueTypeEnum.Time:
                    Value.UserValueTime = ((TimeSpan)EntryInCorrectType).ToString(@"hh\:mm");
                    break;
            }
        }

        protected override bool Validate()
        {
            if (Entry == null && Property.ValueType != PropertyValueTypeEnum.Time)
                return false;

            switch (Property.ValueType)
            {
                case PropertyValueTypeEnum.String:
                    EntryInCorrectType = (string)Entry;
                    return true;

                case PropertyValueTypeEnum.Integer:
                    {
                        EntryInCorrectType = Convert.ToInt32(Entry);
                        return true;
                    }

                case PropertyValueTypeEnum.Decimal:
                    {
                        EntryInCorrectType = Convert.ToDecimal(Entry);
                        return true;
                    }

                case PropertyValueTypeEnum.Date:
                    EntryInCorrectType = (DateTime)Entry;
                    return true;

                case PropertyValueTypeEnum.DateTime:
                    Entry = ((DateTime)Entry).Date;
                    EntryInCorrectType = ((DateTime)Entry).Add(TimeEntry);
                    return true;

                case PropertyValueTypeEnum.Time:
                    EntryInCorrectType = TimeEntry;
                    return true;
            }

            return false;
        }

        #endregion
    }
}
