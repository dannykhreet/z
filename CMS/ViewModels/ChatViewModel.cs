using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Logic;
using WebApp.Models.Action;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class ChatViewModel
    {
        public ChatViewModel() { }

        public string Locale { get; set; }

        public Dictionary<string, string> CmsLanguage { get; set; }

        public CommentsViewModel _CommentsViewModel { get; set; }

        public int UnviewedCount { get; set; }

        public int CurrentActionId { get; set; }

        public UserProfile CurrentUser { get; set; }

        public bool ActionIsResolved { get; set; }

        public ActionCommentModel Input => new ActionCommentModel { ActionId = CurrentActionId, UserId = CurrentUser.Id };
    }
}
