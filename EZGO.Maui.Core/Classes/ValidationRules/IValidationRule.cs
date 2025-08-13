namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public interface IValidationRule<T>
    {
        string ValidationMessage { get; set; }
        bool Check(T value);
    }
}