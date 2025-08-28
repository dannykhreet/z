using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace EZGO.Maui.Core.Utils
{
    public class Statics
    {
        [Obsolete]
        public static HttpClient AppHttpClient => new HttpClient(new HttpDelegatingHandler());

        public static HttpClient SaveToCacheHttpClient => new HttpClient(new SaveToCacheDelegatingHandler());

        public static HttpClient RetrieveFromCacheHttpClient => new HttpClient(new RetrieveFromCacheDelegatingHandler());

        public static HttpClient AWSS3MediaHttpClient => new HttpClient(new AWSS3MediaDelegatingHandler());

        public static IDictionary<string, string> LanguageDictionary { get; set; } = new Dictionary<string, string>();
        public static IDictionary<string, string> DefaultLanguageDictionary { get; set; } = new Dictionary<string, string>();

        public static List<RoleTypeEnum> AppRoles = new List<RoleTypeEnum> { RoleTypeEnum.Basic, RoleTypeEnum.ShiftLeader, RoleTypeEnum.Manager };

        public static string ApiUrl { get; set; }

        public static bool SynchronizationRunning { get; set; }

        public static bool TaskSyncRunning { get; set; }

        public static bool AssessmentsSyncRunning { get; set; }

        public static bool EzFeedSyncRunning { get; set; }
    }
}
