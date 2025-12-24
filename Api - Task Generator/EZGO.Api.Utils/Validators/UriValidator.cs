using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Utils.Validators
{
    public static class UriValidator
    {
        public static bool MediaUrlPartIsValid(string uriPart)
        {
            if(uriPart.Contains("http"))
            {
                return Uri.IsWellFormedUriString(uriPart, UriKind.Absolute) && !uriPart.Contains("/var/") && !uriPart.Contains("/storage/") && !uriPart.Contains("blob:");
            } else
            {
                return Uri.IsWellFormedUriString(uriPart, UriKind.Relative) && !uriPart.Contains("/var/") && !uriPart.Contains("/storage/") && !uriPart.Contains("blob:");
            }
         
        }
    }
}
