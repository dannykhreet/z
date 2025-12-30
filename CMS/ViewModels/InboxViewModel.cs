using EZGO.Api.Models;
using EZGO.Api.Models.Tags;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using WebApp.Models.Task;

namespace WebApp.ViewModels
{
    public class InboxViewModel : BaseViewModel
    {
        public UserProfile CurrentUser { get; set; }
        public List<InboxItemViewModel> InboxItems { get; set; }
        public InboxViewModel()
        {
        }
    }
}
