using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Shift; Contains a shift, multiple shifts per day and company usually exists. Shifts can not have the same start and end dates when they are for the same AreaId.
    /// Database location: [companies_shift].
    /// </summary>
    public class Shift
    {
        #region - fields -
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as ShiftId. DB: [companies_shift.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Start: Start time of the shift; DB: [companies_shift.start]
        /// </summary>
        public string Start { get; set; }
        /// <summary>
        /// End: End time of the shift; DB: [companies_shift.end]
        /// </summary>
        public string End { get; set; }
        /// <summary>
        /// Day: Day number; DB: [companies_shift.day]
        /// shifts.day -> 1 t/m 7 -> Sunday ~ Saterday
        /// </summary>
        public int Day { get; set; }
        /// <summary>
        /// Weekday: Weekday number; DB: [companies_shift.weekday]
        /// shifts.weekday -> 0 t/m 6 -> Monday ~ Sunday
        /// </summary>
        public int Weekday { get; set; }
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [companies_shift.company_id] 
        /// </summary>
        public int? CompanyId { get; set; }
        /// <summary>
        /// AreaId; Shift is linked to this area through its template. DB: [companies_shift.area_id]
        /// </summary>
        public int? AreaId { get; set; }
        /// <summary>
        /// ShiftNr; Number of shift, represents the shift number on a specific day sorted by the start times of the shift.
        /// </summary>
        public int? ShiftNr { get; set; }
        #endregion

        #region - constructor(s) -
        public Shift()
        {

        }
        #endregion
    }
}
