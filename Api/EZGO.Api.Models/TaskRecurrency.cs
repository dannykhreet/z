using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// TaskRecurrency; Recurrency settings for a Task, usually also contains a full schedule.
    /// Database location: [tasks_taskrecurrency].
    /// </summary>
    public class TaskRecurrency
    {
        #region - fields -
        /// <summary>
        /// Primary key, in other variables and objects usually named as RecurrencyId or TaskRecurrencyId. DB: [tasks_taskrecurrency.id]  
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [tasks_taskrecurrency.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// ShiftId; Shift where the TaskRecurrency is directly connected to. Normally if shifts are needed the Shifts collection needs to be used.
        /// But if there is only one this property can also be filled.
        /// DB: [tasks_taskrecurrency.shift_id]  
        /// </summary>
        public int? ShiftId { get; set; }
        /// <summary>
        /// TemplateId; References the TaskTemplate where the TaskRecurrecy belongs to. DB: [tasks_taskrecurrency.template_id]
        /// </summary>
        public int TemplateId { get; set; }
        /// <summary>
        /// AreaId; TaskRecurrency is linked to this area. DB: [tasks_taskrecurrency.area_id]
        /// </summary>
        public int AreaId { get; set; }
        /// <summary>
        /// RecurrencyType; There are four recurrency types in the database.
        /// - no recurrency
        /// - shifts
        /// - month
        /// - week
        /// Depending on this type certain values will need to be filled within the schedule when posting data.
        /// When reading everything that is in the DB is returned.
        /// DB: [tasks_taskrecurrency.type]
        /// </summary>
        public string RecurrencyType { get; set; }
        /// <summary>
        /// Filled with last signed at date. This will only be set from a sign / status update of a task.
        /// </summary>
        public DateTime? LastSignedAt { get; set; }
        /// <summary>
        /// Schedule; Schedule are the date/timebase settings of a TaskRecurrency.
        /// Depending on type different properties need to be filled when submitting a new or changing a existing TaskRecurrency.
        /// </summary>
        public Schedule Schedule { get; set; }
        /// <summary>
        /// Shifts; Shifts are needed with a recurrency type SHIFTS and/or NO RECURRENCY (once in UI).
        /// </summary>
        public List<int> Shifts { get; set; }
        #endregion

        #region - constructor(s) -
        public TaskRecurrency()
        {

        }
        #endregion
    }
}
