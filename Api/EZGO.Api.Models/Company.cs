using EZGO.Api.Models.Settings;
using System;
using System.Collections.Generic;
using System.Text;

namespace EZGO.Api.Models
{
    /// <summary>
    /// Company; Company object. Company is a digital representation of a physical location where all objects are linked to. 
    /// Database location: [companies_company]
    /// </summary>
    public class Company
    {
        #region - fields -
        /// <summary>
        /// Id; Primary key, in other variables and objects usually named as CompanyId. DB: [companies_company.id]
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ManagerId; ManagerId of the company. DB: [companies_company.manager_id]
        /// </summary>
        public int ManagerId { get; set; }
        /// <summary>
        /// ManagerNAme; Name of the manager based on ManagerId.
        /// </summary>
        public string ManagerName { get; set; }
        /// <summary>
        /// Description; Company Description; DB: [companies_company.description]
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Name; Company Name; DB: [companies_company.name]
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Picture; Company Picture. Uri part. DB: [companies_company.picture]
        /// </summary>
        public string Picture { get; set; }
        /// <summary>
        /// Shifts; List of shifts connected to a company. Based on the companies_shifts table. 
        /// </summary>
        public List<Shift> Shifts { get; set; }
        /// <summary>
        /// Settings; Settings with the companies, based on the companies_settings table.
        /// </summary>
        public List<SettingResourceItem> Settings { get; set; }
        /// <summary>
        /// List of connected users to this company.
        /// </summary>
        public List<UserProfile> Users { get; set; }
        /// <summary>
        /// CompanyHoldingSecurityGUID; security guid used for companies with a holding.
        /// </summary>
        public string HoldingCompanySecurityGUID { get; set; }
        /// <summary>
        /// HoldingId of holding connected to company
        /// </summary>
        public int? HoldingId { get; set; }
        /// <summary>
        /// Holding object connected to company
        /// </summary>
        public Holding Holding { get; set; }
        /// <summary>
        /// Holding unit ids for company (should also be available within the holding);
        /// </summary>
        public List<int> HoldingUnitIds { get; set; }
        /// <summary>
        /// HoldingUnits connected to company (can be one or more units)
        /// </summary>
        public List<HoldingUnit> HoldingUnits { get; set; }
        #endregion

        #region - constructor(s) -
        public Company()
        {

        }
        #endregion
    }
}
