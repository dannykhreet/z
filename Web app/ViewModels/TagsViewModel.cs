using EZGO.Api.Models;
using EZGO.Api.Models.Tags;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class TagsViewModel : BaseViewModel
    {
        public int itemId { get; set; }
        public List<TagGroup> TagGroups { get; set; }
        public List<Tag> SelectedTags { get; set; }
        public TagsViewModel()
        {
        }
    }
}
