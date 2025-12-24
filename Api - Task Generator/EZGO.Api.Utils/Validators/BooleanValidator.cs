using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Validators
{
    public static class BooleanValidator
    {
        #region - messages -
        public const string MESSAGE_BOOLEAN_IS_NOT_VALID = "Boolean is not valid.";
        #endregion

        public static bool CheckValue(object value)
        {
            if(value !=null)
            {
                return value.ToString().ToLower() == "true" || value.ToString().ToLower() == "false" || value.ToString().ToLower() == "1" || value.ToString().ToLower() == "0";
            }
            return false;
        }
    }
}
