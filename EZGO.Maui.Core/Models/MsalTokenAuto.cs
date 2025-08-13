using System;
namespace EZGO.Maui.Core.Models
{
    public class MsalTokenAuto
    {
        public string UserName { get; private set; }
        public string AccessToken { get; private set; }
        public string IdToken { get; private set; }

        public MsalTokenAuto(string userName, string accessToken, string idToken)
        {
            UserName = userName;
            AccessToken = accessToken;
            IdToken = idToken;
        }
    }
}
