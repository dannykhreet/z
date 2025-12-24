using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EZGO.Api.Utils.Export
{
    /// <summary>
    /// LanguageResourceFileGenerator; Created language files for use as a resource (offline) within certain applications.
    /// </summary>
    public sealed class LanguageResourceFileGenerator
    {
        /// <summary>
        /// GenerateFileOutputIOS; Generate a language file for use within an IOS application.
        /// Generated output format:
        ///
        /// /*
        /// * Loco ios export: iOS Localizable.strings
        /// * Project: EZ-GO
        /// * Release: Working copy
        /// * Locale: de, Germen
        /// * Exported by: EZGO.API
        /// * Exported at: Tue, 12 Mar 2019 19:52:43 +0400
        /// */
        ///
        /// /* loco:592ec03352e1a16e248b4568 */
        /// "LOGIN_SCREEN_PASSWORD_TITLE" = "Passwort";
        ///
        /// /* loco:592ec04e52e1a15e248b4569 */
        /// "LOGIN_SCREEN_USERNAME_TITLE" = "Benutzername";
        ///
        /// /* loco:592ee40c52e1a1e2328b4567 */
        /// "LOGIN_SCREEN_LOGIN_BUTTON_TITLE" = "Login";
        ///
        /// /* loco:592ee44252e1a1d7328b4568 */
        /// "LOGIN_SCREEN_FORGOT_PASS_BUTTON_TITLE" = "Passwort vergessen?";
        ///
        /// </summary>
        /// <param name="languageResource">Language resource containing the information.</param>
        /// <returns>A string in the format of a file.</returns>
        public static string GenerateFileOutputIOS(LanguageResource languageResource)
        {
            var sb = new StringBuilder();

            sb.Append("/*\n");
            sb.Append("* Loco ios export: iOS Localizable.strings\n");
            sb.Append("* Project: EZ-GO\n");
            sb.AppendFormat("* Release: {0} \n", Assembly.GetEntryAssembly().GetName().Version);
            sb.AppendFormat("* Locale: {0}, {1}\n", languageResource.LanguageIso, languageResource.Language);
            sb.Append("* Exported by: EZGO.API\n");
            sb.AppendFormat("* Exported at: {0} {1}\n", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
            sb.Append("*/\n");

            foreach(var resource in languageResource.ResourceItems)
            {
                sb.Append("\n");
                sb.AppendFormat("/* loco:{0} */\n", resource.Guid);
                sb.AppendFormat("\"{0}\" = \"{1}\";\n", resource.ResourceKey.ToUpper(), resource.ResourceValue); //TODO probably add escapes for characters
            }

            return sb.ToString();
        }
    }
}
