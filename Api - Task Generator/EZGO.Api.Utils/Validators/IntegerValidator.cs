using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Validators
{
    public static class IntegerValidator
    {
        #region - messages -
        public const string MESSAGE_INTEGER_IS_NOT_VALID = "Integer is not valid.";
        #endregion

        public static bool CheckValue(object value)
        {
            if (value != null)
            {
                return int.TryParse(value.ToString().ToLower(), out int output);
            }
            return false;
        }
    }
}
