using EZ.Connector.Init.Interfaces;
using EZ.Connector.SAP.Interfaces;
using EZ.Connector.Ultimo.Interfaces;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZ.Connector.Init.Managers
{
    /// <summary>
    /// ConnectorManager; Connection Manager for initiating all connectors;
    /// The InitConnector call (in this case for actions, but later these may be extended) will initiate all relevant Connectors to third party systems.
    /// A connector executes it main logic in a separate thread. For this reason most information will that will be needed needs to supplied with the Init method.
    /// To make sure that this will keep working correctly please make sure when extending or changing these methods that the way this is handled stays the same.
    /// </summary>
    public class ConnectorManager : IConnectorManager
    {
        #region - privates -
        private readonly IConfigurationHelper _confighelper;
        private readonly ISAPConnector _SAPConnector;
        private readonly IUltimoConnector _UltimoConnector;
        #endregion

        #region - contructors -
        public ConnectorManager(ISAPConnector sapConnector, IUltimoConnector ultimoConnector, IConfigurationHelper configurationHelper)
        {
            _confighelper = configurationHelper;
            _SAPConnector = sapConnector;
            _UltimoConnector = ultimoConnector;
        }
        #endregion

        #region - main init methods -
        /// <summary>
        /// InitConnectors; Init connector methods for actions. Call this methods on add (or maybe change) action parts in the controllers;
        /// Within this methods all other methods will be called that connect to other systems.
        /// </summary>
        /// <param name="companyId">CompanyId (based on company.id)</param>
        /// <param name="userId">UserId (based on user_profile.id)</param>
        /// <param name="action">Action containing all relevant data.</param>
        /// <returns>true/false. NOTE! specific handling of data is not done yet. Will be implemented later on when decided how to handle third party systems.</returns>
        public async Task<bool> InitConnectors(int companyId, int userId, ActionsAction action)
        {
            var output = true;

            var succesSAP = await InitSAPConnector(companyId: companyId, userId: userId, action: action); //TODO add output handling, currently not used.

            var succesUltimo = await InitUltimoConnector(companyId: companyId, userId: userId, action: action); //TODO add output handling, currently not used.

            return output;
        }
        #endregion

        #region - specific init methods for connectors -
        /// <summary>
        /// InitSAPConnector; Initiates SAP connector.
        /// </summary>
        /// <param name="companyId">CompanyId (based on company.id)</param>
        /// <param name="userId">UserId (based on user_profile.id)</param>
        /// <param name="action">Action containing all relevant data.</param>
        /// <returns>true/false. NOTE! specific handling of data is not done yet. Will be implemented later on when decided how to handle third party systems.</returns>
        public async Task<bool> InitSAPConnector(int companyId, int userId, ActionsAction action)
        {
            try
            {
                if (_confighelper.GetValueAsBool(EZGO.Api.Settings.Connectors.ENABLE_SAP_CONFIG_KEY))
                {
                    //check if the SAP connector is enabled and the company is active for using the SAP connector.
                    if (_SAPConnector.CheckCompanyForConnector(companyId: companyId))
                    {
                        //if so, get a clean new action from the database based on the changed/added action within this controller so all properties are up to date
                        //and load it of the SAPConnector.
                        await _SAPConnector.SendActionToSAPAsync(companyId: companyId, action: action);
                    }
                }

            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// InitUltimoConnector; Initiated Ultimo connector.
        /// </summary>
        /// <param name="companyId">CompanyId (based on company.id)</param>
        /// <param name="userId">UserId (based on user_profile.id)</param>
        /// <param name="action">Action containing all relevant data.</param>
        /// <returns>true/false. NOTE! specific handling of data is not done yet. Will be implemented later on when decided how to handle third party systems.</returns>
        public async Task<bool> InitUltimoConnector(int companyId, int userId, ActionsAction action)
        {
            try
            {
                if (_confighelper.GetValueAsBool(EZGO.Api.Settings.Connectors.ENABLE_ULTIMO_CONFIG_KEY))
                {
                    //check if action has to be sent to ultimo
                    if (action.SendToUltimo)
                    {
                        //if so, get a clean new action from the database based on the changed/added action within this controller so all properties are up to date
                        //and load it off to the UltimoConnector.
                        await _UltimoConnector.SendActionToUltimoAsync(companyId: companyId,
                                                                       action: action, userId: userId);
                    }
                }

            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
