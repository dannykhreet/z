using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Models.Tags;
using NodaTime;

namespace EZGO.Maui.Core.Interfaces.Tags
{
    public interface ITagsService
    {
        Task<List<TagModel>> GetTagsAsync(bool refresh = false, bool isFromSyncService = false);
        Task<List<TagModel>> GetTagModelsAsync(bool refresh = true, bool isFromSyncService = false, List<Tag> activeTags = null, TagableObjectEnum? tagableObjectEnum = null);
    }
}
