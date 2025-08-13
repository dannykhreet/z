using System;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Classes.ValidationRules
{
    public class ValidatableObject<T> : NotifyPropertyChanged, IValidatable<T>, IDisposable
    {
        public ValidatableObject()
        {
            Validations = new List<IValidationRule<T>>();
            ValidateDel += Validatate;
        }

        public ValidatableObject(T initialValue) : this()
        {
            Value = initialValue;
        }

        public List<IValidationRule<T>> Validations { get; private set; }

        public List<string> Errors { get; set; } = new List<string>();

        public bool IsValid { get; set; } = true;

        public Func<bool> ValidateDel { get; set; }

        public bool CleanOnChange { get; set; } = true;

        private T _value;
        public T Value
        {
            get => _value;
            set
            {
                _value = value;

                OnPropertyChanged(nameof(DescriptionIsEmpty));

                if (CleanOnChange)
                    IsValid = true;
            }
        }
        public bool DescriptionIsEmpty =>  string.IsNullOrWhiteSpace(Value?.ToString());
        public bool Validatate()
        {
            Errors.Clear();

            Errors = Validations.Where(v => !v.Check(Value)).Select(v => v.ValidationMessage).ToList();
            IsValid = !Errors.Any();

            return this.IsValid;
        }

        public override string ToString()
        {
            return $"{Value}";
        }

        public void Dispose()
        {
            ValidateDel -= Validatate;
            Validations.Clear();
            Validations = null;
            Errors.Clear();
            Errors = null;
        }
    }
}
