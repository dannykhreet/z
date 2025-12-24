using EZGO.Api.Models.SapPm;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Area; Area object, area is used as the tree navation where all checklists, tasks, audits and several other objects area linked to. 
    /// Database location: [companies_area].
    /// </summary>
    public class Area
    {
        #region - fields -
        /// <summary>
        /// CompanyId; Id of the company where action belongs to. DB: [companies_area.company_id] 
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as AreaId.  DB: [companies_area.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Level; Level based on number of levels deep; DB: [companies_area.level]
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// ParentId; ParentId references a parent AreaId. DB: [companies_area.parent_id]
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// Children; Collection of areas based on the ParentId
        /// </summary>
        public List<Area> Children { get; set; }
        /// <summary>
        /// Description; Description of the area; DB: [companies_area.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// FullDisplayName; Name of the area based on the full parent tree. 
        /// </summary>
        public string FullDisplayName { get; set; }
        /// <summary>
        /// FullDisplayIds; list of ids (areas) based on the FullDisplayName;
        /// </summary>
        public string FullDisplayIds { get; set; }
        /// <summary>
        /// Name; Name of area. DB: [companies_area.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Picture; Picture of area for use in apps. DB: [companies_area.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// SystemInformation; AreaSystemInformation contain a set of other area data that is stored in the database. But not used in the current app structure. (used in legacy apps/portals)
        /// </summary>
        public AreaSystemInformation SystemInformation { get; set; }

        public int? SapPmFunctionalLocationId { get; set; }
        public SapPmLocation SapPmLocation { get; set; }
        #endregion

        #region - constructor(s) -
        /// <summary>
        /// Area constructor.
        /// </summary>
        public Area()
        {
            Children = new List<Area>();
        }
        #endregion
    }
}
