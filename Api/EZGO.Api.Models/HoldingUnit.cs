using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Relations;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// HoldingUnit is a unit under a holding. A holding unit can contain other units (tree-like) and one or more companies.
    /// </summary>
    public class HoldingUnit
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
        /// Picture or logo of holding.
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// HoldingId; Holding Id of the connected holding.
        /// </summary>
        public int HoldingId { get; set; }
        /// <summary>
        /// ParentId; ParentId references a parent Holding Unit (HoldingUnit.Id)
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// HoldingUnits; Holding unit children
        /// </summary>
        public List<HoldingUnit> HoldingUnits { get; set; }
        /// <summary>
        /// CompanyRelations -> relation collection
        /// </summary>
        public List<CompanyRelationHoldingUnit> CompanyRelations { get; set; }
    }
}
