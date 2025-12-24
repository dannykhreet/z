using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Marketplace;
using EZGO.Api.Models.Settings;
using EZGO.Api.Settings;
using EZGO.Api.Utils.BusinessValidators;
using EZGO.Api.Utils.Cache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// MarketPlaceManager; The MarketPlaceManager contains all logic for retrieving and setting market place items.
    /// The marketplace is used for specific company settings that a company can manage and are for use with external connectors.
    /// NOTE! most of this functionality is still stubbed and needs to be implemented properly.
    /// </summary>
    public class MarketPlaceManager : BaseManager<MarketPlaceManager>, IMarketPlaceManager
    {
        #region - privates -
        private readonly IGeneralManager _generalManager;
        #endregion

        #region - constructor(s) -
        public MarketPlaceManager(IGeneralManager generalManager, ILogger<MarketPlaceManager> logger) : base(logger)
        {
            _generalManager = generalManager;
        }
        #endregion

        #region - public methods -
        /// <summary>
        /// GetMarketPlace; Get the markent plance (list of marketplace items) for a company.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of MarketPlaceItems for a specific company.</returns>
        public async Task<List<MarketPlaceItem>> GetMarketPlace(int companyId) {
            await Task.CompletedTask;
            var output = new List<MarketPlaceItem>();


            var m = new MarketPlaceItem();

            m.Name = "Solvace";
            m.Description = "Solvace manages improvement opportunities around Visual Management and Problem Solving, Solvace Digitizes your Performance Management Process.";
            m.Picture = "/assets/img/apps/solvace.jpeg";
            m.Id = 1;
            m.SystemKey = "SOLVACE";
            m.ExternalUrl = "";
            m.Configuration = "";
            m.ConfigurationTemplate = "{\"SystemKey\": \"SOLVACE\",\"Version\": \"1.0\",\"Fields\": [{\"Title\": \"Solvace service url\",\"Description\" : \"Enter the full Solvace service url like [yourcompany].solvace.com.\",\"Type\": \"text\",\"PlaceHolder\": \"http://yourcompany.solvace.com\",\"OutputField\": \"url\"},{\"Title\": \"Solvace authorisation token\",\"Description\" : \"Enter the Solvace API authorization token.\",\"Type\": \"password\",\"PlaceHolder\": \"AAABBBCCCCDDDD\",\"OutputField\": \"token\"},{\"Title\": \"Token expirey date\",\"Description\" : \"Enter the token expire date.\",\"Type\": \"date\",\"OutputField\": \"expirydate\"},{\"Title\": \"Integration is activated\",\"Description\" : \"Set status of the integration.\",\"Type\": \"bool\",\"OutputField\": \"isenabled\"}]}";

            output.Add(m);

            var m2 = new MarketPlaceItem();

            m2.Name = "Ultimo";
            m2.Description = "Ultimo offers many rich functionalities as standard. For planning, monitoring, optimization and execution. For tracking all the required maintenance activities.";
            m2.Picture = "/assets/img/apps/ultimo.png";
            m2.Id = 2;
            m2.SystemKey = "ULTIMO";
            m2.ExternalUrl = "";
            m2.Configuration = "";
            m2.ConfigurationTemplate = "{\"SystemKey\": \"ULTIMO\",\"Version\": \"1.0\",\"Fields\": [{\"Title\": \"Ultimo service url\",\"Description\" : \"\",\"Type\": \"text\",\"OutputField\": \"url\"},{\"Title\": \"Ultimo authorisation token\",\"Description\" : \"\",\"Type\": \"password\",\"OutputField\": \"token\"},{\"Title\": \"Token expirey date\",\"Description\" : \"\",\"Type\": \"date\",\"OutputField\": \"expirydate\"},{\"Title\": \"Integration is activated\",\"Description\" : \"\",\"Type\": \"bool\",\"OutputField\": \"isenabled\"}]}";

            output.Add(m2);

            //get configuration.

            if( output.Any() && output.Count > 0)
            {

            }
            return output;
        }

        /// <summary>
        /// SaveMarketPlaceConfiguration; Save a configuration. 
        /// NOTE: NOT YET IMPLEMENTED
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="configuration">Configuration to be saved</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SaveMarketPlaceConfiguration(int companyId, string configuration)
        {
            await Task.CompletedTask;
            //encrypt configuration

            return true;
        }
        #endregion

        #region - private methods -
        /// <summary>
        /// GetConfigurationFromSettings; Get a list of configuration settings from the settings resources for a specific company.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A list of setting resources for use within the marketplace.</returns>
        private async Task<List<SettingResourceItem>> GetConfigurationFromSettings(int companyId)
        {
            List<SettingResourceItem> output = new List<SettingResourceItem>();

            output = await _generalManager.GetSettingResourceItemForCompany(companyid: companyId);

            return output;
        }

        /// <summary>
        /// AppendCompanyConfigurationToMarketPlace; Append specific company configuration to marketplace items.
        /// NOTE! NOT YET IMPLEMENTED.
        /// </summary>
        /// <param name="marketPlaceItems">List of items to be amended.</param>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>A list of marketplace items</returns>
        private async Task<List<MarketPlaceItem>> AppendCompanyConfigurationToMarketPlace(List<MarketPlaceItem> marketPlaceItems, int companyId)
        {
            var companySettings = await GetConfigurationFromSettings(companyId: companyId);

            return marketPlaceItems;
        }
        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {

            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_generalManager.GetPossibleExceptions());
            }
            catch (Exception ex)
            {
                //error occurs with errors, return only this error
                listEx.Add(ex);
            }
            return listEx;
        }
        #endregion
    }
}
