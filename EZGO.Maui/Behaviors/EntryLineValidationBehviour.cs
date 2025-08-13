using System;
using EZGO.Maui.Behaviors;
using Syncfusion.Maui.Core;

namespace EZGO.Maui.Behaviors
{
    public class EntryLineValidationBehviour : BehaviorBase<SfTextInputLayout>
    {
        #region StaticFields

        public static readonly BindableProperty IsValidProperty = BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(EntryLineValidationBehviour), true, BindingMode.Default, null, (bindable, oldValue, newValue) => OnIsValidChanged(bindable, newValue));

        #endregion

        #region Properties

        public bool IsValid
        {
            get
            {
                return (bool)GetValue(IsValidProperty);
            }
            set
            {
                SetValue(IsValidProperty, value);
            }
        }

        #endregion

        #region StaticMethods

        private static void OnIsValidChanged(BindableObject bindable, object newValue)
        {
            if (bindable is EntryLineValidationBehviour IsValidBehavior &&
                 newValue is bool IsValid)
            {
                IsValidBehavior.AssociatedObject.Stroke = IsValid ? Colors.Black : Colors.Red;
                IsValidBehavior.AssociatedObject.Stroke = IsValid ? Color.FromArgb("#6E6E6E") : Colors.Red;
            }
        }

        #endregion
    }
}

