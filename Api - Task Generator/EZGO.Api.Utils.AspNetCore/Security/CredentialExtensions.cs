using EZGO.Api.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Security
{
    public static class CredentialExtensions
    {
        public static MediaToken ToMediaToken(this Amazon.SecurityToken.Model.Credentials credentials)
        {
            var output = new MediaToken();
            if(credentials != null)
            {
                output.SessionToken = credentials.SessionToken;
                output.AccessKeyId = credentials.AccessKeyId;
                output.SecretAccessKey = credentials.SecretAccessKey;
                output.Expiration = credentials.Expiration;
            }
            return output;

        }
    }
}
