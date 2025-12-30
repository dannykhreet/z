using EZGO.Api.Models;
using EZGO.Api.Models.Filters;
using EZGO.Api.Models.Relations;
using EZGO.Api.Models.Stats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EZGO.Api.Interfaces.Managers
{
    public interface ICommentManager
    {
        string Culture { get; set; }
        Task<List<Comment>> GetCommentsAsync(int companyId, int? userId = null, CommentFilters? filters = null, string include = null);
        Task<CommentCountStatistics> GetCommentCountsAsync(int companyId, int? userId = null, CommentFilters? filters = null, string include = null);
        Task<List<CommentRelation>> GetCommentsRelationsAsync(int companyId, int? userId = null, CommentFilters? filters = null, string include = null);
        Task<Comment> GetCommentAsync(int companyId, int commentId, string include = null);
        Task<int> AddCommentAsync(int companyId, int userId, Comment comment);
        Task<bool> ChangeCommentAsync(int companyId, int userId, int commentId, Comment comment);
        Task<bool> SetCommentActiveAsync(int companyId, int userId, int commentId, bool isActive);

        List<Exception> GetPossibleExceptions();
    }
}
