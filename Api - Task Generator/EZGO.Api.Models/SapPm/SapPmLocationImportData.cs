using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models.SapPm
{
    /// <summary>
    /// SapPmLocationExtended; Contains all info needed to construct json for the 'merge_functional_locations' SP
    /// </summary>
    public class SapPmLocationImportData
    {
        #region - fields -
        public string FunctionalLocation { get; set; }
        public string FunctionalLocationName { get; set; }
        public string SuperiorFunctionalLocation { get; set; }
        public string MaintenancePlant { get; set; }
        public string MaintenancePlannerGroup { get; set; }
        public string MaintenancePlanningPlant { get; set; }
        public string MainWorkCenter { get; set; }
        public string MainWorkCenterPlant { get; set; }
        // incoming: 25-9-2014
        // expected in json: DD.MM.YYYY 
        public string LastChangeDateTime { get; set; }
        //SP expects string, can be true or false
        public string FuncnlLocIsMarkedForDeletion { get; set; }
        #endregion
    }
}
