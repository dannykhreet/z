using System;
using System.Collections.Generic;
using System.Text;

namespace EZ.Connector.SAP.Models
{
    /// <summary>
    /// 
    /// Example for values:
    /// {
    ///     "AttachmentId" : "",
    ///     "UserCanBeNotified" : false,
    ///     "NotificationPhase" : "1",
    ///     "PlantSection" : "YOH",
    ///     "Completed" : false,
    ///     "Room" : "",
    ///     "ShortText" : "First automated test - EZ factory",
    ///     "NotificationTimezone" : "UTC",
    ///     "NotificationDate" : "\/Date(1633478400000)\/",
    ///     "NotificationTime" : "PT11H46M51S",
    ///     "TecObjNoLeadingZeros" : "000000000210100004",
    ///     "TechnicalObjectTypeDesc" : "Equipment",
    ///     "ReporterDisplay" : "",
    ///     "LastChangedTimestamp" : "\/Date(1633520984000)\/",
    ///     "TechnicalObjectType" : "EAMS_EQUI",
    ///     "Deleted" : false,
    ///     "Effect" : "",
    ///     "EffectText" : "",
    ///     "DateMonitor" : "Y",
    ///     "NotificationTimestamp" : "\/Date(1633520811000)\/",
    ///     "ReporterUserId" : "PGOSWAMI",
    ///     "TechnicalObjectNumber" : "210100004",
    ///     "TechnicalObjectDescription" : "Compressor Motor",
    ///     "NotificationType" : "M2",
    ///     "NotificationTypeText" : "Malfunction Report",
    ///     "Priority" : "2",
    ///     "PriorityType" : "PM",
    ///     "PriorityText" : "High",
    ///     "Location" : "",
    ///     "Reporter" : "PGOSWAMI",
    ///     "Subscribed" : false,
    ///     "SystemStatus" : "Outstanding",
    ///     "MediaUrl": ["imgurl"]
    /// }
    ///
    /// 
    /// 
    /// </summary>
    public class SAPAariniBTP
    {
        public string AttachmentId { get; set; }
        public bool UserCanBeNotified { get; set; }
        public string NotificationPhase { get; set; }
        public string PlantSection { get; set; }
        public bool Completed { get; set; }
        public string Room { get; set; }
        public string ShortText { get; set; }
        public string NotificationTimezone { get; set; }
        public string NotificationDate { get; set; }
        public string NotificationTime { get; set; }
        public string TecObjNoLeadingZeros { get; set; }
        public string TechnicalObjectTypeDesc { get; set; }
        public string ReporterDisplay { get; set; }
        public string LastChangedTimestamp { get; set; }
        public string TechnicalObjectType { get; set; }
        public bool Deleted { get; set; }
        public string Effect { get; set; }
        public string EffectText { get; set; }
        public string DateMonitor { get; set; }
        public string NotificationTimestamp { get; set; }
        public string ReporterUserId { get; set; }
        public string TechnicalObjectNumber { get; set; }
        public string TechnicalObjectDescription { get; set; }
        public string NotificationType { get; set; }
        public string NotificationTypeText { get; set; }
        public string Priority { get; set; }
        public string PriorityType { get; set; }
        public string PriorityText { get; set; }
        public string Location { get; set; }
        public string Reporter { get; set; }
        public bool Subscribed { get; set; }
        public string SystemStatus { get; set; }
        public List<string> MediaFiles { get; set; }
    }

    /// <summary>
    /// Container for the SAPAariniBTP object (needed for specific post)
    /// </summary>
    public class SAPAariniContainer
    {
        public SAPAariniBTP d { get; set; }
    }
}
