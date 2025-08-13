using System;
using System.Collections.Generic;
using System.Linq;
using EZGO.Api.Models.Tags;
using EZGO.Maui.Core.Models.Tags;

namespace EZGO.Maui.Core.Classes
{
    public static class TagsHelper
    {
        public static List<Tag> GetActiveTagsList(IEnumerable<TagModel> tags)
        {
            if (tags == null || !tags.Any())
                return new List<Tag>();

            var activeTagModels = tags.Concat(tags.SelectMany(t => t.SubTags)).Where(t => t.IsActive);
            var activeTags = activeTagModels.Select(x => x.ToTag()).ToList();
            return activeTags;
        }
    }
}
