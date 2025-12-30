using DocumentFormat.OpenXml.Spreadsheet;
using EEZGO.Api.Utils.Data;
using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Basic;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.SapPm;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    /// <summary>
    /// AreaBasicManager; The AreaBasicManager contains only logic for retrieving and setting AreaBasics.
    /// A area is used as a collection group. Areas are created with a company and mostly setup as a tree (e.g. parent -> child 1 -> child 1.1 etc).
    /// Most of the primary objects (Task/TaskTemplate, Checklist/ChecklistTemplate, Audit/AuditTemplate etc.) are directly linked to a area. 
    /// Area's are also the primary filters for displaying data in the client apps (users choose a specific area before actually using the other functionalities).
    /// Depending on the user, one are more areas are coupled to a user. These are treaded as 'rights', so a user has rights to view a certain area. 
    /// These rights are display only though. Seeing depending on which functionality a user is using they can view objects that are linked to areas where they don't have any rights to. 
    /// </summary>
    public class AreaBasicManager : BaseManager<AreaBasicManager>, IAreaBasicManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        #endregion

        #region - constructor(s) -
        public AreaBasicManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ILogger<AreaBasicManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
        }
        #endregion

        #region - public methods -


        /// <summary>
        /// GetAreasBasicByStartAreaAsync; Get a AreaBasic object based on a AreaId. Depending on the filter type a single area or a area tree is retrieved.
        /// Following stored procedures will be used for database data retrieval: "get_area_nodes_from_leaf_to_root" OR "get_area_nodes_from_root_to_leaf"
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <param name="areaId">AreaId (DB: companies_area.id)</param>
        /// <param name="areaFilterType">FilterAreaTypeEnum value. Possibilities: RecursiveLeafToRoot, RecursiveRootToLeaf, Single</param>
        /// <returns>List of Area objects.</returns>
        public async Task<List<AreaBasic>> GetAreasBasicByStartAreaAsync(int companyId, int areaId, FilterAreaTypeEnum areaFilterType, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var output = new List<AreaBasic>();

            NpgsqlDataReader dr = null;

            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_id", areaId));

                string sp = "get_area";
                if (areaFilterType == FilterAreaTypeEnum.RecursiveLeafToRoot) { sp = "get_area_nodes_from_leaf_to_root"; }
                if (areaFilterType == FilterAreaTypeEnum.RecursiveRootToLeaf) { sp = "get_area_nodes_from_root_to_leaf"; }

                using (dr = await _manager.GetDataReader(sp, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        var area = CreateOrFillAreaBasicFromReader(dr);
                        output.Add(area);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("AreaBasicManager.GetAreasBasicByStartAreaAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }
        #endregion

        #region - private methods - 

        /// <summary>
        /// CreateOrFillAreaBasicFromReader; creates and fills a AreaBasic object from a DataReader.
        /// NOTE! intended for use with the area stored procedures within the database.
        /// </summary>
        /// <param name="dr">DataReader containing the relevant data.</param>
        /// <param name="area">AreaBasic object that is going to be filled. If object is not supplied it will be created.</param>
        /// <returns>A filled AreaBasic object.</returns>
        private AreaBasic CreateOrFillAreaBasicFromReader(NpgsqlDataReader dr, AreaBasic area = null)
        {
            if (area == null) area = new AreaBasic();

            area.Id = Convert.ToInt32(dr["id"]);
            if (dr.HasColumn("parent_id") && dr["parent_id"] != DBNull.Value)
            {
                area.ParentId = Convert.ToInt32(dr["parent_id"]);
            }
            if (dr.HasColumn("area_name_full") && dr["area_name_full"] != DBNull.Value)
            {
                area.NamePath = dr["area_name_full"].ToString();
            }
            area.Name = dr["name"].ToString();

            return area;
        }

        #endregion

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion

    }
}
