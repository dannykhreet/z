using EEZGO.Api.Utils.Data;
using EZGO.Api.Interfaces.Data;
using EZGO.Api.Interfaces.Managers;
using EZGO.Api.Interfaces.Settings;
using EZGO.Api.Interfaces.Utils;
using EZGO.Api.Logic.Base;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Stats;
using EZGO.Api.Models.Tags;
using EZGO.Api.Settings.Helpers;
using EZGO.Api.Utils.Data;
using EZGO.Api.Utils.Json;
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
    /// CommentManager; The CommentManager contains all logic for retrieving and setting Comments.
    /// Comments are used within the client apps to comment a issue, or make a specific statement. A comment is linked to a Task, Checklist Item or Audit Item.
    /// Comments are used more or less as a Action light, a comment does have a description and a comment and can contain 1 or more media items.
    /// NOTE! a comment is NOT the same as a Action Comment.
    /// </summary>
    public class CommentManager : BaseManager<CommentManager> ,ICommentManager
    {
        #region - properties -
        private string culture;
        public string Culture
        {
            get { return culture; }
            set { culture = _tagManager.Culture = value; }
        }
        #endregion

        #region - privates -
        private readonly IDatabaseAccessHelper _manager;
        private readonly ITagManager _tagManager;
        private readonly IDataAuditing _dataAuditing;
        private readonly IConfigurationHelper _configurationHelper;


        #endregion

        #region - constructor(s) -
        public CommentManager(IDatabaseAccessHelper manager, ITagManager tagManager, IDataAuditing dataAuditing, IConfigurationHelper configurationHelper, ILogger<CommentManager> logger) : base(logger)
        {
            _manager = manager;
            _tagManager = tagManager;
            _dataAuditing = dataAuditing;
            _configurationHelper = configurationHelper;
        }
        #endregion

        /// <summary>
        /// GetCommentsAsync; Gets a list of comments, based on the filters (if supplied). Depending on the include parameter this object structure will be extended with more data.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="filters">CommentFilters containing filter options to use with data retrieval</param>
        /// <param name="include">Include parameter, comma separated string, based on the includes enum. </param>
        /// <returns>List of comment items.</returns>
        public async Task<List<Comment>> GetCommentsAsync(int companyId, int? userId = null, CommentFilters? filters = null, string include = null)
        {
            var output = new List<Comment>();

            NpgsqlDataReader dr = null;

            try
            {
                var storedProcedure = "get_comments_v2";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                if (filters.HasValue)
                {
                    //filter text
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    //created from
                    if (filters.Value.CreatedFrom.HasValue && filters.Value.CreatedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdfrom", filters.Value.CreatedFrom.Value));
                    }
                    //created to
                    if (filters.Value.CreatedTo.HasValue && filters.Value.CreatedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdto", filters.Value.CreatedTo.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.UserId.HasValue && filters.Value.UserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId.Value));
                    }

                    if (filters.Value.TaskId.HasValue && filters.Value.TaskId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_taskid", filters.Value.TaskId.Value));
                    }

                    if (filters.Value.TaskTemplateId.HasValue && filters.Value.TaskTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tasktemplateid", filters.Value.TaskTemplateId.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                }
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        var comment = CreateOrFillCommentFromReader(dr);
                        output.Add(comment);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CommentManager.GetCommentsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            //handle includes
            if (!string.IsNullOrEmpty(include) && include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) output = await AppendTagsToCommentsAsync(companyId: companyId, comments: output);

            return output;
        }

        public async Task<CommentCountStatistics> GetCommentCountsAsync(int companyId, int? userId = null, CommentFilters? filters = null, string include = null)
        {
            var output = new CommentCountStatistics();

            NpgsqlDataReader dr = null;

            try
            {
                var storedProcedure = "get_comments_v2_counts";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                if (filters.HasValue)
                {
                    //filter text
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    //created from
                    if (filters.Value.CreatedFrom.HasValue && filters.Value.CreatedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdfrom", filters.Value.CreatedFrom.Value));
                    }
                    //created to
                    if (filters.Value.CreatedTo.HasValue && filters.Value.CreatedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdto", filters.Value.CreatedTo.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.UserId.HasValue && filters.Value.UserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId.Value));
                    }

                    if (filters.Value.TaskId.HasValue && filters.Value.TaskId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_taskid", filters.Value.TaskId.Value));
                    }

                    if (filters.Value.TaskTemplateId.HasValue && filters.Value.TaskTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tasktemplateid", filters.Value.TaskTemplateId.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                }
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        output.TotalCount = dr["total_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["total_count"]);
                        output.IsCreatedByMeCount = dr["is_created_by_me_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_created_by_me_count"]);
                        output.IsCreatedTodayCount = dr["is_created_today_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_created_today_count"]);
                        output.IsModifiedTodayCount = dr["is_modified_today_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_modified_today_count"]);
                        output.IsCommentedToday = dr["is_commented_today_count"] == DBNull.Value ? 0 : Convert.ToInt32(dr["is_commented_today_count"]);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CommentManager.GetCommentCountsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }


        public async Task<List<CommentRelation>> GetCommentsRelationsAsync(int companyId, int? userId = null, CommentFilters? filters = null, string include = null)
        {
            var output = new List<CommentRelation>();

            NpgsqlDataReader dr = null;

            try
            {
                var storedProcedure = "get_comments_v2_relations";

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

                if (filters.HasValue)
                {
                    //filter text
                    if (!string.IsNullOrEmpty(filters.Value.FilterText))
                    {
                        parameters.Add(new NpgsqlParameter("@_filtertext", filters.Value.FilterText));
                    }

                    //created from
                    if (filters.Value.CreatedFrom.HasValue && filters.Value.CreatedFrom.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdfrom", filters.Value.CreatedFrom.Value));
                    }
                    //created to
                    if (filters.Value.CreatedTo.HasValue && filters.Value.CreatedTo.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_createdto", filters.Value.CreatedTo.Value));
                    }

                    if (filters.Value.Timestamp.HasValue && filters.Value.Timestamp.Value > DateTime.MinValue)
                    {
                        parameters.Add(new NpgsqlParameter("@_timestamp", filters.Value.Timestamp.Value));
                    }

                    if (filters.Value.UserId.HasValue && filters.Value.UserId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_userid", filters.Value.UserId.Value));
                    }

                    if (filters.Value.TaskId.HasValue && filters.Value.TaskId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_taskid", filters.Value.TaskId.Value));
                    }

                    if (filters.Value.TaskTemplateId.HasValue && filters.Value.TaskTemplateId.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tasktemplateid", filters.Value.TaskTemplateId.Value));
                    }

                    if (filters.Value.TagIds != null && filters.Value.TagIds.Length > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_tagids", filters.Value.TagIds));
                    }

                    if (filters.Value.Limit.HasValue && filters.Value.Limit.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_limit", filters.Value.Limit.Value));
                    }

                    if (filters.Value.Offset.HasValue && filters.Value.Offset.Value > 0)
                    {
                        parameters.Add(new NpgsqlParameter("@_offset", filters.Value.Offset.Value));
                    }

                }
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));

                using (dr = await _manager.GetDataReader(storedProcedure, commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CommentRelation commentRelation = new CommentRelation();

                        commentRelation.CommentId = Convert.ToInt32(dr["comment_id"]);
                        if (dr["checklist_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["checklist_id"].ToString()))
                        {
                            commentRelation.ChecklistId = Convert.ToInt32(dr["checklist_id"]);
                        }
                        if (dr["audit_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["audit_id"].ToString()))
                        {
                            commentRelation.AuditId = Convert.ToInt32(dr["audit_id"]);
                        }
                        if (dr["task_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["task_id"].ToString()))
                        {
                            commentRelation.TaskId = Convert.ToInt32(dr["task_id"]);
                        }
                        if (dr["tasktemplate_id"] != DBNull.Value && !string.IsNullOrEmpty(dr["tasktemplate_id"].ToString()))
                        {
                            commentRelation.TaskTemplateId = Convert.ToInt32(dr["tasktemplate_id"]);
                        }

                        output.Add(commentRelation);
                    }
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CommentManager.GetCommentsAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            return output;
        }

        /// <summary>
        /// GetCommentAsync; Get a single comment item from the database.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="commentId">CommentId (DB: comments.id)</param>
        /// <returns>Returns a comment objects.</returns>
        public async Task<Comment> GetCommentAsync(int companyId, int commentId, string include = null)
        {
            var output = new Comment();

            NpgsqlDataReader dr = null;

            try
            {

                List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
                parameters.Add(new NpgsqlParameter("@_companyid", companyId));
                parameters.Add(new NpgsqlParameter("@_commentid", commentId));

                using (dr = await _manager.GetDataReader("get_comment", commandType: System.Data.CommandType.StoredProcedure, parameters: parameters))
                {
                    while (await dr.ReadAsync())
                    {
                        CreateOrFillCommentFromReader(dr, output);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(exception: ex, message: string.Concat("CommentManager.GetCommentAsync(): ", ex.Message));

                if (_configurationHelper.GetValueAsBool(Settings.ApiSettings.ENABLE_ELASTIC_SEARCH_IN_LOGIC_TRACE_CONFIG_KEY)) this.Exceptions.Add(ex);
            }
            finally
            {
                if (dr != null) { if (!dr.IsClosed) await dr.CloseAsync(); await dr.DisposeAsync(); }
            }

            //handle includes
            if (output.Id > 0 && !string.IsNullOrEmpty(include))
            {
                if (include.Split(",").Contains(IncludesEnum.Tags.ToString().ToLower())) output.Tags = await GetTagsWithCommentAsync(companyId: companyId, commentId: output.Id);
            }

            return output;
        }

        /// <summary>
        /// AddCommentAsync; Add comment to database based on comment object.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="comment">Comment object containing all data.</param>
        /// <returns>PossibleId (indentity, comments.id)</returns>
        public async Task<int> AddCommentAsync(int companyId, int userId, Comment comment)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromComment(comment: comment, companyId: companyId));

            var possibleId = Convert.ToInt32(await _manager.ExecuteScalarAsync("add_comment", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (possibleId > 0)
            {
                comment.Id = possibleId;
                await UpdateTagsForComment(companyId, userId, comment);

                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.comments.ToString(), possibleId);
                await _dataAuditing.WriteDataAudit(original: string.Empty, mutated: mutated, Models.Enumerations.TableNames.comments.ToString(), objectId: possibleId, userId: userId, companyId: companyId, description: "Added comment.");
            }

            return possibleId;
        }

        /// <summary>
        /// ChangeCommentAsync; Change an existing comment in db
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="commentId">CommentId (DB: comments.id)</param>
        /// <param name="comment">Comment object containing all data.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> ChangeCommentAsync(int companyId, int userId, int commentId, Comment comment)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.comments.ToString(), commentId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            parameters.AddRange(GetNpgsqlParametersFromComment(comment: comment, companyId: companyId));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("change_comment", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            await UpdateTagsForComment(companyId, userId, comment);

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.comments.ToString(), commentId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.comments.ToString(), objectId: commentId, userId: userId, companyId: companyId, description: "Changed comment.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// SetCommentActiveAsync; Set a comment active / inactive
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="userId">UserId (DB: profile_user.id)</param>
        /// <param name="commentId">CommentId (DB: comments.id)</param>
        /// <param name="isActive">true/false depending on if comment needs to be active or not.</param>
        /// <returns>true/false depending on outcome.</returns>
        public async Task<bool> SetCommentActiveAsync(int companyId, int userId, int commentId, bool isActive)
        {
            var original = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.comments.ToString(), commentId);

            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();
            parameters.Add(new NpgsqlParameter("@_companyid", companyId));
            parameters.Add(new NpgsqlParameter("@_id", commentId));
            parameters.Add(new NpgsqlParameter("@_active", isActive));

            var rowseffected = Convert.ToInt32(await _manager.ExecuteScalarAsync("set_comment_active", parameters: parameters, commandType: System.Data.CommandType.StoredProcedure));

            if (rowseffected > 0)
            {
                var mutated = await _manager.GetDataRowAsJson(Models.Enumerations.TableNames.comments.ToString(), commentId);
                await _dataAuditing.WriteDataAudit(original: original, mutated: mutated, Models.Enumerations.TableNames.comments.ToString(), objectId: commentId, userId: userId, companyId: companyId, description: "Changed comment active state.");
            }

            return (rowseffected > 0);
        }

        /// <summary>
        /// UpdateRelationsWithComment; Update the relations (tags) with a comment. Based on supplied data, tags will be removed or added.
        /// </summary>
        /// <param name="comment">The comment where the tags to be updated.</param>
        /// <returns>true/false depending on outcome.</returns>
        private async Task<bool> UpdateTagsForComment(int companyId, int userId, Comment comment)
        {
            comment.Tags ??= new();
            await _tagManager.UpdateTagsOnObjectAsync(ObjectTypeEnum.Comment, comment.Id, comment.Tags, companyId, userId);

            return true; //TODO when adding checks change this.
        }

        /// <summary>
        /// GetTagsWithCommentAsync; Get Tags with an Comment based on CommentId
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="commentId">commentId (DB: comments.id)</param>
        /// <returns>List of Tags.</returns>
        private async Task<List<Tag>> GetTagsWithCommentAsync(int companyId, int commentId)
        {
            var output = await _tagManager.GetTagsWithObjectAsync(companyId: companyId, objectType: ObjectTypeEnum.Comment, id: commentId);
            if (output != null && output.Count > 0)
            {
                return output;
            }
            return null;
        }

        /// <summary>
        /// AppendTagsToCommentsAsync; append tags to comment collections.
        /// </summary>
        /// <param name="companyId">CompanyId (DB: companies_company.id)</param>
        /// <param name="comments">Collection of comments</param>
        /// <returns>Collection of comments</returns>
        private async Task<List<Comment>> AppendTagsToCommentsAsync(int companyId, List<Comment> comments)
        {
            var allTagsOnComments = await _tagManager.GetTagRelationsByObjectTypeAsync(companyId: companyId, objectType: ObjectTypeEnum.Comment);
            if (allTagsOnComments != null)
            {
                foreach (var comment in comments)
                {
                    var tagsOnThisComment = allTagsOnComments.Where(t => t.ObjectId == comment.Id).ToList();
                    if (tagsOnThisComment != null && tagsOnThisComment.Count > 0)
                    {
                        comment.Tags ??= new List<Tag>();
                        comment.Tags.AddRange(tagsOnThisComment);
                    }

                }
            }

            return comments;
        }

        /// <summary>
        /// CreateOrFillCommentFromReader; Get comment objects from reader;
        /// </summary>
        /// <param name="dr">Reader containing data</param>
        /// <param name="comment">Comment object, if not supplied, will be created;</param>
        /// <returns>Filled comment object.</returns>
        private Comment CreateOrFillCommentFromReader(NpgsqlDataReader dr, Comment comment = null)
        {
            if (comment == null) comment = new Comment();

            comment.Id = Convert.ToInt32(dr["id"]);
            comment.CompanyId = Convert.ToInt32(dr["company_id"]);
            if (dr["description"] != DBNull.Value && !string.IsNullOrEmpty(dr["description"].ToString()))
            {
                comment.Description = dr["description"].ToString();
            }
            if(dr["comment_date"] != DBNull.Value)
            {
                comment.CommentDate = Convert.ToDateTime(dr["comment_date"]);
            }
            comment.CommentText = dr["comment"].ToString();
            if(dr["attachments"] != DBNull.Value && !string.IsNullOrEmpty(dr["attachments"].ToString()))
            {
                var attachmentsBasic = new List<string>();
                var attachmentsExtended = new List<Attachment>();
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    attachmentsBasic = dr["attachments"].ToString().ToObjectFromJson<List<string>>();
                }
                catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    attachmentsExtended = dr["attachments"].ToString().ToObjectFromJson<List<Attachment>>();
                }
                catch (Exception ex)
                {

                }
#pragma warning restore CS0168 // Variable is declared but never used
                if (attachmentsBasic != null && attachmentsBasic.Count > 0)
                {
                    comment.Attachments = attachmentsBasic;
                    comment.Media = attachmentsBasic.Select(a => new Attachment() { Uri = a }).ToList();
                }
                else if(attachmentsBasic != null && attachmentsExtended.Count > 0)
                {
                    comment.Media = attachmentsExtended;
                    comment.Attachments = attachmentsExtended.Select(a => a.Uri).ToList();
                }
            }
            if(dr.HasColumn("task_id") && dr["task_id"] != DBNull.Value)
            {
                comment.TaskId = Convert.ToInt32(dr["task_id"]);
            }
            if(dr.HasColumn("tasktemplate_id") && dr["tasktemplate_id"] != DBNull.Value)
            {
                comment.TaskTemplateId = Convert.ToInt32(dr["tasktemplate_id"]);
            }
            if (dr["user_id"] != DBNull.Value)
            {
                comment.UserId = Convert.ToInt32(dr["user_id"]);
            }
            if (dr.HasColumn("created_at"))
            {
                comment.CreatedAt = DateTime.SpecifyKind(Convert.ToDateTime(dr["created_at"]), DateTimeKind.Utc);
            }
            if (dr.HasColumn("modified_at"))
            {
                comment.ModifiedAt = DateTime.SpecifyKind(Convert.ToDateTime(dr["modified_at"] ), DateTimeKind.Utc);
            }
            if(dr.HasColumn("created_by"))
            {
                if(!string.IsNullOrEmpty(dr["created_by"].ToString()))
                {
                    comment.CreatedBy = dr["created_by"].ToString().Trim();
                }
            }

            return comment;
        }

        /// <summary>
        /// GetNpgsqlParametersFromComment; Get SQLParameters based on object.
        /// </summary>
        /// <param name="companyId">CompanyId (companies_company.id)</param>
        /// <param name="comment">Comment object, parameters will be based on fields of object.</param>
        /// <returns>List of NpgsqlParameter</returns>
        private List<NpgsqlParameter> GetNpgsqlParametersFromComment(int companyId, Comment comment)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            if (comment.Id > 0) parameters.Add(new NpgsqlParameter("@_id", comment.Id));

            if(!string.IsNullOrEmpty(comment.Description))
            {
                parameters.Add(new NpgsqlParameter("@_description", comment.Description));
            } else
            {
                parameters.Add(new NpgsqlParameter("@_description", DBNull.Value));
            }

            if (!string.IsNullOrEmpty(comment.CommentText))
            {
                parameters.Add(new NpgsqlParameter("@_comment", comment.CommentText));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_comment", DBNull.Value));
            }

            parameters.Add(new NpgsqlParameter("@_companyid", companyId));

            if(comment.UserId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_userid", comment.UserId));
            } else
            {
                parameters.Add(new NpgsqlParameter("@_userid", DBNull.Value));
            }

            if (comment.TaskId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_taskid", comment.TaskId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_taskid", DBNull.Value));
            }

            if (comment.TaskTemplateId.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_tasktemplateid", comment.TaskTemplateId));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_tasktemplateid", DBNull.Value));
            }

            if (comment.CommentDate.HasValue)
            {
                parameters.Add(new NpgsqlParameter("@_commentdate", new DateTime(comment.CommentDate.Value.Ticks)));
            } else
            {
                parameters.Add(new NpgsqlParameter("@_commentdate", DBNull.Value));
            }

            if (comment.Media != null && comment.Media.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_attachments", comment.Media.ToJsonFromObject()));
            }
            else if (comment.Attachments != null && comment.Attachments.Count > 0)
            {
                parameters.Add(new NpgsqlParameter("@_attachments", comment.Attachments.ToJsonFromObject()));
            }
            else
            {
                parameters.Add(new NpgsqlParameter("@_attachments", DBNull.Value));
            }

            return parameters;
        }

        #region - logging / error handling -
        public new List<Exception> GetPossibleExceptions()
        {
            var listEx = new List<Exception>();
            try
            {
                listEx.AddRange(this.Exceptions);
                listEx.AddRange(_tagManager.GetPossibleExceptions());
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
