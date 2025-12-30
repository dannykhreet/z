using EZGO.Api.Controllers.Base;
using EZGO.Api.Security.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace EZGO.Api.Controllers.General
{
    /// <summary>
    /// WellKnown, used for specific functionality within the android and ios app for direct linking to the app. 
    /// </summary>
    [Route(".well-known")]
    [ApiController]
    public class WellKnownController : BaseController<WellKnownController>
    {
        #region - properties -

        #endregion

        #region - constructor(s) -
        public WellKnownController(ILogger<WellKnownController> logger) : base(logger)
        {
        }
        #endregion

        #region - routes -
        [AllowAnonymous]
        [HttpGet]
        [Route("assetlinks.json")]
        public async Task<IActionResult> GetAssessLinks()
        {
            var outputString = new StringBuilder();

            outputString.AppendLine("[");
            outputString.AppendLine("  {");
            outputString.AppendLine("    \"relation\": [\"delegate_permission/common.handle_all_urls\"],");
            outputString.AppendLine("    \"target\": {");
            outputString.AppendLine("      \"namespace\": \"android_app\",");
            outputString.AppendLine("      \"package_name\": \"com.ezfactory.ezgo\",");
            outputString.AppendLine("      \"sha256_cert_fingerprints\":");
            outputString.AppendLine("        [\"B4:F9:78:C2:86:1C:27:2E:DF:F4:46:7F:96:68:83:CE:E6:B9:70:D7:92:3C:17:EA:E9:13:E3:07:33:26:55:3D\"]");
            outputString.AppendLine("    }");
            outputString.AppendLine("  }");
            outputString.AppendLine("]");

            await Task.CompletedTask;

            return StatusCode((int)HttpStatusCode.OK, outputString.ToString());
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("apple-app-site-association")]
        public async Task<IActionResult> GetAppleSiteAssociationJson()
        {
            var outputString = new StringBuilder();

            outputString.AppendLine("{");
            outputString.AppendLine("    \"applinks\": {");
            outputString.AppendLine("        \"apps\": [],");
            outputString.AppendLine("        \"details\": [");
            outputString.AppendLine("            {");
            outputString.AppendLine("                \"appID\": \"4G6PSRDZ3F.nl.ezfactory.ezgoxam\",");
            outputString.AppendLine("                \"paths\": [\"/shared/*\"]");
            outputString.AppendLine("            }");
            outputString.AppendLine("        ]");
            outputString.AppendLine("    }");
            outputString.AppendLine("}");

            await Task.CompletedTask;

            return StatusCode((int)HttpStatusCode.OK, outputString.ToString());
        }
        #endregion
    }
}
