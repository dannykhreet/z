using EZGO.Api.Models.SapPm;
using EZGO.Api.Models.Tags;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApp.Models.Action;
using WebApp.Models.Comment;
using WebApp.Models.User;

namespace WebApp.ViewModels
{
    public class ActionViewModel : BaseViewModel
    {
        public List<ActionModel> ActionList { get; set; }
        public List<CommentModel> CommentList { get; set; }
        public ActionModel CurrentAction { get; set; }
        public CommentModel CurrentComment { get; set; }
        public ActionEditViewModel EditAction { get; set; }
        public int PreviousActionId { get; set; }
        public int NextActionId { get; set; }
        public ChatViewModel _ChatViewModel { get; set; }
        public UserProfile CurrentUser { get; set; }
        public int TaskId { get; set; }
        public TagsViewModel Tags { get; set; } = new TagsViewModel();
        
        public SapPmNotificationOptions NotificationOptions { get; set; }
        public SapPmLocation SelectedLocation { get; set; }
        public bool GoBack { get; set; }
        public bool UseTaskIdFiltering { get; set; }
        public List<SapPmLocation> SapPmFunctionalLocations { get; set; }
        public List<EZGO.Api.Models.ChecklistTemplate> ChecklistTemplates { get; set; }
        public List<EZGO.Api.Models.AuditTemplate> AuditTemplates { get; set; }
        public List<EZGO.Api.Models.TaskTemplate> TaskTemplates { get; set; }
        public ActionViewModel() { }
    }
}
