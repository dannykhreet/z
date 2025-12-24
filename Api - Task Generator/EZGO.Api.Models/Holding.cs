using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Holding; Holding, structure for grouping companies and/or units. 
    /// </summary>
    public class Holding
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of holding
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Possible description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// SecurityGUID used for use when coupling companies (needed for validation)
        /// </summary>
        public string SecurityGUID { get; set; }
        public string SapPmNotificationOptions { get; set; }
        public string SapPmAuthorizationUrl { get; set; }
        public string SapPmFunctionalLocationUrl { get; set; }
        public string SapPmNotificationUrl { get; set; }
        public string SapPmTimezone { get; set; }
        /// <summary>
        /// Picture or logo of holding.
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// HoldingUnits; List of possible holding units. 
        /// </summary>
        public List<HoldingUnit> HoldingUnits { get; set; }
        /// <summary>
        /// CompanyRelations -> relation collection
        /// </summary>
        public List<CompanyRelationHolding> CompanyRelations { get; set; }

    }
}
