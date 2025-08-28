using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EZGO.Api.Models.Enumerations;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Classes;
using EZGO.Maui.Core.Interfaces.ApiRequestHandlers;
using EZGO.Maui.Core.Interfaces.Tags;
using EZGO.Maui.Core.Models.Tags;

namespace EZGO.Maui.Core.Services.Tags
{
    public class TagsService : ITagsService
    {
        private readonly IApiRequestHandler _apiRequestHandler;

        public TagsService(IApiRequestHandler apiRequestHandler)
        {
            _apiRequestHandler = apiRequestHandler;
        }

        public async Task<List<TagModel>> GetTagsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                return new List<TagModel>();

            var uri = $"tags";
            var tags = await _apiRequestHandler.HandleListRequest<TagModel>(uri, refresh, isFromSyncService);

            return tags;
        }

        //currently not used
        public async Task<List<TagGroup>> GetTagGroupsAsync(bool refresh = false, bool isFromSyncService = false)
        {
            if (!CompanyFeatures.CompanyFeatSettings.TagsEnabled)
                return new List<TagGroup>();

            var uri = $"taggroups";
            var tagGroups = await _apiRequestHandler.HandleListRequest<TagGroup>(uri, refresh, isFromSyncService);

            return tagGroups;
        }

        public async Task<List<TagModel>> GetTagModelsAsync(bool refresh = true, bool isFromSyncService = false, List<Tag> activeTags = null, TagableObjectEnum? tagableObjectEnum = null)
        {
            var tags = await GetTagsAsync(refresh, isFromSyncService);

            if (activeTags?.Any() ?? false)
            {
                foreach (var activeTag in activeTags)
                {
                    var tag = tags.FirstOrDefault(t => t.Id == activeTag.Id);
                    if (tag != null)
                        tag.IsActive = true;
                }
            }

            var result = tags.GroupBy(t => t.GroupGuid, (key, tag) =>
            {
                var mainTag = tag.FirstOrDefault(x => x.IsSystemTag);
                var subTags = tag.Where(x => !x.IsSystemTag);
                if (tagableObjectEnum != null)
                    subTags = subTags.Where(x => x.AllowedOnObjectTypes != null && x.AllowedOnObjectTypes.Contains(tagableObjectEnum.Value));

                mainTag.SubTags = subTags.OrderBy(x => x.Id).ToList();
                return mainTag;
            }).OrderBy(x => x.Id).ToList();

            return result;
        }
    }
}
