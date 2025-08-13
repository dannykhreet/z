using System.Collections.Generic;

namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public interface IValidatable<T>
    {
        List<IValidationRule<T>> Validations { get; }

        List<string> Errors { get; set; }

        bool Validatate();

        bool IsValid { get; set; }
    }
}