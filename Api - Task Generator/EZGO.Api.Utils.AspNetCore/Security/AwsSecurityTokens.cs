using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Models.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EZGO.Api.Utils.Security
{
    /// <summary>
    /// AwsSecurityTokenStore; Token store for several tokens that can be used within the AWS infra structure.
    /// </summary>
    public class AwsSecurityTokenStore : IAwsSecurityTokenStore
    {
        private readonly ILogger _logger;
        private readonly IConfigurationHelper _confighelper;

        #region - constructor(s) -
        public AwsSecurityTokenStore(IConfigurationHelper configurationHelper, ILogger<AwsSecurityTokenStore> logger)
        {
            _logger = logger;
            _confighelper = configurationHelper;
        }
        #endregion

        /// <summary>
        /// FetchMediaToken; Fetch media tokens for implementation in clients and API. 
        /// Media tokens are based on a assume-role functionality. 
        /// </summary>
        /// <returns></returns>
        public async Task<MediaToken> FetchMediaToken()
        {
            string awsAccessKeyId = _confighelper.GetValueAsString("AWSAssumeRoleConfig:AccessKey");
            string awsSecretAccessKey = _confighelper.GetValueAsString("AWSAssumeRoleConfig:SecretAccesskey");
            string roleARN = _confighelper.GetValueAsString("AWSAssumeRoleConfig:RoleArn");

            Amazon.SecurityToken.Model.AssumeRoleResponse result;
            System.Threading.CancellationToken token = new CancellationToken();
            using (Amazon.SecurityToken.AmazonSecurityTokenServiceClient client = new Amazon.SecurityToken.AmazonSecurityTokenServiceClient(awsAccessKeyId: awsAccessKeyId, awsSecretAccessKey: awsSecretAccessKey, region: Amazon.RegionEndpoint.EUCentral1))
            {
                Amazon.SecurityToken.Model.AssumeRoleRequest request = new Amazon.SecurityToken.Model.AssumeRoleRequest();
                request.RoleArn = roleARN;
                request.DurationSeconds = 900; //15 minutes
                request.RoleSessionName = "STSRole";
                result = await client.AssumeRoleAsync(request, token);
            }

            return result.Credentials.ToMediaToken();
        }
    }
}
