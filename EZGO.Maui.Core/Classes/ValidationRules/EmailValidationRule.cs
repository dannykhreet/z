using System;
using System.Text.RegularExpressions;

namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class EmailValidationRule<T> : IValidationRule<T>
    {
        public string ValidationMessage { get; set; }

        public EmailValidationRule(string validationMessage)
        {
            ValidationMessage = validationMessage;
        }

        private Regex EmailRegex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$");

        public bool Check(T value)
        {
            var content = value as string;

            if (string.IsNullOrEmpty(content)) return true;

            var result = EmailRegex.IsMatch(content);

            return result;
        }
    }
}
