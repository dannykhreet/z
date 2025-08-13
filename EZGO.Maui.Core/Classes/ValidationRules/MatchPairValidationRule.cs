namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class MatchPairValidationRule<T> : IValidationRule<ValidatablePair<T>>
    {
        public MatchPairValidationRule(string validationMessage)
        {
            ValidationMessage = validationMessage;
        }

        public string ValidationMessage { get; set; }

        public bool Check(ValidatablePair<T> value)
        {
            var result = value.Item1.Value.Equals(value.Item2.Value);
            return result;
        }
    }
}
