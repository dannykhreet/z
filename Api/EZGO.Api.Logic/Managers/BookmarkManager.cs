using EZGO.Api.Data.Enumerations;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Logic.Managers
{
    public class BookmarkManager : BaseManager<BookmarkManager>, IBookmarkManager
    {
        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly IConfigurationHelper _configurationHelper;
        private readonly IUserAccessManager _userAccessManager;
        private readonly IDataAuditing _dataAuditing;
        #endregion

        #region - constructor(s) -
        public BookmarkManager(IDatabaseAccessHelper manager, IConfigurationHelper configurationHelper, IUserAccessManager userAccessManager, IDataAuditing dataAuditing, ILogger<BookmarkManager> logger) : base(logger)
        {
            _manager = manager;
            _configurationHelper = configurationHelper;
            _userAccessManager = userAccessManager;
            _dataAuditing = dataAuditing;
        }
        #endregion

        #region - public methods Bookmarks -
        public async Task<Bookmark> CreateOrRetrieveBookmarkAsync(int companyId, int userId, ObjectTypeEnum objectType, int objectId, BookmarkTypeEnum bookmarkType)
        {
            //try retrieve bookmark by object type, object id and bookmark type
            var bookmark = await GetBookmarkAsync(companyId, objectType, objectId, bookmarkType);
            
            //return bookmark if id > 0
            if (bookmark.Id > 0)
                return bookmark;

            //if bookmark doesnt exist create a new one
            var guid = Guid.NewGuid();
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_objecttype", (int)objectType));
            parameters.Add(new NpgsqlParameter("@_objectid", objectId));
            parameters.Add(new NpgsqlParameter("@_bookmarktype", (int)bookmarkType));
            parameters.Add(new NpgsqlParameter("@_guid", guid.ToString("N")));
            parameters.Add(new NpgsqlParameter("@_userid", userId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_bookmark", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));
            if (possibleId > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.bookmarks.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.bookmarks.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added bookmark.");
            }
            return await GetBookmarkAsync(companyId, guid, connectionKind: ConnectionKind.Writer);
        }

        public async Task<Bookmark> GetBookmarkAsync(int companyId, Guid guid, ConnectionKind connectionKind = ConnectionKind.Reader)
        {
            var bookmark = new Bookmark();
            NpgsqlDataReader dr = null;

            //try retrieve bookmark by guid
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_guid", guid.ToString("N")));

                using (dr = await _manager.GetDataReader("get_bookmark", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters, connectionKind: connectionKind))
                {
                    while (await dr.ReadAsync())
                    {
                        bookmark = CreateOrFillBookmarkFromReader(dr, bookmark: bookmark);
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("BookmarkManager.GetBookmarkAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            
            return bookmark;
        }

        public async Task<Bookmark> GetBookmarkAsync(int companyId, ObjectTypeEnum objectType, int objectId, BookmarkTypeEnum bookmarkType, ConnectionKind connectionKind = ConnectionKind.Writer)
        {
            var bookmark = new Bookmark();
            NpgsqlDataReader dr = null;

            //try retrieve bookmark by objecttype, objectid and bookmarktype
            try
            {
                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_objecttype", (int)objectType));
                parameters.Add(new NpgsqlParameter("@_objectid", objectId));
                parameters.Add(new NpgsqlParameter("@_bookmarktype", (int)bookmarkType));

                using (dr = await _manager.GetDataReader("get_bookmark", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        bookmark = CreateOrFillBookmarkFromReader(dr, bookmark: bookmark);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("BookmarkManager.GetBookmarkAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }

            return bookmark;
        }

        public async Task<bool> SetBookmarkActiveAsync(int companyId, int userId, Guid bookmarkGuid, bool isActive)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_guid", bookmarkGuid.ToString("N")));
            parameters.Add(new NpgsqlParameter("@_active", isActive));
            var rowsaffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_bookmark_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            return (rowsaffected > 0);
        }

        public async Task<bool> CheckBookmarkAllowedAreas(Bookmark bookmark, int companyId, int userId)
        {
            var ids = new List<int>();

            switch (bookmark.ObjectType)
            {
                case ObjectTypeEnum.AuditTemplate:
                    ids = await _userAccessManager.GetAllowedAuditTemplateIdsWithUserAsync(companyId, userId);
                    break;
                case ObjectTypeEnum.ChecklistTemplate:
                    ids = await _userAccessManager.GetAllowedChecklistTemplateIdsWithUserAsync(companyId, userId);
                    break;
                case ObjectTypeEnum.WorkInstructionTemplate:
                    ids = await _userAccessManager.GetAllowedWorkInstructionTemplateIdsWithUserAsync(companyId, userId);
                    break;
                default:
                    break;
            }

            return ids.Contains(bookmark.ObjectId);
        }
        #endregion

        #region - private methods Bookmarks -
        private Bookmark CreateOrFillBookmarkFromReader(NpgsqlDataReader dr, Bookmark bookmark)
        {
            if (bookmark == null) bookmark = new Bookmark();

            bookmark.Id = Convert.ToInt32(dr["id"]);
            bookmark.CompanyId = Convert.ToInt32(dr["company_id"]);
            bookmark.UserId = Convert.ToInt32(dr["user_id"]);
            bookmark.Guid = Guid.Parse(dr["guid"].ToString());

            if (Enum.TryParse(dr["bookmark_type"].ToString(), true, out BookmarkTypeEnum bookmarkType))
            {
                bookmark.BookmarkType = bookmarkType;
            }
            bookmark.BookmarkDate = Convert.ToDateTime(dr["bookmark_date"]);

            if (Enum.TryParse(dr["object_type"].ToString(), true, out ObjectTypeEnum objectType))
            {
                bookmark.ObjectType = objectType;
            }
            bookmark.ObjectId = Convert.ToInt32(dr["object_id"]);

            bookmark.ExtendedData = Convert.ToString(dr["extended_data"]);
            bookmark.CreatedAt = Convert.ToDateTime(dr["created_at"]);
            bookmark.ModifiedAt = Convert.ToDateTime(dr["modified_at"]);

            return bookmark;
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
