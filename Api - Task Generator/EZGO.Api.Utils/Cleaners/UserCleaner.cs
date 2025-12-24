using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Cleaners
{
    /// <summary>
    /// UserCleaner; User cleaner, cleans incorrect data from user data.
    /// </summary>
    public class UserCleaner
    {
        public static string CleanUserNameForDisplay(string username)
        {
            string output = "";
            if (string.IsNullOrEmpty(username))
            {
                output = "********";
            }
            else
            {
                if (username.Length < 6)
                {
                    output = string.Concat(username.Substring(0, 1), "******");
                }
                else
                {
                    output = string.Concat(username.Substring(0, 5), "******");
                }
            }

            return output;
        }
    }
}
