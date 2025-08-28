using System;
namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class IsLenghtToLongValidationRule<T> : IValidationRule<T>
    {
        public IsLenghtToLongValidationRule(string validationMessage, int maxLenght)
        {
            ValidationMessage = validationMessage;
            _maxLenght = maxLenght;
        }

        private int _maxLenght;

        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            if (value == null)
            {
                return false;
            }

            var str = value as string;

            return str.Length <= _maxLenght;
        }
    }
}

