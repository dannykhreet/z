using EZGO.Api.Models.Tags;
using System.Collections.Generic;
using WebApp.ViewModels;

namespace WebApp.Models.Tags
{
    public class TagGroupsModel : BaseViewModel
    {
        public List<TagGroup> TagGroups { get; set; }
    }
}
