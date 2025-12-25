namespace WebApp.Logic
{
    /// <summary>
    /// API endpoint constants for Skills Matrix Legend
    /// Extends the Logic.Constants with Skills Matrix Legend specific endpoints
    /// </summary>
    public static class SkillMatrixLegendConstants
    {
        /// <summary>
        /// Base URL pattern for Skills Matrix Legend endpoints
        /// </summary>
        private const string BaseUrl = "/v1/company/{0}/skillmatrixlegend";

        /// <summary>
        /// GET: Get legend configuration for a company
        /// POST: Save legend configuration for a company
        /// DELETE: Delete legend configuration for a company
        /// Parameter: companyId
        /// </summary>
        public static string GetLegendConfiguration => BaseUrl;

        /// <summary>
        /// POST: Update a single legend item
        /// Parameter: companyId
        /// </summary>
        public static string UpdateLegendItem => BaseUrl + "/item";

        /// <summary>
        /// POST: Reset legend configuration to defaults
        /// Parameter: companyId
        /// </summary>
        public static string ResetLegendConfiguration => BaseUrl + "/reset";
    }
}
