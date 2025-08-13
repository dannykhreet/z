namespace EZGO.Maui.Core.Models.Authentication
{
    public enum AuthenticationMethod
    {
        /// <summary>
        /// Standard username-password method
        /// </summary>
        Credentials,

        /// <summary>
        /// Authenticate through Microsoft MSAL
        /// </summary>
        MSAL,
    }
}
