using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Authentication
{
    /// <summary>
    /// MediaToken; Used for AWS S3 media services. 
    /// </summary>
    public class MediaToken
    {
        /// <summary>
        /// AccesskeyID (AWS S3 parameter)
        /// </summary>
        public string AccessKeyId { get; set; }
        /// <summary>
        /// Expiration (AWS S3 parameter)
        /// </summary>
        public DateTime? Expiration { get; set; }
        /// <summary>
        /// SecretAccessKey (AWS S3 parameter)
        /// </summary>
        public string SecretAccessKey { get; set; }
        /// <summary>
        /// SessionToken (AWS S3 parameter)
        /// </summary>
        public string SessionToken { get; set; }
    }
}
