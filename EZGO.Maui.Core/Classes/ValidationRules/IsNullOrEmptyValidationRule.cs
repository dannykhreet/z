namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class IsNullOrEmptyValidationRule<T> : IValidationRule<T>
    {
        public IsNullOrEmptyValidationRule(string validationMessage)
        {
            ValidationMessage = validationMessage;
        }

        public string ValidationMessage { get; set; }

        public bool Check(T value)
        {
            if(value == null)
            {
                return false;
            }

            var str = value as string;
            return !string.IsNullOrEmpty(str);
        }
    }
}
