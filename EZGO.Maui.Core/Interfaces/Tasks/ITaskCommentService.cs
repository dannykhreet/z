using EZGO.Api.Models;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EZGO.Maui.Core.Interfaces.Tasks
{
    public interface ITaskCommentService : IDisposable
    {
        Task<bool> AddAsync(CommentModel model);

        Task<bool> AddChecklistOrAuditCommentAsync(CommentModel model);

        Task AddToChecklistAuditAsync(CommentModel model);

        Task<bool> ChangeAsync(CommentModel model);

        Task<bool> ChangeLocalForChecklistAuditAsync(CommentModel model);

        Task AddCommentsToTasksTaskAsync(TasksTask task, bool refreshIfNotFound);

        Task<List<CommentModel>> GetCommentsForTaskAsync(int taskId, bool refresh = false, List<CommentModel> allComments = null);

        Task<List<CommentModel>> GetCommentsForTaskTemplateAsync(int templateId, bool refresh = false);

        Task<List<CommentModel>> GetAllAsync(bool refresh = false, bool includeLocal = false, bool includeLocalForChecklistAudit = false, bool isFromSyncService = false);

        Task<List<CommentModel>> GetLocalCommentsAsync();

        Task<int> UploadLocalCommentsAsync();

        Task LoadCommentCountForTasksAsync(IEnumerable<BasicTaskModel> tasks, bool refresh = false);
    }
}
