using System.Text.RegularExpressions;
using EZGO.Maui.Core.Utils;

namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class PasswordValidationRule<T> : IValidationRule<T>
    {
        public PasswordValidationRule(string validationMessage)
        {
            ValidationMessage = validationMessage;
        }

        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            if (value == null) return false;

            var str = value as string;
            var reg = new Regex(CompanyFeatures.PasswordValidationRegEx);
            var matchresult = reg.IsMatch(str);

            return matchresult;
        }
    }
}
