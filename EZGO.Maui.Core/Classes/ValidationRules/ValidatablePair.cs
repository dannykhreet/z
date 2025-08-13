using System;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class ValidatablePair<T> : IDisposable
    {
        public ValidatablePair()
        {
            Validations = new List<IValidationRule<ValidatablePair<T>>>();
            ValidateDel += Validate;
        }

        public ValidatablePair(T initailValue1, T initialValue2) : this()
        {
            Item1.Value = initailValue1;
            Item2.Value = initialValue2;
        }

        public ValidatableObject<T> Item1 { get; set; } = new ValidatableObject<T>();

        public ValidatableObject<T> Item2 { get; set; } = new ValidatableObject<T>();

        public List<IValidationRule<ValidatablePair<T>>> Validations { get; }

        public List<string> Errors { get; set; }

        public bool IsValid { get; set; }

        public Func<bool> ValidateDel { get; set; }

        public bool Validate()
        {
            var result1 = Item1.Validatate();

            var result2 = Item2.Validatate();

            if(result1 && result2)
            {
                Errors = Validations.Where(v => !v.Check(this)).Select(v => v.ValidationMessage).ToList();

                IsValid = !Errors.Any();

                Item2.IsValid = IsValid;
                Item2.Errors.AddRange(Errors);
            }

            return (IsValid && Item1.IsValid && Item2.IsValid);
        }

        public void Dispose()
        {
            ValidateDel -= Validate;
            Validations.Clear();
            Errors.Clear();
            Errors = null;
            Item1.Dispose();
            Item2.Dispose();
            Item1 = null;
            Item2 = null;
        }
    }
}
