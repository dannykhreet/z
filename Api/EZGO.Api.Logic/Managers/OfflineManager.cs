
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models.Basic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

//TODO sort methods, rename all append methods in same structure.

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// OfflineManager; Manager containing functionalities for specific offline usage of certain client apps. 
    /// </summary>
    public class OfflineManager : BaseManager<OfflineManager>, IOfflineManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        #endregion

        #region - constructor(s) -
        public OfflineManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper,ILogger<OfflineManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
        }
        #endregion

        /// <summary>
        /// GetMediaUriAsync; Get a list of media urls used for offline mode of the client apps.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <returns>List of media basic items.</returns>
        public async Task<List<MediaBasic>> GetMediaUriAsync(int companyId)
        {
            var output = new List<MediaBasic>();

            NpgsqlDataReader dr = null;

            try
            {
                var parameters = _manager.GetBaseParameters(companyId: companyId);

                using (dr = await _manager.GetDataReader("offline_media_images", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var mediaItem = new MediaBasic();
                        mediaItem.MediaType = dr["MediaType"].ToString();
                        mediaItem.MediaUri = dr["MediaUri"].ToString();
                        if(dr["ModifiedAt"] != DBNull.Value)
                        {
                            mediaItem.LastModifiedDate = Convert.ToDateTime(dr["ModifiedAt"]);
                        } else
                        {
                            mediaItem.LastModifiedDate = DateTime.Now;
                        }

                        output.Add(mediaItem);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("OfflineManager.GetMediaUriAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);


            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
