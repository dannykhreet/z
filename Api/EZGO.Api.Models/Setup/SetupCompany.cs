using System.Collections.Generic;

namespace EZGO.Api.Models.Setup
{
    /// <summary>
    /// SetupCompany; Setup company, used for creating a new company. Based on this object a basic company is setup with a primary administrator. 
    /// </summary>
    public class SetupCompany
    {
        public int? CompanyId { get; set; }
        public int? ManagerId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string TimeZone { get; set; }
        public string TierLevel { get; set; }
        public string ShiftDays { get; set; }
        public string ShiftStartTime { get; set; }
        public int? ShiftHours { get; set; }
        public int? ShiftMinutes { get; set; }
        public string PrimaryUserName { get; set; }
        public string PrimaryUserPassword { get; set; }
        public string PrimaryFirstName { get; set; }
        public string PrimaryLastName { get; set; }
        public string Locale { get; set; }
        public string Country { get; set; }
        public string Coords { get; set; }
        public bool EnableTaskGeneration { get; set; }
        public bool EnableDataWarehouse { get; set; }
        public bool EnableWorkInstructionChangesNotifications { get; set; }
        public bool EnableMatrixStandardScore { get; set; }
        public int? HoldingId { get; set; }
        public List<int> HoldingUnitIds { get; set; }
        public string HoldingCompanySecurityGUID { get; set; }


    }
}
