using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.PropertyValue;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Extensions;
using System;

namespace EZGO.Maui.Core.Models.Tasks.Properties
{
    /// <summary>
    /// Provides basic functionality for editing task properties.
    /// </summary>
    public abstract class BaseTaskPropertyEditViewModel : NotifyPropertyChanged
    {
        #region Protected Properties

        /// <summary>
        /// The underlying value for the property
        /// </summary>
        protected PropertyUserValue Value { get; private set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// The underlying property.
        /// </summary>
        public BasicTaskPropertyModel Property { get; private set; }

        /// <summary>
        /// Indicates if the max value informations is visible.
        /// </summary>
        public bool IsMaxVisible { get; private set; }

        /// <summary>
        /// Indicates if the min value informations is visible.
        /// </summary>
        public bool IsMinVisible { get; private set; }

        /// <summary>
        /// Maximum value for the property for displaying.
        /// </summary>
        public string MaxValue { get; private set; }

        /// <summary>
        /// Minimum value for the property for displaying.
        /// </summary>
        public string MinValue { get; private set; }

        /// <summary>
        /// Indicates if the value for the property exists
        /// </summary>
        public bool HasValue { get; private set; }

        /// <summary>
        /// Indicates if the model contains an error.
        /// </summary>
        public bool HasError { get; protected set; }

        /// <summary>
        /// The error message to be shown.
        /// </summary>
        public string ErrorMessage { get; protected set; }

        public string MinTextLabel { get; set; } = "Min: ";

        public string MaxTextLabel { get; set; } = "Max: ";

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new edit view model for a given task property with a given value.
        /// </summary>
        /// <param name="property">The property. Cannot be null.</param>
        protected BaseTaskPropertyEditViewModel(BasicTaskPropertyModel property)
        {
            Property = property;

            Value = property.UserValue;
            HasValue = Value != null;

            Value ??= new PropertyUserValue()
            {
                Id = 0,
                PropertyId = Property.PropertyId,
                TemplatePropertyId = Property.PropertyTemplateId,
            };

            Value.UserId = UserSettings.userSettingsPrefs.Id;
            Value.CompanyId = UserSettings.userSettingsPrefs.CompanyId;
            Value.ModifiedBy = UserSettings.userSettingsPrefs.Fullname;

            switch (Property.FieldType)
            {
                case PropertyFieldTypeEnum.Range:
                    IsMinVisible = !Property.PrimaryValue.IsNullOrEmpty();
                    IsMaxVisible = !Property.SecondaryValue.IsNullOrEmpty();
                    MinValue = Property.PrimaryValue;
                    MaxValue = Property.SecondaryValue;
                    break;
                case PropertyFieldTypeEnum.LowerLimit:
                    IsMinVisible = !Property.PrimaryValue.IsNullOrEmpty();
                    MinValue = Property.PrimaryValue;
                    MinTextLabel = ">";
                    break;
                case PropertyFieldTypeEnum.UpperLimit:
                    IsMaxVisible = !Property.PrimaryValue.IsNullOrEmpty();
                    MaxValue = Property.PrimaryValue;
                    MaxTextLabel = "<";
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Attempts to submit changes to the model.
        /// </summary>
        /// <returns><see langword="true"/> is successful, otherwise <see langword="false"/>.</returns>
        public bool TrySubmit()
        {
            // Clear the error message before calling Validate because eventual error message will be set inside this function
            ErrorMessage = string.Empty;

            // Check for errors
            var succeeded = Validate();

            // Only has error if not succeeded
            HasError = !succeeded;

            // If there are no errors
            if (succeeded)
            {
                WriteChanges();

                // And save the value
                Property.UserValue = Value;

                HasValue = true;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the value for the property.
        /// </summary>
        /// <returns>Property value or <see langword="null"/> if <see cref="HasValue"/> is <see langword="false"/>.</returns>
        public PropertyUserValue GetValue()
        {
            if (HasValue)
                return Value;

            return null;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Validates the model.
        /// </summary>
        /// <remarks>You can set <see cref="ErrorMessage"/> in this method.</remarks>
        /// <returns><see langword="true"/> if no errors, otherwise <see langword="false"/>.</returns>
        protected virtual bool Validate()
        {
            return true;
        }

        /// <summary>
        /// Writes changes to the underlying <see cref="Value"/> object.
        /// </summary>
        /// <remarks>This method will only be called if <see cref="Validate"/> returns <see langword="true"/></remarks>
        protected abstract void WriteChanges();

        #endregion

        #region Public Instantiation Methods


        /// <summary>
        /// Creates a specific editor based on a given task property type
        /// </summary>
        /// <param name="property">The property to create the editor for</param>
        /// <returns>Concrete editor for the given property type. If there are no editors available returns <see langword="null"/>.</returns>
        public static BaseTaskPropertyEditViewModel FromPropertyModel(BasicTaskPropertyModel property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            if (property.IsPlannedTimeProperty)
            {
                return new MeasureTimeTaskPropertyEditViewModel(property);
            }

            if (property.Type == PropertyTypeEnum.Input)
            {
                switch (property.ValueType)
                {
                    case PropertyValueTypeEnum.String:
                    case PropertyValueTypeEnum.Integer:
                    case PropertyValueTypeEnum.Decimal:
                    case PropertyValueTypeEnum.Date:
                    case PropertyValueTypeEnum.DateTime:
                    case PropertyValueTypeEnum.Time:
                        return new SingleEntryTaskPropertyEditViewModel(property);
                    case PropertyValueTypeEnum.Boolean:
                        break;
                }
            }

            return null;
        }

        #endregion
    }
}
