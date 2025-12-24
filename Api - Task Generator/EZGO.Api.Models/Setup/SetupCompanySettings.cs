namespace EZGO.Api.Models.Setup
{
    /// <summary>
    /// Company; Company object. Company is a digital representation of a physical location where all objects are linked to. 
    /// Database location: [companies_company]
    /// </summary>
    public class SetupCompanySettings
    {
        #region - fields -

        public int? CompanyId { get; set; }
        public string TimeZone { get; set; }
        public string Locale { get; set; }
        public string TierLevel { get; set; }
        public string Country { get; set; }
        public string Coords { get; set; }
        public string MapsJson { get; set; }
        public bool EnableTaskGeneration { get; set; }
        public bool EnableDataWarehouse { get; set; }
        public bool EnableWorkInstructionChangesNotifications { get; set; }
        public bool EnableSkillsMatrixStandardRound { get; set; }
        public string SapPmCompanyId { get; set; }
        public string SapPmNotificationOptions { get; set; }
        public string SapPmAuthorizationUrl { get; set; }
        public string SapPmFunctionalLocationUrl { get; set; }
        public string SapPmNotificationUrl { get; set; }
        public string SapPmTimezone { get; set; }
        public bool EnableIpRestrictions { get; set; }
        public string IpRestrictionList { get; set; }
        public bool EnableVirtualTeamLead { get; set; }
        public string VirtualTeamLeadModules { get; set; }
        public bool EnableTranslations { get; set; }
        public string TranslationModules { get; set; }
        public string TranslationLanguages { get; set; }
        #endregion

        #region - constructor(s) -
        public SetupCompanySettings()
        {

        }
        #endregion
    }
}
