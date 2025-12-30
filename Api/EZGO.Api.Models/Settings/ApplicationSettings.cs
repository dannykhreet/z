using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Settings
{
    /// <summary>
    /// ApplicationSettings; For use with local settings for the application.
    /// Currently the StorageLocation URLS and the Upload URL are located here, but this object will be expanded with multiple properties.
    /// </summary>
    public class ApplicationSettings
    {
        public string ImageStorageBaseUrl { get; set; }
        public string VideoStorageBaseUrl { get; set; }
        public string ImageUploadUrl { get; set; }
        public string VideoUploadUrl { get; set; }
        public List<string> AvailableLanguages { get; set; }
        /// <summary>
        /// ActiveApiVersion; Active API version (e.g. v1)
        /// Note depending on supplied customer version or user id this can be overridden for testing purposes or rolling out preview version.
        /// </summary>
        public string ActiveApiVersion { get; set; }
        public string RunningEnvironment { get; set; }
        public string PasswordValidationRegEx { get; set; }
        /// <summary>
        /// TestImageStorageBaseUrl; filled when running on development, to make overloads when testing.
        /// </summary>
        public string TestImageStorageBaseUrl { get; set; }
        /// <summary>
        /// AcceptanceImageStorageBaseUrl; filled when running on development, testing, acceptance, to make overloads when testing.
        /// </summary>
        public string AcceptanceImageStorageBaseUrl { get; set; }
        /// <summary>
        /// ProductionImageStorageBaseUrl; filled when running on development, testing, acceptance, to make overloads when testing.
        /// </summary>
        public string ProductionImageStorageBaseUrl { get; set; }
        /// <summary>
        /// ApiProductionUri; Url used for production;
        /// </summary>
        public string ApiProductionUri { get; set; }
        /// <summary>
        /// ApiAcceptanceUri; Url used for Acceptance; Only filled on development, test and staging
        /// </summary>
        public string ApiAcceptanceUri { get; set; }
        /// <summary>
        /// ApiTestUri; Url used for testing; Only filled on development, test and staging
        /// </summary>
        public string ApiTestUri { get; set; }
        /// <summary>
        ///
        /// </summary>
        public string ApiDevelopmentUri { get; set; }
        /// <summary>
        ///
        /// </summary>
        public Features Features { get; set; }
        /// <summary>
        /// Company timezone used by company (can be set in management portal)
        /// </summary>
        public string CompanyTimezone { get; set; }
        /// <summary>
        /// Limit to the amount of active tags a company can have (can be set globally and per company in management portal)
        /// </summary>
        public int TagLimit { get; set; }
        /// <summary>
        /// Limit to the amount of active tag groups a company can have (can be set globally and per company in management portal)
        /// </summary>
        public int TagGroupLimit { get; set; }
        /// <summary>
        /// Media locations used by application. 
        /// </summary>
        public MediaLocations MediaLocations { get; set; }
        /// <summary>
        /// AnalyticsLocationUri: Uri for sending analytics information.
        /// e.g. esapi.connect.ezfactory.nl/ingest
        /// </summary>
        public string AnalyticsLocationUri { get; set; }


    }
}
