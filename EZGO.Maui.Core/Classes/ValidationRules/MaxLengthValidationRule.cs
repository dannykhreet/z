using System;
using System.Text;
using System.Text.RegularExpressions;

namespace EZGO.Maui.Core.Classes.ValidationRules
    {
    public class MaxLengthValidationRule<T> : IValidationRule<T>
    {
        private readonly int _maxLength;

        public MaxLengthValidationRule(int maxLength, string validationMessage)
        {
            _maxLength = maxLength;
            ValidationMessage = validationMessage;
        }

        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            var str = value as string;
            return str == null || str.Length <= _maxLength;
        }
    }
}
