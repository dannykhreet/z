using System;
using System.Collections.Generic;
using System.Linq;

namespace EZGO.Maui.Core.Classes
{
    public class EnumListItem<T> : IEquatable<EnumListItem<T>> where T : Enum
    {
        public string DisplayName { get; private set; }

        public T Value { get; private set; }

        public EnumListItem(T value, bool translate = true)
        {
            Value = value;
            DisplayName = translate ? EnumHelper.GetTranslation(value) : Value.ToString();
        }

        public static List<EnumListItem<T>> FromEnum(bool translate = true)
        {
            List<EnumListItem<T>> values = Enum.GetValues(typeof(T)).Cast<T>().Select(item => new EnumListItem<T>(item, translate)).ToList();

            return values;
        }

        public static List<EnumListItem<T>> FromEnumValues(IEnumerable<T> enumValues, bool translate = true)
        {
            List<EnumListItem<T>> values = enumValues.Select(item => new EnumListItem<T>(item, translate)).ToList();

            return values;
        }

        public bool Equals(EnumListItem<T> other)
        {
            return Value.Equals(other.Value);
        }
    }
}
