using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class ShouldOnlyContainsOneUppercaseLetterValidationRule : IValidationRule<string>
    {
        public ShouldOnlyContainsOneUppercaseLetterValidationRule(string validationMessage)
        {
            ValidationMessage = validationMessage;
        }

        public string ValidationMessage { get; set; }

        public bool Check(string value)
        {
            Regex regex = new Regex(@"^[\p{L}\p{M}]+$");
            return regex.IsMatch(value);
        }
    }
}
