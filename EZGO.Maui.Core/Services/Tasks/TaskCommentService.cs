using Autofac;
using EZGO.Api.Models;
using EZGO.Api.Models.Enumerations;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.File;
using EZGO.Maui.Core.Interfaces.Tasks;
using EZGO.Maui.Core.Interfaces.Utils;
using EZGO.Maui.Core.Models.Comments;
using EZGO.Maui.Core.Models.Tasks;
using EZGO.Maui.Core.Utils;
using Plugin.Media.Abstractions;
using System.Diagnostics;

namespace EZGO.Maui.Core.Services.Tasks
{
    public class TaskCommentService : ITaskCommentService
    {
        private const string LocalCommentsFileName = "localtaskcomments.json";
        private const string LocalChecklistAuditComments = "localChecklistAuditComments.json";
        private const int _commentsLimit = 10000;

        private readonly IApiRequestHandler _apiRequestHandler;
        private readonly IFileService _fileService;
        private readonly IMediaService _mediaService;

        public TaskCommentService(IApiRequestHandler apiRequestHandler, IMediaService mediaService)
        {
            _fileService = DependencyService.Get<IFileService>();
            _mediaService = mediaService;
            _apiRequestHandler = apiRequestHandler;
        }

        public async Task<bool> AddChecklistOrAuditCommentAsync(CommentModel model)
        {
            return await AddInternalAsync(model, false).ConfigureAwait(false);
        }

        public async Task<bool> AddAsync(CommentModel model)
        {
            return await AddInternalAsync(model, true, true).ConfigureAwait(false);
        }

        public async Task<List<CommentModel>> GetAllAsync(bool refresh = false, bool includeLocal = false, bool includeLocalForChecklistAudit = false, bool isFromSyncService = false)
        {
            var action = $"comments?limit={_commentsLimit}";

            if (CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                action += "&include=tags";

            var result = await _apiRequestHandler.HandleListRequest<Comment>(action, refresh, isFromSyncService).ConfigureAwait(false);

            await ClearUploadedChecklistAuditCommentsAsync().ConfigureAwait(false);

            var resultModels = result?.Select(x => x != null ? CommentModel.FromModel(x) : null).ToList() ?? new List<CommentModel>();

            if (includeLocal)
            {
                var localComments = await GetLocalCommentsInternalAsync(LocalCommentsFileName).ConfigureAwait(false);
                resultModels.AddRange(localComments);
            }
            if (includeLocalForChecklistAudit)
            {
                var localComments = await GetLocalCommentsInternalAsync(LocalChecklistAuditComments).ConfigureAwait(false);
                resultModels.AddRange(localComments);
            }

            return resultModels.Where(x => x != null).ToList();
        }

        public async Task<List<CommentModel>> GetCommentsForTaskAsync(int taskId, bool refresh = false, List<CommentModel> allComments = null)
        {
            var comments = allComments ?? await GetAllAsync(refresh, true, true).ConfigureAwait(false);
            var result = comments.Where(x => x.TaskId == taskId).ToList();
            result ??= new List<CommentModel>();
            return result;
        }

        public async Task<List<CommentModel>> GetCommentsForTaskTemplateAsync(int templateId, bool refresh = false)
        {
            var result = (await GetAllAsync(refresh, true, true).ConfigureAwait(false))
                .Where(x => x.TaskTemplateId == templateId)
                .ToList();

            result ??= new List<CommentModel>();

            return result;
        }

        public async Task<List<CommentModel>> GetLocalCommentsAsync()
        {
            return await GetLocalCommentsInternalAsync(LocalCommentsFileName).ConfigureAwait(false);
        }

        private async Task<List<CommentModel>> GetLocalCommentsInternalAsync(string filename)
        {
            var result = new List<CommentModel>();

            string localComments = await _fileService.ReadFromInternalStorageAsync(filename, Constants.PersistentDataDirectory).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(localComments))
                result = JsonSerializer.Deserialize<List<CommentModel>>(localComments) ?? new List<CommentModel>();

            return result;
        }

        public async Task LoadCommentCountForTasksAsync(IEnumerable<BasicTaskModel> tasks, bool refresh = false)
        {
            List<CommentModel> comments = await GetAllAsync(refresh: refresh).ConfigureAwait(false);
            foreach (var task in tasks)
            {
                var commentsCount = comments.Count(c => c.TaskId == task.Id);
                task.CommentCount = commentsCount;
            }
        }

        public async Task<int> UploadLocalCommentsAsync()
        {
            int count = 0;

            var localComments = await GetLocalCommentsInternalAsync(LocalCommentsFileName).ConfigureAwait(false);

            var Iterations = localComments.Count - 1;

            for (int i = Iterations; i >= 0; i--)
            {
                var comment = localComments[i];

                if (await InternetHelper.HasInternetConnection().ConfigureAwait(false))
                {
                    bool result = await AddInternalAsync(comment, false).ConfigureAwait(false);

                    if (result)
                    {
                        localComments.Remove(comment);

                        await SaveLocalCommentListAsync(localComments, LocalCommentsFileName).ConfigureAwait(false);

                        count++;
                    }
                }
                else
                    break;
            }

            return count;
        }

        private async Task SaveLocalCommentListAsync(List<CommentModel> comments, string fileName)
        {
            var loclaCommentsJson = JsonSerializer.Serialize(comments);
            await _fileService.SaveFileToInternalStorageAsync(loclaCommentsJson, fileName, Constants.PersistentDataDirectory).ConfigureAwait(false);
        }

        public async Task AddToChecklistAuditAsync(CommentModel model)
        {
            await AddLocalComment(model, LocalChecklistAuditComments).ConfigureAwait(false);
        }

        private async Task AddLocalComment(CommentModel model, string filename)
        {
            var localComments = await GetLocalCommentsInternalAsync(filename).ConfigureAwait(false);
            // Create an internal id
            model.InternalId = Guid.NewGuid().ToString("N");
            localComments.Add(model);
            await SaveLocalCommentListAsync(localComments, filename).ConfigureAwait(false);
        }

        private async Task<bool> AddInternalAsync(CommentModel model, bool saveToLocalIfFailed, bool isFromTask = false)
        {
            if (await InternetHelper.HasInternetAndApiConnectionAsync().ConfigureAwait(false))
            {
                try
                {
                    var commentApiModel = new Comment();

                    if (model.Attachments != null)
                    {
                        if (model.Attachments.Any())
                        {
                            // A bit of a hacky way of solving the issue of photos being taken offline.
                            // When you create a comment offline with a photo, media item containing that photo is saved to JSON locally.
                            // When you try to upload that photo later you will get a NullReferenceException when you try to execute GetStream on that media file,
                            // which is due to the fact that the stream getter is not saved to that JSON file since it is a private member of MediaFile.
                            // Therefore the line below recreates the media file using simple stream getter which does File.OpenRead 
                            // TODO improve the mechanics of offline pictures
                            model.Attachments.ForEach(x => x.MediaFile = new MediaFile(x.MediaFile.Path, () => File.OpenRead(x.MediaFile.Path), null, null));
                            await _mediaService.UploadMediaItemsAsync(model.Attachments, MediaStorageTypeEnum.Comments, 0).ConfigureAwait(false);
                            //commentApiModel.Attachments = model.Attachments.Where(x => !x.IsLocalFile && !x.IsVideo).Select(x => x.PictureUrl).ToList();
                            commentApiModel.Media = model.Attachments.Where(x => !x.IsLocalFile).Select(MediaItem.ToApiAttachment).ToList();
                        }
                    }

                    commentApiModel.UserId = model.UserId;
                    commentApiModel.CompanyId = model.CompanyId;
                    commentApiModel.TaskId = model.TaskId;
                    commentApiModel.TaskTemplateId = model.TaskTemplateId;
                    commentApiModel.CommentText = model.CommentText;
                    commentApiModel.CommentDate = model.CommentDate;
                    commentApiModel.Tags = model.Tags;

                    var result = await _apiRequestHandler.HandlePostRequest("comment/add", commentApiModel).ConfigureAwait(false);

                    var id = JsonSerializer.Deserialize<int>(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
                    model.Id = id;
                    return result.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    //Crashes.TrackError(ex);
                    Debug.WriteLine($"[TaskCommentServiceError:] {ex.Message}");
                    throw;
                }
            }
            else if (saveToLocalIfFailed)
            {
                if (isFromTask)
                    await AddLocalComment(model, LocalCommentsFileName).ConfigureAwait(false);
                else
                    await AddToChecklistAuditAsync(model).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public async Task<bool> ChangeLocalForChecklistAuditAsync(CommentModel model)
        {
            var localComments = await GetLocalCommentsInternalAsync(LocalChecklistAuditComments).ConfigureAwait(false);
            var localComment = localComments.Where(x => x.InternalId == model.InternalId).FirstOrDefault();
            if (localComment != null)
            {
                localComment.CommentText = model.CommentText;
                localComment.Attachments = model.Attachments;
                localComment.TaskId = model.TaskId;
                localComment.Id = model.Id;
                await SaveLocalCommentListAsync(localComments, LocalChecklistAuditComments).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public async Task AddCommentsToTasksTaskAsync(TasksTask task, bool refreshIfNotFound)
        {
            static List<Comment> Filter(List<CommentModel> source, long id)
            {
                return source
                    .Where(x => x.TaskId == id)
                    .Select(x => x.ToApiModel())
                    .ToList();
            }

            var comments = Filter(await GetAllAsync(refresh: false, includeLocal: false, includeLocalForChecklistAudit: true), task.Id);

            if (comments.Count == 0 && refreshIfNotFound)
            {
                comments = Filter(await GetAllAsync(refresh: true, includeLocal: false, includeLocalForChecklistAudit: true), task.Id);
            }

            task.Comments = comments;
        }

        private async Task ClearUploadedChecklistAuditCommentsAsync()
        {
            var localComments = await GetLocalCommentsInternalAsync(LocalChecklistAuditComments).ConfigureAwait(false);
            // Create an internal id
            var numberOfRemoved = localComments.RemoveAll(x => x.Id != 0);

            if (numberOfRemoved > 0)
                await SaveLocalCommentListAsync(localComments, LocalChecklistAuditComments).ConfigureAwait(false);
        }

        public async Task<bool> ChangeAsync(CommentModel model)
        {
            try
            {
                if (model.Id == 0)
                {
                    return await ChangeLocalForChecklistAuditAsync(model).ConfigureAwait(false);
                }
                else
                {
                    var url = $"comment/change/{model.Id}";
                    if (await InternetHelper.HasInternetAndApiConnectionAsync().ConfigureAwait(false))
                    {
                        var commentApiModel = new Comment();

                        if (model.Attachments != null)
                        {
                            await _mediaService.UploadMediaItemsAsync(model.Attachments, MediaStorageTypeEnum.Comments, 0).ConfigureAwait(false);
                            //commentApiModel.Attachments = model.Attachments.Where(x => !x.IsLocalFile && !x.IsVideo).Select(x => x.PictureUrl).ToList();
                            commentApiModel.Media = model.Attachments.Where(x => !x.IsLocalFile).Select(MediaItem.ToApiAttachment).ToList();
                        }

                        commentApiModel.Id = model.Id;
                        commentApiModel.UserId = model.UserId;
                        commentApiModel.CompanyId = model.CompanyId;
                        commentApiModel.TaskId = model.TaskId;
                        commentApiModel.TaskTemplateId = model.TaskTemplateId;
                        commentApiModel.CommentText = model.CommentText;
                        commentApiModel.CommentDate = model.CommentDate;
                        commentApiModel.Tags = model.Tags;

                        var result = await _apiRequestHandler.HandlePostRequest(url, commentApiModel).ConfigureAwait(false);
                        return result.IsSuccessStatusCode;
                    }
                    else
                    {
                        // TODO Offline cache of changes.
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //Crashes.TrackError(ex);
                Debug.WriteLine($"[TaskCommentServiceError:] {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            //_apiRequestHandler.Dispose();
            //_mediaService.Equals
            //_fileService.di
        }
    }
}
