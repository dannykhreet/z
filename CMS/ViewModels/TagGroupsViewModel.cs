using EZGO.Api.Models;
using EZGO.Api.Models.Tags;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class TagGroupsViewModel : BaseViewModel
    {
        public List<TagGroup> TagGroups { get; set; }
        public List<Tag> Tags { get; set; }
        public UserProfile CurrentUser { get; set; }
        public bool EnableTagGroupTranslation { get; set; }
        public TagGroupsViewModel()
        {
        }
    }
}
