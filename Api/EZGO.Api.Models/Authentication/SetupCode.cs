using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// SetupCode used for 2Factor authentication.
    /// </summary>
    public class SetupCode
    {
        /// <summary>
        /// Account; Account name of 2factor authentication.
        /// </summary>
        public string Account { get; internal set; }
        /// <summary>
        /// ManualEntryKey; Key used for validating. 
        /// </summary>
        public string ManualEntryKey { get; internal set; }
        /// <summary>
        /// Base64-encoded PNG image
        /// </summary>
        public string QrCodeSetupImageUrl { get; internal set; }
        /// <summary>
        /// SetupCode; Constructor.
        /// </summary>
        public SetupCode() { }
        /// <summary>
        /// SetupCode; Contructor. 
        /// </summary>
        /// <param name="account">Account name (see Account field)</param>
        /// <param name="manualEntryKey">ManualEntryKey (see ManualEntryKey field) </param>
        /// <param name="qrCodeSetupImageUrl">QrCodeSetupImageUrl (see QrCodeSetupImageUrl field)</param>
        public SetupCode(string account, string manualEntryKey, string qrCodeSetupImageUrl)
        {
            Account = account;
            ManualEntryKey = manualEntryKey;
            QrCodeSetupImageUrl = qrCodeSetupImageUrl;
        }
    }
}
