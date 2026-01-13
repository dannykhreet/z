using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.General;
using EZGO.Api.Models.Search;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class SearchManager : BaseManager<SearchManager>, ISearchManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;

        #endregion

        #region - constructor(s) -
        public SearchManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, ILogger<SearchManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;

        }
        #endregion


        public async Task<List<SearchResult>> GetSearchResultAsync(int companyId, SearchTypeEnum searchType, int? userId = null, SearchFilters? filters = null, string include = null)
        {
            var output = new List<SearchResult>();

            NpgsqlDataReader dr = null;

 
            output.Add(new SearchResult() { Id = 0, Name = "Dummy Data", SearchType = searchType });


            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                if (userId.HasValue)
                {
                    parameters.Add(new NpgsqlParameter("@_userid", userId));
                }

                if (filters.HasValue)
                {
                    if (string.IsNullOrEmpty(filters.Value.SearchValue))
                    {
                        parameters.Add(new NpgsqlParameter("@_searchvalue", filters.Value.SearchValue));
                    }

                    if (filters.Value.AreaId.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_areaid", filters.Value.AreaId.Value));
                    }

                    if (filters.Value.AssignedToMe.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_assignedtome", filters.Value.AssignedToMe.Value));
                    }

                    if (filters.Value.HasChildren.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_haschildren", filters.Value.HasChildren.Value));
                    }

                    if (filters.Value.HasItemsAttached.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hasitemsattached", filters.Value.HasItemsAttached.Value));
                    }

                    if (filters.Value.HasPictureAttached.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_haspicturesattached", filters.Value.HasPictureAttached.Value));
                    }

                    if (filters.Value.HasSignedChildren.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hassignedchildren", filters.Value.HasSignedChildren.Value));
                    }

                    if (filters.Value.HasSubItemsAttached.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hassubitemsattached", filters.Value.HasSubItemsAttached.Value));
                    }

                    if (filters.Value.HasVideoAttached.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_hasvideoattached", filters.Value.HasVideoAttached.Value));
                    }

                    if (filters.Value.Limit.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.MyItems.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_myitems", filters.Value.MyItems.Value));
                    }

                    if (filters.Value.OffSet.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.OffSet.Value));
                    }

                    if (filters.Value.Roles != null && filters.Value.Roles.Count > 0)
                    {

                        parameters.Add(new NpgsqlParameter("@_roles", String.Join(",", filters.Value.Roles)));
                    }

                    if (filters.Value.Types != null && filters.Value.Types.Count > 0)
                    {

                        parameters.Add(new NpgsqlParameter("@_types", String.Join(",", filters.Value.Types)));
                    }

                    if (filters.Value.SortColumn.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_sortcolumn", filters.Value.SortColumn.Value.ToString().ToLower()));
                    }

                    if (filters.Value.SortDirection.HasValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_sortdirection", filters.Value.SortDirection.Value.ToString().ToLower()));
                    }

                }

                string storedProcedure = GetStoredProcedureBasedOnSearchType(searchType: searchType);

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var result = CreateOrFillSearchResultFromReader(dr); //to be implemented

                        output.Add(result);
                    }
                }
               
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("SearchManager.GetSearchResultAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }


            await Task.CompletedTask;

            return output;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchType"></param>
        /// <returns></returns>
        private string GetStoredProcedureBasedOnSearchType(SearchTypeEnum searchType)
        {
            var output = "search_all";

            switch (searchType)
            {
                case SearchTypeEnum.Actions: break;

                case SearchTypeEnum.Assessments: break;

                case SearchTypeEnum.AssessmentTemplates: break;

                case SearchTypeEnum.Audits: break;

                case SearchTypeEnum.AuditTemplates: break;

                case SearchTypeEnum.Checklists: break;

                case SearchTypeEnum.ChecklistTemplates: break;

                case SearchTypeEnum.Comments: break;

                case SearchTypeEnum.Matrices: break;

                case SearchTypeEnum.Tasks: break;

                case SearchTypeEnum.TaskTemplates: break;

                case SearchTypeEnum.Users: break;

                case SearchTypeEnum.WorkInstructions: break;

                case SearchTypeEnum.WorkInstructionTemplates: break;

                default: output = "search_all"; break; //default to all.
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private SearchResult CreateOrFillSearchResultFromReader(NpgsqlDataReader dr)
        {
            var searchResult = new SearchResult();
            searchResult.Id = Convert.ToInt32(dr["id"]);

            if (dr.HasColumn("area_path") && dr["area_path"] != DBNull.Value && !string.IsNullOrEmpty(dr["area_path"].ToString())) searchResult.AreaPath = dr["area_path"].ToString();
            if (dr.HasColumn("description") && dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString())) searchResult.Description = dr["description"].ToString();
            if (dr.HasColumn("name") && dr["name"] != DBNull.Value && !string.IsNullOrEmpty(dr["name"].ToString())) searchResult.Name = dr["name"].ToString();
            if (dr.HasColumn("picture") && dr["picture"] != DBNull.Value && !string.IsNullOrEmpty(dr["picture"].ToString())) searchResult.Picture = dr["picture"].ToString();
            if (dr.HasColumn("role") && dr["role"] != DBNull.Value && !string.IsNullOrEmpty(dr["role"].ToString())) searchResult.Role = dr["role"].ToString();
            if (dr.HasColumn("type") && dr["type"] != DBNull.Value && !string.IsNullOrEmpty(dr["type"].ToString())) searchResult.Type = dr["type"].ToString();
            if (dr.HasColumn("created_at") && dr["created_at"] != DBNull.Value) searchResult.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            if (dr.HasColumn("modified_at") && dr["modified_at"] != DBNull.Value) searchResult.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);

            //TODO add counts

            return searchResult;
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            return this.Exceptions;
        }
        #endregion
    }
}
