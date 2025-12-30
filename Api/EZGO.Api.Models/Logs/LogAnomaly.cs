using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.Logs
{
    /// <summary>
    /// LogAnomaly; Object used for anomaly logging. 
    /// Will include the following fields:
    /// A type: type of anomaly; CONNECTION, VALIDATION etc.
    /// A module: normally the module where the issue occurs CHECKLISTS, ACTIONS etc. or else GENERAL, if empty will be defaulted to UNKNOWN.
    /// A description: a small description what the anomaly is. 
    /// The data: containing the data where the anomaly occurs.
    /// A date: when the issue occurred.
    /// A number: if the issue occurred multiple times, this will be counted, but send once per send option
    /// </summary>
    public class LogAnomaly
    {
        public string AnomalyType { get; set; }
        public string Module { get; set; }
        public string Description { get; set; }
        public int? OccuranceNr { get; set; }
        public DateTime? LastOccuranceDate { get; set; }
        public DateTime? FirstOccuranceDate { get; set; }
        public object AnomalyData { get; set; }
    }
}
