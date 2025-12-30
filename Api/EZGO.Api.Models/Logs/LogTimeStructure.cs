using EZGO.Api.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Logs
{
    /// <summary>
    /// LogTimeStructure; Usage for data logging starting and ending times of a certain action around in app, cms or other.
    /// The item needs to be filled as completely as possible. So if started on creating a new actions, when done with submitting add the end date and the ActionId so it can be correctly found if needed.
    /// </summary>
    public class LogTimeStructure
    {
        /// <summary>
        /// Internal id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// StartDateTime of a certain action with a certain object.
        /// </summary>
        public DateTime? StartDateTimeUtc { get; set; }

        /// <summary>
        /// EndTime of a certain action with a certain object.
        /// </summary>
        public DateTime? EndDateTimeUtc { get; set; }

        /// <summary>
        /// UserId of user that is initiating the action.
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// CompanyId; CompanyId of the user/ connected user.
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// ObjectId (e.g. ActionId, AuditId, CompanyId etc.) of item where the action occurs.
        /// </summary>
        public int? ObjectId { get; set; }

        /// <summary>
        /// ObjectType (e.g. an Action, an Audit etc) where the action applies to.
        /// </summary>
        public ObjectTypeEnum? ObjectType { get; set; }

        /// <summary>
        /// ObjectTypeAction, action that is being executed, new, view, change etc.
        /// </summary>
        public ObjectTypeActionEnum? ObjectTypeAction { get; set; }

        /// <summary>
        /// Technical create date. (normally should fall around the end DateTime or the start DateTime if no end data time is used).
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
